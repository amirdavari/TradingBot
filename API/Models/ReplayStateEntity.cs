namespace API.Models;

/// <summary>
/// Database entity for persisting replay simulation state.
/// </summary>
public class ReplayStateEntity
{
    /// <summary>
    /// Singleton ID - always 1 to ensure only one state exists.
    /// </summary>
    public int Id { get; set; } = 1;

    /// <summary>
    /// The starting time point for the replay simulation.
    /// CurrentTime will be reset to this value on application restart.
    /// </summary>
    public DateTime ReplayStartTime { get; set; }

    /// <summary>
    /// Replay speed multiplier (1.0, 5.0, 10.0).
    /// </summary>
    public double Speed { get; set; } = 1.0;

    /// <summary>
    /// Indicates whether the replay is currently running or paused.
    /// </summary>
    public bool IsRunning { get; set; }

    /// <summary>
    /// The current market mode (Live or Replay).
    /// </summary>
    public MarketMode Mode { get; set; } = MarketMode.Live;
}
