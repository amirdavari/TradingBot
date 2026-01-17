using API.Models;
using API.Services;

namespace API.Data;

/// <summary>
/// Market data provider for Replay mode.
/// Wraps YahooFinanceMarketDataProvider and filters candles based on replay time.
/// </summary>
public class YahooReplayMarketDataProvider : IMarketDataProvider
{
    private readonly YahooFinanceMarketDataProvider _yahooProvider;
    private readonly IMarketTimeProvider _timeProvider;
    private readonly ILogger<YahooReplayMarketDataProvider> _logger;

    public YahooReplayMarketDataProvider(
        YahooFinanceMarketDataProvider yahooProvider,
        IMarketTimeProvider timeProvider,
        ILogger<YahooReplayMarketDataProvider> logger)
    {
        _yahooProvider = yahooProvider;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <summary>
    /// Gets candlestick data filtered by current replay time.
    /// Returns only candles where Candle.Time <= ReplayState.CurrentTime.
    /// </summary>
    public async Task<List<Candle>> GetCandlesAsync(string symbol, int timeframe, string period = "1d")
    {
        // Load all historical candles from Yahoo Finance
        var allCandles = await _yahooProvider.GetCandlesAsync(symbol, timeframe, period);

        // Get current replay time
        var currentTime = _timeProvider.GetCurrentTime();

        // Filter candles strictly: only return candles that have occurred by now in replay time
        var filteredCandles = allCandles
            .Where(c => c.Time <= currentTime)
            .ToList();

        _logger.LogInformation(
            "Replay mode: Filtered {FilteredCount} of {TotalCount} candles for {Symbol} (Replay time: {CurrentTime})",
            filteredCandles.Count, allCandles.Count, symbol, currentTime);

        return filteredCandles;
    }
}
