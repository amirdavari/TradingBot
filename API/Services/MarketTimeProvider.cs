namespace API.Services;

/// <summary>
/// Manages market mode state (Live vs Mock).
/// Simplified after removal of time-simulation replay feature.
/// </summary>
public class MarketTimeProvider : IMarketTimeProvider
{
    private Models.MarketMode _mode;
    private readonly IServiceProvider _serviceProvider;
    private readonly object _syncRoot = new();

    public MarketTimeProvider(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _mode = LoadModeFromDatabase();
    }

    private Models.MarketMode LoadModeFromDatabase()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();
            var entity = db.MarketModes.FirstOrDefault();
            
            if (entity != null)
            {
                return entity.Mode;
            }
        }
        catch { /* Ignore - use default */ }
        return Models.MarketMode.Live;
    }

    private void SaveModeToDatabase()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Data.ApplicationDbContext>();
            
            var entity = db.MarketModes.FirstOrDefault();
            if (entity == null)
            {
                entity = new Models.MarketModeEntity { Id = 1 };
                db.MarketModes.Add(entity);
            }
            
            entity.Mode = _mode;
            db.SaveChanges();
        }
        catch { /* Ignore persistence errors */ }
    }

    public DateTime GetCurrentTime()
    {
        return DateTime.UtcNow;
    }

    public Models.MarketMode GetMode()
    {
        lock (_syncRoot)
        {
            return _mode;
        }
    }

    public Models.MarketModeState GetMarketState()
    {
        lock (_syncRoot)
        {
            return new Models.MarketModeState
            {
                Mode = _mode
            };
        }
    }

    public void SetMode(Models.MarketMode mode)
    {
        lock (_syncRoot)
        {
            _mode = mode;
            SaveModeToDatabase();
        }
    }
}
