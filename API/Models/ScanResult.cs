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
    /// Overall score (0-100) indicating daytrading suitability.
    /// </summary>
    public int Score { get; set; }

    /// <summary>
    /// Trend direction based on signal.
    /// </summary>
    public string Trend { get; set; } = "NONE"; // LONG | SHORT | NONE

    /// <summary>
    /// Volume status categorization.
    /// </summary>
    public string VolumeStatus { get; set; } = "LOW"; // LOW | MEDIUM | HIGH

    /// <summary>
    /// Indicates if relevant news are available for this symbol.
    /// </summary>
    public bool HasNews { get; set; }

    /// <summary>
    /// Confidence score (0-100).
    /// </summary>
    public int Confidence { get; set; }

    /// <summary>
    /// List of reasons why this stock was scored this way (max 3 for MVP).
    /// </summary>
    public List<string> Reasons { get; set; } = new();

    /// <summary>
    /// Current price of the symbol at scan time.
    /// </summary>
    public decimal CurrentPrice { get; set; }

    /// <summary>
    /// Indicates if there was an error scanning this symbol.
    /// </summary>
    public bool HasError { get; set; }

    /// <summary>
    /// Error message if HasError is true.
    /// </summary>
    public string? ErrorMessage { get; set; }
}
