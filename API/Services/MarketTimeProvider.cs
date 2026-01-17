namespace API.Services;

/// <summary>
/// Manages replay simulation state and provides market time.
/// This is a singleton service that holds the replay state.
/// </summary>
public class MarketTimeProvider : IMarketTimeProvider
{
    private readonly Models.ReplayState _replayState;
    private readonly ILogger<MarketTimeProvider> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly object _syncRoot = new();

    public MarketTimeProvider(ILogger<MarketTimeProvider> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        
        // Try to load state from database, otherwise use defaults
        _replayState = LoadStateFromDatabase() ?? new Models.ReplayState
        {
            Mode = Models.MarketMode.Live,
            CurrentTime = DateTime.UtcNow,
            ReplayStartTime = DateTime.UtcNow,
            Speed = 1.0,
            IsRunning = false
        };
        
        _logger.LogInformation("MarketTimeProvider initialized with mode: {Mode}, CurrentTime: {Time}", 
            _replayState.Mode, _replayState.CurrentTime);
    }

    private Models.ReplayState? LoadStateFromDatabase()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();
            var entity = db.ReplayStates.FirstOrDefault();
            
            if (entity != null)
            {
                _logger.LogInformation("Loaded replay state from database: Mode={Mode}, ReplayStartTime={Time}", 
                    entity.Mode, entity.ReplayStartTime);
                return new Models.ReplayState
                {
                    Mode = entity.Mode,
                    CurrentTime = entity.ReplayStartTime, // Start at replay start time
                    ReplayStartTime = entity.ReplayStartTime,
                    Speed = entity.Speed,
                    IsRunning = entity.IsRunning
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load replay state from database, using defaults");
        }
        return null;
    }

    private void SaveStateToDatabase()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();
            
            var entity = db.ReplayStates.FirstOrDefault();
            if (entity == null)
            {
                entity = new Models.ReplayStateEntity { Id = 1 };
                db.ReplayStates.Add(entity);
            }
            
            entity.Mode = _replayState.Mode;
            // CurrentTime is not persisted - it will be reset to ReplayStartTime on restart
            entity.ReplayStartTime = _replayState.ReplayStartTime;
            entity.Speed = _replayState.Speed;
            entity.IsRunning = _replayState.IsRunning;
            
            db.SaveChanges();
            _logger.LogDebug("Saved replay state to database");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save replay state to database");
        }
    }

    /// <summary>
    /// Gets the current market time based on the active mode.
    /// </summary>
    public DateTime GetCurrentTime()
    {
        lock (_syncRoot)
        {
            return _replayState.Mode == Models.MarketMode.Live
                ? DateTime.UtcNow
                : _replayState.CurrentTime;
        }
    }

    /// <summary>
    /// Gets the current market mode.
    /// </summary>
    public Models.MarketMode GetMode()
    {
        lock (_syncRoot)
        {
            return _replayState.Mode;
        }
    }

    /// <summary>
    /// Gets the current replay state.
    /// Returns a copy to prevent external modifications.
    /// </summary>
    public Models.ReplayState GetReplayState()
    {
        lock (_syncRoot)
        {
            return new Models.ReplayState
            {
                Mode = _replayState.Mode,
                CurrentTime = _replayState.CurrentTime,
                ReplayStartTime = _replayState.ReplayStartTime,
                Speed = _replayState.Speed,
                IsRunning = _replayState.IsRunning
            };
        }
    }

    /// <summary>
    /// Sets the market mode.
    /// </summary>
    public void SetMode(Models.MarketMode mode)
    {
        lock (_syncRoot)
        {
            _logger.LogInformation("Switching market mode from {OldMode} to {NewMode}", _replayState.Mode, mode);
            _replayState.Mode = mode;
            SaveStateToDatabase();
        }
    }

    /// <summary>
    /// Updates the replay state.
    /// Only applicable in Replay mode.
    /// </summary>
    public void UpdateReplayState(Action<Models.ReplayState> updateAction)
    {
        lock (_syncRoot)
        {
            updateAction(_replayState);
            SaveStateToDatabase();
        }
    }
}
