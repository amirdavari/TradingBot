using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

/// <summary>
/// Service for managing simulation settings.
/// </summary>
public interface ISimulationSettingsService
{
    /// <summary>Gets current simulation settings</summary>
    Task<SimulationSettings> GetSettingsAsync();

    /// <summary>Updates simulation settings</summary>
    Task<SimulationSettings> UpdateSettingsAsync(SimulationSettings settings);

    /// <summary>Resets to default settings</summary>
    Task<SimulationSettings> ResetToDefaultsAsync();
}

public class SimulationSettingsService : ISimulationSettingsService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SimulationSettingsService> _logger;

    // In-memory cache
    private SimulationSettings _cachedSettings = new();
    private readonly object _lock = new();

    public SimulationSettingsService(
        IServiceProvider serviceProvider,
        ILogger<SimulationSettingsService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;

        // Load settings on startup
        _ = Task.Run(LoadSettingsAsync);
    }

    public async Task<SimulationSettings> GetSettingsAsync()
    {
        lock (_lock)
        {
            return _cachedSettings;
        }
    }

    public async Task<SimulationSettings> UpdateSettingsAsync(SimulationSettings settings)
    {
        // Validate bounds
        settings.VolatilityScale = Math.Clamp(settings.VolatilityScale, 0.01m, 1.0m);
        settings.DriftScale = Math.Clamp(settings.DriftScale, 0.01m, 1.0m);
        settings.MeanReversionStrength = Math.Clamp(settings.MeanReversionStrength, 0m, 1.0m);
        settings.FatTailMultiplier = Math.Clamp(settings.FatTailMultiplier, 0m, 2.0m);
        settings.FatTailMinSize = Math.Clamp(settings.FatTailMinSize, 1.0m, 3.0m);
        settings.FatTailMaxSize = Math.Clamp(settings.FatTailMaxSize, settings.FatTailMinSize, 5.0m);
        settings.MaxReturnPerBar = Math.Clamp(settings.MaxReturnPerBar, 0.005m, 0.1m);
        settings.LiveTickNoise = Math.Clamp(settings.LiveTickNoise, 0m, 0.1m);
        settings.HighLowRangeMultiplier = Math.Clamp(settings.HighLowRangeMultiplier, 0.1m, 1.0m);
        settings.PatternOverlayStrength = Math.Clamp(settings.PatternOverlayStrength, 0m, 3.0m);

        lock (_lock)
        {
            _cachedSettings = settings;
        }

        await PersistSettingsAsync(settings);
        _logger.LogInformation("Simulation settings updated");

        return settings;
    }

    public async Task<SimulationSettings> ResetToDefaultsAsync()
    {
        var defaults = new SimulationSettings();
        return await UpdateSettingsAsync(defaults);
    }

    private async Task LoadSettingsAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var entity = await db.SimulationSettings.FirstOrDefaultAsync(e => e.Id == 1);
            if (entity != null)
            {
                lock (_lock)
                {
                    _cachedSettings = new SimulationSettings
                    {
                        VolatilityScale = entity.VolatilityScale,
                        DriftScale = entity.DriftScale,
                        MeanReversionStrength = entity.MeanReversionStrength,
                        FatTailMultiplier = entity.FatTailMultiplier,
                        FatTailMinSize = entity.FatTailMinSize,
                        FatTailMaxSize = entity.FatTailMaxSize,
                        MaxReturnPerBar = entity.MaxReturnPerBar,
                        LiveTickNoise = entity.LiveTickNoise,
                        HighLowRangeMultiplier = entity.HighLowRangeMultiplier,
                        PatternOverlayStrength = entity.PatternOverlayStrength
                    };
                }
                _logger.LogInformation("Loaded simulation settings from database");
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load simulation settings, using defaults");
        }
    }

    private async Task PersistSettingsAsync(SimulationSettings settings)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var entity = await db.SimulationSettings.FirstOrDefaultAsync(e => e.Id == 1);
            if (entity == null)
            {
                entity = new SimulationSettingsEntity { Id = 1 };
                db.SimulationSettings.Add(entity);
            }

            entity.VolatilityScale = settings.VolatilityScale;
            entity.DriftScale = settings.DriftScale;
            entity.MeanReversionStrength = settings.MeanReversionStrength;
            entity.FatTailMultiplier = settings.FatTailMultiplier;
            entity.FatTailMinSize = settings.FatTailMinSize;
            entity.FatTailMaxSize = settings.FatTailMaxSize;
            entity.MaxReturnPerBar = settings.MaxReturnPerBar;
            entity.LiveTickNoise = settings.LiveTickNoise;
            entity.HighLowRangeMultiplier = settings.HighLowRangeMultiplier;
            entity.PatternOverlayStrength = settings.PatternOverlayStrength;
            entity.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist simulation settings");
        }
    }
}
