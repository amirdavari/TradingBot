namespace API.Models;

/// <summary>
/// Defines the market operation mode.
/// </summary>
public enum MarketMode
{
    /// <summary>
    /// Live market data from Yahoo Finance (delayed).
    /// </summary>
    Live,

    /// <summary>
    /// Mock data mode using generated/simulated market data.
    /// Historically called "Replay" - kept for database compatibility.
    /// </summary>
    Replay
}
