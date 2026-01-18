namespace API.Models;

/// <summary>
/// Represents a paper trading account/depot.
/// Tracks balance, equity, and capital allocation.
/// </summary>
public class Account
{
    /// <summary>
    /// Unique identifier for the account.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Initial balance when account was created (e.g., 10,000 EUR).
    /// </summary>
    public decimal InitialBalance { get; set; }

    /// <summary>
    /// Current total balance (cash + value of open positions).
    /// </summary>
    public decimal Balance { get; set; }

    /// <summary>
    /// Current equity (total value including unrealized PnL).
    /// </summary>
    public decimal Equity { get; set; }

    /// <summary>
    /// Available cash for new trades (Balance - allocated capital in open positions).
    /// </summary>
    public decimal AvailableCash { get; set; }

    /// <summary>
    /// When the account was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Last update timestamp.
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
