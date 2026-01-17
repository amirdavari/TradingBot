namespace API.Models;

/// <summary>
/// Represents a paper trade (simulated trade) in the system.
/// Used for tracking performance of trading signals without real money.
/// </summary>
public class PaperTrade
{
    /// <summary>
    /// Unique identifier for the trade.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Stock symbol (e.g., AAPL, MSFT).
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Trade direction: LONG or SHORT.
    /// </summary>
    public string Direction { get; set; } = string.Empty;

    /// <summary>
    /// Entry price for the trade.
    /// </summary>
    public decimal EntryPrice { get; set; }

    /// <summary>
    /// Stop-Loss price level.
    /// </summary>
    public decimal StopLoss { get; set; }

    /// <summary>
    /// Take-Profit price level.
    /// </summary>
    public decimal TakeProfit { get; set; }

    /// <summary>
    /// Number of shares/units traded.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Confidence score (0-100) at trade entry.
    /// </summary>
    public int Confidence { get; set; }

    /// <summary>
    /// Reasons for entering the trade.
    /// </summary>
    public List<string> Reasons { get; set; } = new();

    /// <summary>
    /// Trade status: OPEN, CLOSED_TP, CLOSED_SL, CLOSED_MANUAL.
    /// </summary>
    public string Status { get; set; } = "OPEN";

    /// <summary>
    /// Exit price (null if trade is still open).
    /// </summary>
    public decimal? ExitPrice { get; set; }

    /// <summary>
    /// Profit or Loss in currency units (null if trade is still open).
    /// </summary>
    public decimal? PnL { get; set; }

    /// <summary>
    /// Profit or Loss in percentage (null if trade is still open).
    /// </summary>
    public decimal? PnLPercent { get; set; }

    /// <summary>
    /// Timestamp when the trade was opened.
    /// </summary>
    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Timestamp when the trade was closed (null if still open).
    /// </summary>
    public DateTime? ClosedAt { get; set; }

    /// <summary>
    /// Additional notes or remarks about the trade.
    /// </summary>
    public string? Notes { get; set; }
}
