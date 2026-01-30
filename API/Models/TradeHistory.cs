namespace API.Models;

/// <summary>
/// Represents a historical trade record for analytics and reporting.
/// Contains all trade data since closed trades are removed from PaperTrades.
/// </summary>
public class TradeHistory
{
    /// <summary>
    /// Unique identifier for the history record.
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
    /// Entry price.
    /// </summary>
    public decimal EntryPrice { get; set; }

    /// <summary>
    /// Exit price.
    /// </summary>
    public decimal ExitPrice { get; set; }

    /// <summary>
    /// Stop-Loss price level at entry.
    /// </summary>
    public decimal StopLoss { get; set; }

    /// <summary>
    /// Take-Profit price level at entry.
    /// </summary>
    public decimal TakeProfit { get; set; }

    /// <summary>
    /// Number of shares/units traded.
    /// </summary>
    public int Quantity { get; set; }

    /// <summary>
    /// Position size as decimal (supports fractional shares).
    /// </summary>
    public decimal PositionSize { get; set; }

    /// <summary>
    /// Total investment amount (PositionSize * EntryPrice).
    /// </summary>
    public decimal InvestAmount { get; set; }

    /// <summary>
    /// Profit or Loss in currency units.
    /// </summary>
    public decimal PnL { get; set; }

    /// <summary>
    /// Profit or Loss in percentage.
    /// </summary>
    public decimal PnLPercent { get; set; }

    /// <summary>
    /// Whether the trade was profitable.
    /// </summary>
    public bool IsWinner { get; set; }

    /// <summary>
    /// How the trade was closed: TP (Take Profit), SL (Stop Loss), MANUAL.
    /// </summary>
    public string ExitReason { get; set; } = string.Empty;

    /// <summary>
    /// Confidence score (0-100) at trade entry.
    /// </summary>
    public int Confidence { get; set; }

    /// <summary>
    /// Duration of the trade in minutes.
    /// </summary>
    public int DurationMinutes { get; set; }

    /// <summary>
    /// Timestamp when the trade was opened.
    /// </summary>
    public DateTime OpenedAt { get; set; }

    /// <summary>
    /// Timestamp when the trade was closed.
    /// </summary>
    public DateTime ClosedAt { get; set; }
}
