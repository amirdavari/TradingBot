namespace API.Models;

/// <summary>
/// Represents a single candlestick (OHLCV) for a specific timeframe.
/// </summary>
public class Candle
{
    /// <summary>
    /// Timestamp of the candle.
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    /// Opening price.
    /// </summary>
    public decimal Open { get; set; }

    /// <summary>
    /// Highest price.
    /// </summary>
    public decimal High { get; set; }

    /// <summary>
    /// Lowest price.
    /// </summary>
    public decimal Low { get; set; }

    /// <summary>
    /// Closing price.
    /// </summary>
    public decimal Close { get; set; }

    /// <summary>
    /// Trading volume.
    /// </summary>
    public long Volume { get; set; }
}
