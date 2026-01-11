using API.Data;
using API.Models;

namespace API.Services;

/// <summary>
/// Service for scanning stocks and identifying daytrading candidates.
/// MVP: Simple scoring based on volume, volatility, and VWAP.
/// </summary>
public class ScannerService
{
    private readonly IMarketDataProvider _marketDataProvider;
    private readonly ILogger<ScannerService> _logger;

    // Default symbols to scan (can be configured later)
    private static readonly string[] DefaultSymbols =
    [
        "AAPL", "MSFT", "GOOGL", "AMZN", "TSLA",
        "META", "NVDA", "AMD", "NFLX", "SPY"
    ];

    public ScannerService(IMarketDataProvider marketDataProvider, ILogger<ScannerService> logger)
    {
        _marketDataProvider = marketDataProvider;
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

        // Filter out null results (errors) and sort by score
        results = scanResults
            .Where(r => r != null)
            .OrderByDescending(r => r!.Score)
            .ToList()!;

        _logger.LogInformation("Scan completed. Found {Count} candidates", results.Count);

        return results;
    }

    /// <summary>
    /// Scans a single symbol and calculates its daytrading score.
    /// </summary>
    private async Task<ScanResult?> ScanSymbolAsync(string symbol, int timeframe)
    {
        try
        {
            var candles = await _marketDataProvider.GetCandlesAsync(symbol, timeframe, "1d");

            if (candles.Count < 20)
            {
                _logger.LogWarning("Insufficient data for {Symbol}", symbol);
                return null;
            }

            var currentCandle = candles.Last();
            var currentPrice = currentCandle.Close;

            // Calculate indicators
            var vwap = CalculateVWAP(candles);
            var avgVolume = CalculateAverageVolume(candles);
            var atr = CalculateATR(candles, 14);

            var volumeRatio = (double)currentCandle.Volume / avgVolume;
            var volatility = (atr / currentPrice) * 100; // as percentage
            var distanceToVWAP = ((currentPrice - vwap) / vwap) * 100; // as percentage

            // Determine trend
            var trend = "NONE";
            if (currentPrice > vwap && volumeRatio > 0.5)
                trend = "LONG";
            else if (currentPrice < vwap && volumeRatio > 0.5)
                trend = "SHORT";

            // Determine volume status
            var volumeStatus = volumeRatio switch
            {
                > 1.5 => "HIGH",
                > 0.8 => "MEDIUM",
                _ => "LOW"
            };

            // Calculate score and confidence
            var score = CalculateScore(volumeRatio, (double)volatility, (double)Math.Abs(distanceToVWAP), false);
            var confidence = CalculateConfidence(volumeRatio, volatility);

            // Generate reasons
            var reasons = GenerateReasons(volumeRatio, (double)volatility, (double)Math.Abs(distanceToVWAP));

            return new ScanResult
            {
                Symbol = symbol,
                Score = score,
                Trend = trend,
                VolumeStatus = volumeStatus,
                HasNews = false, // MVP: News integration not yet implemented
                Confidence = confidence,
                Reasons = reasons
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scanning symbol {Symbol}", symbol);
            return null;
        }
    }

    private double CalculateConfidence(double volumeRatio, decimal volatility)
    {
        double confidence = 0.0;

        if (volumeRatio > 1.5) confidence += 0.4;
        else if (volumeRatio > 1.0) confidence += 0.3;
        else if (volumeRatio > 0.5) confidence += 0.2;

        if (volatility >= 1.5m && volatility <= 3.0m) confidence += 0.4;
        else if (volatility >= 1.0m && volatility <= 4.0m) confidence += 0.3;
        else if (volatility > 0.5m) confidence += 0.2;

        return Math.Clamp(confidence, 0.0, 1.0);
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
