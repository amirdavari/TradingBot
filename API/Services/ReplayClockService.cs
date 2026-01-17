using API.Models;

namespace API.Services;

/// <summary>
/// Background service that controls the replay clock.
/// Advances CurrentTime based on Speed when replay is running.
/// This service ONLY manages time - no market data or business logic.
/// </summary>
public class ReplayClockService : BackgroundService
{
    private readonly MarketTimeProvider _timeProvider;
    private readonly ILogger<ReplayClockService> _logger;
    private readonly TimeSpan _tickInterval = TimeSpan.FromSeconds(1);

    public ReplayClockService(
        MarketTimeProvider timeProvider,
        ILogger<ReplayClockService> logger)
    {
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <summary>
    /// Starts the replay simulation.
    /// </summary>
    public void Start()
    {
        _timeProvider.UpdateReplayState(state =>
        {
            if (state.Mode == MarketMode.Replay)
            {
                state.IsRunning = true;
                _logger.LogInformation("Replay started at {Time} with speed {Speed}x", 
                    state.CurrentTime, state.Speed);
            }
            else
            {
                _logger.LogWarning("Cannot start replay in Live mode");
            }
        });
    }

    /// <summary>
    /// Pauses the replay simulation.
    /// </summary>
    public void Pause()
    {
        _timeProvider.UpdateReplayState(state =>
        {
            state.IsRunning = false;
            _logger.LogInformation("Replay paused at {Time}", state.CurrentTime);
        });
    }

    /// <summary>
    /// Resets the replay to the start time.
    /// </summary>
    public void Reset()
    {
        _timeProvider.UpdateReplayState(state =>
        {
            state.CurrentTime = state.ReplayStartTime;
            state.IsRunning = false;
            _logger.LogInformation("Replay reset to {Time}", state.ReplayStartTime);
        });
    }

    /// <summary>
    /// Sets the replay speed multiplier.
    /// </summary>
    /// <param name="speed">Speed multiplier (1.0 = real-time, 5.0 = 5x speed, etc.)</param>
    public void SetSpeed(double speed)
    {
        if (speed <= 0)
        {
            _logger.LogWarning("Invalid speed {Speed}. Speed must be positive.", speed);
            return;
        }

        _timeProvider.UpdateReplayState(state =>
        {
            state.Speed = speed;
            _logger.LogInformation("Replay speed set to {Speed}x", speed);
        });
    }

    /// <summary>
    /// Sets the replay start time and current time.
    /// </summary>
    public void SetReplayStartTime(DateTime startTime)
    {
        _timeProvider.UpdateReplayState(state =>
        {
            state.ReplayStartTime = startTime;
            state.CurrentTime = startTime;
            state.IsRunning = false;
            _logger.LogInformation("Replay start time set to {Time}", startTime);
        });
    }

    /// <summary>
    /// Background task that advances the replay clock.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReplayClockService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_tickInterval, stoppingToken);

                var replayState = _timeProvider.GetReplayState();

                // Only advance time if in Replay mode and running
                if (replayState.Mode == MarketMode.Replay && replayState.IsRunning)
                {
                    _timeProvider.UpdateReplayState(state =>
                    {
                        // Advance time: CurrentTime += Tick * Speed
                        var timeAdvance = TimeSpan.FromSeconds(_tickInterval.TotalSeconds * state.Speed);
                        state.CurrentTime = state.CurrentTime.Add(timeAdvance);

                        _logger.LogDebug("Replay time advanced to {Time} (Speed: {Speed}x)", 
                            state.CurrentTime, state.Speed);
                    });
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ReplayClockService");
            }
        }

        _logger.LogInformation("ReplayClockService stopped");
    }
}
