namespace API.Models;

/// <summary>
/// Database entity for persisting market mode state (Live vs Mock).
/// Simplified after removal of time-simulation replay feature.
/// </summary>
public class MarketModeEntity
{
    /// <summary>
    /// Singleton ID - always 1 to ensure only one state exists.
    /// </summary>
    public int Id { get; set; } = 1;

    /// <summary>
    /// The current market mode (Live or Mock).
    /// </summary>
    public MarketMode Mode { get; set; } = MarketMode.Live;
}
