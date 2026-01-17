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
        // Get current replay time
        var currentTime = _timeProvider.GetCurrentTime();

        _logger.LogInformation(
            "Replay mode: Requesting {Period} of data for {Symbol} at timeframe {Timeframe}min (Current replay time: {CurrentTime})",
            period, symbol, timeframe, currentTime);

        // Load all historical candles from Yahoo Finance
        // The YahooFinanceMarketDataProvider will calculate the correct time range
        // based on the replay time and the requested period
        var allCandles = await _yahooProvider.GetCandlesAsync(symbol, timeframe, period);

        // Filter candles strictly: only return candles that have occurred by now in replay time
        var filteredCandles = allCandles
            .Where(c => c.Time <= currentTime)
            .ToList();

        var firstCandle = allCandles.Count > 0 ? allCandles.First().Time : (DateTime?)null;
        var lastCandle = allCandles.Count > 0 ? allCandles.Last().Time : (DateTime?)null;
        var firstFiltered = filteredCandles.Count > 0 ? filteredCandles.First().Time : (DateTime?)null;
        var lastFiltered = filteredCandles.Count > 0 ? filteredCandles.Last().Time : (DateTime?)null;

        _logger.LogInformation(
            "Replay mode: Filtered {FilteredCount} of {TotalCount} candles for {Symbol} | Replay time: {CurrentTime} | All data range: {FirstCandle} to {LastCandle} | Filtered range: {FirstFiltered} to {LastFiltered}",
            filteredCandles.Count, allCandles.Count, symbol, currentTime, firstCandle, lastCandle, firstFiltered, lastFiltered);

        return filteredCandles;
    }
}
