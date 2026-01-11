using API.Models;

namespace API.Data;

/// <summary>
/// Interface for providing market data (candles).
/// </summary>
public interface IMarketDataProvider
{
    /// <summary>
    /// Gets candlestick data for a specific symbol and timeframe.
    /// </summary>
    /// <param name="symbol">Stock symbol (e.g., "AAPL")</param>
    /// <param name="timeframe">Timeframe in minutes (1, 5, 15)</param>
    /// <param name="period">Time period (e.g., "1d", "5d", "1mo")</param>
    /// <returns>List of candles, sorted by time ascending</returns>
    Task<List<Candle>> GetCandlesAsync(string symbol, int timeframe, string period = "1d");
}
