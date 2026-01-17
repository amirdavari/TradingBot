namespace API.Services;

/// <summary>
/// Manages replay simulation state and provides market time.
/// This is a singleton service that holds the replay state.
/// </summary>
public class MarketTimeProvider : IMarketTimeProvider
{
    private readonly Models.ReplayState _replayState;
    private readonly ILogger<MarketTimeProvider> _logger;

    public MarketTimeProvider(ILogger<MarketTimeProvider> logger)
    {
        _logger = logger;
        _replayState = new Models.ReplayState
        {
            Mode = Models.MarketMode.Live,
            CurrentTime = DateTime.UtcNow,
            ReplayStartTime = DateTime.UtcNow,
            Speed = 1.0,
            IsRunning = false
        };
    }

    /// <summary>
    /// Gets the current market time based on the active mode.
    /// </summary>
    public DateTime GetCurrentTime()
    {
        return _replayState.Mode == Models.MarketMode.Live
            ? DateTime.UtcNow
            : _replayState.CurrentTime;
    }

    /// <summary>
    /// Gets the current market mode.
    /// </summary>
    public Models.MarketMode GetMode()
    {
        return _replayState.Mode;
    }

    /// <summary>
    /// Gets the current replay state.
    /// </summary>
    public Models.ReplayState GetReplayState()
    {
        return _replayState;
    }

    /// <summary>
    /// Sets the market mode.
    /// </summary>
    public void SetMode(Models.MarketMode mode)
    {
        _logger.LogInformation("Switching market mode from {OldMode} to {NewMode}", _replayState.Mode, mode);
        _replayState.Mode = mode;
    }

    /// <summary>
    /// Updates the replay state.
    /// Only applicable in Replay mode.
    /// </summary>
    public void UpdateReplayState(Action<Models.ReplayState> updateAction)
    {
        updateAction(_replayState);
    }
}
