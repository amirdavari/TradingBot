namespace API.Models;

/// <summary>
/// Represents the current market mode state.
/// Simplified after removal of time-simulation replay feature.
/// </summary>
public class MarketModeState
{
    /// <summary>
    /// The current market mode (Live or Mock).
    /// </summary>
    public MarketMode Mode { get; set; } = MarketMode.Live;

    /// <summary>
    /// Current time (always UTC now, no simulation).
    /// </summary>
    public DateTime CurrentTime => DateTime.UtcNow;
}
