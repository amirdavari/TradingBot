namespace API.Models;

/// <summary>
/// Represents the state of replay simulation.
/// This is the single source of truth for replay time.
/// </summary>
public class ReplayState
{
    /// <summary>
    /// The starting time point for the replay simulation.
    /// </summary>
    public DateTime ReplayStartTime { get; set; }

    /// <summary>
    /// The current simulated time in replay mode.
    /// This is the ONLY valid time source during replay.
    /// </summary>
    public DateTime CurrentTime { get; set; }

    /// <summary>
    /// Replay speed multiplier.
    /// 1.0 = real-time, 5.0 = 5x speed, 10.0 = 10x speed
    /// </summary>
    public double Speed { get; set; } = 1.0;

    /// <summary>
    /// Indicates whether the replay is currently running or paused.
    /// </summary>
    public bool IsRunning { get; set; }

    /// <summary>
    /// The current market mode.
    /// </summary>
    public MarketMode Mode { get; set; } = MarketMode.Live;
}
