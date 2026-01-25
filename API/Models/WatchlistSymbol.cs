namespace API.Models;

/// <summary>
/// Represents a symbol in the user's watchlist.
/// Persisted in database for MVP.
/// </summary>
public class WatchlistSymbol
{
    public int Id { get; set; }

    /// <summary>
    /// Stock symbol (e.g., "AAPL"). Always uppercase.
    /// </summary>
    public required string Symbol { get; set; }

    /// <summary>
    /// Company name (e.g., "Apple Inc.").
    /// </summary>
    public string? CompanyName { get; set; }

    /// <summary>
    /// Timestamp when symbol was added to watchlist.
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
