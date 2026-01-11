namespace API.Models;

/// <summary>
/// Represents a scanned stock candidate for daytrading.
/// </summary>
public class ScanResult
{
    /// <summary>
    /// Stock symbol (e.g., "AAPL").
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Current price.
    /// </summary>
    public decimal CurrentPrice { get; set; }

    /// <summary>
    /// Overall score (0-100) indicating daytrading suitability.
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Volume ratio compared to average (e.g., 1.5 = 150% of average).
    /// </summary>
    public double VolumeRatio { get; set; }

    /// <summary>
    /// Volatility percentage (ATR as % of price).
    /// </summary>
    public decimal Volatility { get; set; }

    /// <summary>
    /// Distance to VWAP as percentage.
    /// </summary>
    public decimal DistanceToVWAP { get; set; }

    /// <summary>
    /// Indicates if relevant news exists for this symbol.
    /// </summary>
    public bool HasNews { get; set; }

    /// <summary>
    /// List of reasons explaining the score.
    /// </summary>
    public List<string> Reasons { get; set; } = new();
}
