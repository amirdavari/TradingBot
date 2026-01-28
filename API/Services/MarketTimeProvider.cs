namespace API.Services;

/// <summary>
/// Manages market mode state (Live vs Mock).
/// </summary>
public class MarketTimeProvider : IMarketTimeProvider
{
    private readonly Models.ReplayState _replayState;
    private readonly IServiceProvider _serviceProvider;
    private readonly object _syncRoot = new();

    public MarketTimeProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _replayState = LoadStateFromDatabase() ?? new Models.ReplayState
        {
            Mode = Models.MarketMode.Live,
            CurrentTime = DateTime.UtcNow,
            ReplayStartTime = DateTime.UtcNow,
            Speed = 1.0,
            IsRunning = false
        };
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
                return new Models.ReplayState
                {
                    Mode = entity.Mode,
                    CurrentTime = DateTime.UtcNow,
                    ReplayStartTime = entity.ReplayStartTime,
                    Speed = entity.Speed,
                    IsRunning = false
                };
            }
        }
        catch { /* Ignore - use defaults */ }
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
            entity.ReplayStartTime = _replayState.ReplayStartTime;
            entity.Speed = _replayState.Speed;
            entity.IsRunning = _replayState.IsRunning;
            
            db.SaveChanges();
        }
        catch { /* Ignore persistence errors */ }
    }

    public DateTime GetCurrentTime()
    {
        lock (_syncRoot)
        {
            return DateTime.UtcNow; // Always use real time
        }
    }

    public Models.MarketMode GetMode()
    {
        lock (_syncRoot)
        {
            return _replayState.Mode;
        }
    }

    public Models.ReplayState GetReplayState()
    {
        lock (_syncRoot)
        {
            return new Models.ReplayState
            {
                Mode = _replayState.Mode,
                CurrentTime = DateTime.UtcNow,
                ReplayStartTime = _replayState.ReplayStartTime,
                Speed = _replayState.Speed,
                IsRunning = _replayState.IsRunning
            };
        }
    }

    public void SetMode(Models.MarketMode mode)
    {
        lock (_syncRoot)
        {
            _replayState.Mode = mode;
            SaveStateToDatabase();
        }
    }

    public void UpdateReplayState(Action<Models.ReplayState> updateAction)
    {
        lock (_syncRoot)
        {
            updateAction(_replayState);
            SaveStateToDatabase();
        }
    }
}
