namespace API.Models;

/// <summary>
/// Represents a trading signal with entry, stop-loss, and take-profit levels.
/// </summary>
public class TradeSignal
{
    /// <summary>
    /// Stock symbol (e.g., "AAPL").
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Trading direction: "LONG", "SHORT", or "NONE".
    /// </summary>
    public string Direction { get; set; } = "NONE";

    /// <summary>
    /// Suggested entry price.
    /// </summary>
    public decimal Entry { get; set; }

    /// <summary>
    /// Stop-loss price.
    /// </summary>
    public decimal StopLoss { get; set; }

    /// <summary>
    /// Take-profit price.
    /// </summary>
    public decimal TakeProfit { get; set; }

    /// <summary>
    /// Confidence score (0-100).
    /// </summary>
    public int Confidence { get; set; }

    /// <summary>
    /// List of reasons explaining the signal.
    /// </summary>
    public List<string> Reasons { get; set; } = new();
}
