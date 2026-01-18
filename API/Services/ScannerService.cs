using API.Data;
using API.Models;

namespace API.Services;

/// <summary>
/// Service for scanning stocks and identifying daytrading candidates.
/// Includes volume, volatility, VWAP analysis, and news sentiment.
/// </summary>
public class ScannerService
{
    private readonly IMarketDataProvider _marketDataProvider;
    private readonly INewsProvider _newsProvider;
    private readonly SignalService _signalService;
    private readonly ILogger<ScannerService> _logger;

    // Default symbols to scan (can be configured later)
    private static readonly string[] DefaultSymbols =
    [
        "AAPL", "MSFT", "GOOGL", "AMZN", "TSLA",
        "META", "NVDA", "AMD", "NFLX", "SPY"
    ];

    public ScannerService(
        IMarketDataProvider marketDataProvider, 
        INewsProvider newsProvider,
        SignalService signalService,
        ILogger<ScannerService> logger)
    {
        _marketDataProvider = marketDataProvider;
        _newsProvider = newsProvider;
        _signalService = signalService;
        _logger = logger;
    }

    /// <summary>
    /// Scans multiple stocks and returns daytrading candidates sorted by score.
    /// </summary>
    public async Task<List<ScanResult>> ScanStocksAsync(string[]? symbols = null, int timeframe = 1)
    {
        symbols ??= DefaultSymbols;
        var results = new List<ScanResult>();

        _logger.LogInformation("Scanning {Count} symbols for daytrading candidates", symbols.Length);

        // Scan all symbols in parallel for better performance
        var scanTasks = symbols.Select(symbol => ScanSymbolAsync(symbol, timeframe));
        var scanResults = await Task.WhenAll(scanTasks);

        // All results are now returned (including errors), no filtering needed
        results = scanResults
            .OrderByDescending(r => r!.Score)
            .ToList()!;

        var errorCount = results.Count(r => r.HasError);
        if (errorCount > 0)
        {
            _logger.LogWarning("{ErrorCount} symbols had errors during scanning", errorCount);
        }

        _logger.LogInformation("Scan completed. Found {Count} candidates out of {Total} symbols ({Errors} with errors)", 
            results.Count, symbols.Length, errorCount);

        return results;
    }

    /// <summary>
    /// Scans a single symbol and calculates its daytrading score.
    /// Public for testing purposes.
    /// </summary>
    public async Task<ScanResult> ScanSymbolAsync(string symbol, int timeframe)
    {
        try
        {
            _logger.LogInformation("=== Scanning symbol: {Symbol} with timeframe {Timeframe} ===", symbol, timeframe);
            
            // Yahoo Finance limits: 1m=7days, 5m/15m=60days
            // Use appropriate periods to ensure sufficient candles (need at least 20)
            var period = timeframe switch
            {
                1 => "7d",   // 1-minute: Yahoo Finance limit is 7 days (max ~420 candles/day)
                5 => "5d",   // 5-minute: 5 days provides ~390 candles (78/day)
                15 => "5d",  // 15-minute: 5 days provides ~130 candles (26/day)
                _ => "5d"
            };
            
            _logger.LogInformation("Requesting period: {Period} for symbol {Symbol} (timeframe: {Timeframe}min)", period, symbol, timeframe);
            
            var candles = await _marketDataProvider.GetCandlesAsync(symbol, timeframe, period);

            _logger.LogInformation("Retrieved {Count} candles for {Symbol}", candles.Count, symbol);
            
            if (candles.Count > 0)
            {
                _logger.LogInformation("First candle: {FirstTime}, Last candle: {LastTime}", 
                    candles.First().Time, candles.Last().Time);
            }

            if (candles.Count < 10)
            {
                _logger.LogWarning("!!! INSUFFICIENT DATA for {Symbol}. Got {Count} candles, need at least 10 !!!", 
                    symbol, candles.Count);
                
                // Return error result instead of null to show symbol in UI with error
                return new ScanResult
                {
                    Symbol = symbol,
                    Score = 0,
                    Trend = "NONE",
                    VolumeStatus = "LOW",
                    HasNews = false,
                    Confidence = 0,
                    Reasons = new List<string>(),
                    HasError = true,
                    ErrorMessage = $"Insufficient data: Only {candles.Count} candles available (need at least 10)"
                };
            }

            _logger.LogInformation("✓ Successfully retrieved {Count} sufficient candles for {Symbol}", candles.Count, symbol);

            var currentCandle = candles.Last();
            var currentPrice = currentCandle.Close;

            // Calculate indicators
            var vwap = CalculateVWAP(candles);
            var avgVolume = CalculateAverageVolume(candles);
            var atr = CalculateATR(candles, 14);

            var volumeRatio = (double)currentCandle.Volume / avgVolume;
            var volatility = (atr / currentPrice) * 100; // as percentage
            var distanceToVWAP = ((currentPrice - vwap) / vwap) * 100; // as percentage

            // Determine volume status
            var volumeStatus = volumeRatio switch
            {
                > 1.5 => "HIGH",
                > 0.8 => "MEDIUM",
                _ => "LOW"
            };

            // Check for news
            var hasNews = await CheckForNewsAsync(symbol);

            // Get signal to extract confidence and trend
            var signal = await _signalService.GenerateSignalAsync(symbol, 5);

            // Calculate score
            var score = CalculateScore(volumeRatio, (double)volatility, (double)Math.Abs(distanceToVWAP), hasNews);

            // Generate reasons
            var reasons = GenerateReasons(volumeRatio, (double)volatility, (double)Math.Abs(distanceToVWAP));

            _logger.LogInformation("Successfully scanned {Symbol}: Score={Score}, Trend={Trend}, VolumeStatus={VolumeStatus}, Confidence={Confidence}", 
                symbol, score, signal.Direction, volumeStatus, signal.Confidence);

            return new ScanResult
            {
                Symbol = symbol,
                Score = score,
                Trend = signal.Direction,
                VolumeStatus = volumeStatus,
                HasNews = hasNews,
                Confidence = signal.Confidence,
                Reasons = reasons,
                HasError = false,
                ErrorMessage = null
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scanning symbol {Symbol}. Exception: {Message}", symbol, ex.Message);
            
            // Return error result instead of null
            return new ScanResult
            {
                Symbol = symbol,
                Score = 0,
                Trend = "NONE",
                VolumeStatus = "LOW",
                HasNews = false,
                Confidence = 0,
                Reasons = new List<string>(),
                HasError = true,
                ErrorMessage = $"Error: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Checks if there are recent news for a symbol.
    /// </summary>
    private async Task<bool> CheckForNewsAsync(string symbol)
    {
        try
        {
            _logger.LogInformation("Checking news for {Symbol}...", symbol);
            
            // Use Task.WhenAny for timeout without CancellationToken
            var newsTask = _newsProvider.GetNewsAsync(symbol, 5);
            var timeoutTask = Task.Delay(5000);
            
            var completedTask = await Task.WhenAny(newsTask, timeoutTask);
            
            if (completedTask == newsTask)
            {
                var news = await newsTask;
                _logger.LogInformation("Found {Count} news items for {Symbol}", news.Count, symbol);
                return news.Count > 0;
            }
            else
            {
                _logger.LogWarning("News check for {Symbol} timed out after 5 seconds", symbol);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to check news for {Symbol}", symbol);
            return false;
        }
    }

    private int CalculateScore(double volumeRatio, double volatility, double absDistance, bool hasNews)
    {
        int score = 0;

        if (volumeRatio > 2.0) score += 35;
        else if (volumeRatio > 1.5) score += 25;
        else if (volumeRatio > 1.2) score += 15;

        if (volatility >= 1.5 && volatility <= 3.0) score += 30;
        else if (volatility >= 1.0 && volatility <= 4.0) score += 20;
        else if (volatility > 0.5) score += 10;

        if (absDistance <= 0.5) score += 25;
        else if (absDistance <= 1.0) score += 20;
        else if (absDistance <= 2.0) score += 10;

        if (hasNews) score += 10;

        return Math.Clamp(score, 0, 100);
    }

    private List<string> GenerateReasons(double volumeRatio, double volatility, double absDistance)
    {
        var reasons = new List<string>();

        if (volumeRatio > 1.5) reasons.Add($"High volume ({volumeRatio:F1}x avg)");
        else if (volumeRatio > 1.0) reasons.Add($"Above avg volume ({volumeRatio:F1}x)");
        else reasons.Add($"Volume only {volumeRatio:F1}x average");

        if (volatility >= 1.5 && volatility <= 3.0) reasons.Add($"Good volatility ({volatility:F1}%)");
        else if (volatility > 3.0) reasons.Add($"High volatility ({volatility:F1}%)");
        else reasons.Add($"Low volatility ({volatility:F1}%)");

        if (absDistance > 1.0) reasons.Add($"Far from VWAP ({absDistance:F1}%)");

        return reasons.Take(3).ToList();
    }

    private decimal CalculateVWAP(List<Candle> candles)
    {
        decimal sumPriceVolume = 0;
        long sumVolume = 0;

        foreach (var candle in candles)
        {
            var typicalPrice = (candle.High + candle.Low + candle.Close) / 3;
            sumPriceVolume += typicalPrice * candle.Volume;
            sumVolume += candle.Volume;
        }

        return sumVolume > 0 ? sumPriceVolume / sumVolume : 0;
    }

    private double CalculateAverageVolume(List<Candle> candles)
    {
        return candles.Average(c => (double)c.Volume);
    }

    private decimal CalculateATR(List<Candle> candles, int period)
    {
        if (candles.Count < period + 1) return 0;

        var trueRanges = new List<decimal>();

        for (int i = 1; i < candles.Count; i++)
        {
            var high = candles[i].High;
            var low = candles[i].Low;
            var prevClose = candles[i - 1].Close;

            var tr = Math.Max(
                high - low,
                Math.Max(
                    Math.Abs(high - prevClose),
                    Math.Abs(low - prevClose)
                )
            );

            trueRanges.Add(tr);
        }

        return trueRanges.TakeLast(period).Average();
    }
}
