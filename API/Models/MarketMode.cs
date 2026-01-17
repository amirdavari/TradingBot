namespace API.Models;

/// <summary>
/// Defines the market operation mode.
/// </summary>
public enum MarketMode
{
    /// <summary>
    /// Live market data with real-time clock.
    /// Uses DateTime.UtcNow as time source.
    /// </summary>
    Live,

    /// <summary>
    /// Replay mode using historical data with simulated clock.
    /// Uses ReplayState.CurrentTime as time source.
    /// </summary>
    Replay
}
