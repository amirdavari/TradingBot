using API.Data;
using API.Models;

namespace API.Services;

/// <summary>
/// Service for scanning stocks and identifying daytrading candidates.
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
    public async Task<List<ScanResult>> ScanStocksAsync(string[]? symbols = null, int timeframe = 5)
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
            .OrderByDescending(r => r.Score)
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

            // Calculate score based on rules
            var scanResult = new ScanResult
            {
                Symbol = symbol,
                CurrentPrice = currentPrice,
                VolumeRatio = volumeRatio,
                Volatility = volatility,
                DistanceToVWAP = distanceToVWAP,
                HasNews = false, // TODO: Implement when NewsProvider is available
                Reasons = new List<string>()
            };

            scanResult.Score = CalculateScore(scanResult);
            GenerateReasons(scanResult);

            return scanResult;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scanning symbol {Symbol}", symbol);
            return null;
        }
    }

    /// <summary>
    /// Calculates daytrading score (0-100) based on multiple criteria.
    /// </summary>
    private int CalculateScore(ScanResult result)
    {
        int score = 0;

        // Volume Score (0-35 points)
        if (result.VolumeRatio > 2.0)
            score += 35;
        else if (result.VolumeRatio > 1.5)
            score += 25;
        else if (result.VolumeRatio > 1.2)
            score += 15;

        // Volatility Score (0-30 points)
        // Ideal volatility for daytrading: 1.5% - 3%
        if (result.Volatility >= 1.5m && result.Volatility <= 3.0m)
            score += 30;
        else if (result.Volatility >= 1.0m && result.Volatility <= 4.0m)
            score += 20;
        else if (result.Volatility > 0.5m)
            score += 10;

        // VWAP Proximity Score (0-25 points)
        // Best when price is near VWAP (within ±1%)
        var absDistance = Math.Abs(result.DistanceToVWAP);
        if (absDistance <= 0.5m)
            score += 25;
        else if (absDistance <= 1.0m)
            score += 20;
        else if (absDistance <= 2.0m)
            score += 10;

        // News Score (0-10 points)
        if (result.HasNews)
            score += 10;

        return Math.Clamp(score, 0, 100);
    }

    /// <summary>
    /// Generates human-readable reasons for the score.
    /// </summary>
    private void GenerateReasons(ScanResult result)
    {
        // Volume
        if (result.VolumeRatio > 1.5)
            result.Reasons.Add($"High volume: {result.VolumeRatio:F2}x average");
        else if (result.VolumeRatio > 1.2)
            result.Reasons.Add($"Above average volume: {result.VolumeRatio:F2}x");
        else
            result.Reasons.Add($"Low volume: {result.VolumeRatio:F2}x average");

        // Volatility
        if (result.Volatility >= 1.5m && result.Volatility <= 3.0m)
            result.Reasons.Add($"Ideal volatility: {result.Volatility:F2}%");
        else if (result.Volatility > 3.0m)
            result.Reasons.Add($"High volatility: {result.Volatility:F2}% (risky)");
        else
            result.Reasons.Add($"Low volatility: {result.Volatility:F2}%");

        // VWAP
        var absDistance = Math.Abs(result.DistanceToVWAP);
        if (absDistance <= 1.0m)
            result.Reasons.Add($"Price near VWAP: {result.DistanceToVWAP:F2}%");
        else if (result.DistanceToVWAP > 0)
            result.Reasons.Add($"Price above VWAP: +{result.DistanceToVWAP:F2}%");
        else
            result.Reasons.Add($"Price below VWAP: {result.DistanceToVWAP:F2}%");

        // Overall assessment
        if (result.Score >= 70)
            result.Reasons.Add("✓ Excellent daytrading candidate");
        else if (result.Score >= 50)
            result.Reasons.Add("✓ Good daytrading candidate");
        else if (result.Score >= 30)
            result.Reasons.Add("⚠ Moderate candidate");
        else
            result.Reasons.Add("✗ Poor daytrading candidate");
    }

    private decimal CalculateVWAP(List<Candle> candles)
    {
        decimal cumulativePriceVolume = 0;
        long cumulativeVolume = 0;

        foreach (var candle in candles)
        {
            var typicalPrice = (candle.High + candle.Low + candle.Close) / 3;
            cumulativePriceVolume += typicalPrice * candle.Volume;
            cumulativeVolume += candle.Volume;
        }

        return cumulativeVolume > 0 ? cumulativePriceVolume / cumulativeVolume : 0;
    }

    private double CalculateAverageVolume(List<Candle> candles)
    {
        if (candles.Count == 0) return 0;
        return candles.Average(c => (double)c.Volume);
    }

    private decimal CalculateATR(List<Candle> candles, int period)
    {
        if (candles.Count < period + 1) return 0;

        var trueRanges = new List<decimal>();

        for (int i = 1; i < candles.Count; i++)
        {
            var current = candles[i];
            var previous = candles[i - 1];

            var tr1 = current.High - current.Low;
            var tr2 = Math.Abs(current.High - previous.Close);
            var tr3 = Math.Abs(current.Low - previous.Close);

            var trueRange = Math.Max(tr1, Math.Max(tr2, tr3));
            trueRanges.Add(trueRange);
        }

        var recentTRs = trueRanges.TakeLast(period).ToList();
        return recentTRs.Average();
    }
}
