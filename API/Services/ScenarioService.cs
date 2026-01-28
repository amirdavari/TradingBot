using System.Text.Json;
using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

/// <summary>
/// Interface for scenario configuration service.
/// </summary>
public interface IScenarioService
{
    /// <summary>Gets the currently active scenario configuration</summary>
    ScenarioConfig GetActiveScenario();

    /// <summary>Gets whether scenario simulation is enabled</summary>
    bool IsScenarioEnabled();

    /// <summary>Gets all available preset scenarios</summary>
    List<ScenarioPresetDto> GetPresets();

    /// <summary>Gets current scenario state</summary>
    Task<ScenarioStateDto> GetStateAsync();

    /// <summary>Applies a preset scenario by name</summary>
    Task ApplyPresetAsync(string presetName);

    /// <summary>Applies a custom scenario configuration</summary>
    Task ApplyCustomAsync(ScenarioConfig config);

    /// <summary>Resets to default (no scenario)</summary>
    Task ResetAsync();

    /// <summary>Enables/disables scenario simulation</summary>
    Task SetEnabledAsync(bool enabled);
}

/// <summary>
/// Service for managing market simulation scenarios.
/// Handles preset library, custom configs, and persistence.
/// </summary>
public class ScenarioService : IScenarioService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ScenarioService> _logger;
    private readonly MarketSimulationEngine _simulationEngine;

    // In-memory cache of active config
    private ScenarioConfig _activeConfig = CreateDefaultConfig();
    private bool _isEnabled = false;
    private readonly object _lock = new();

    // Built-in presets
    private static readonly Dictionary<string, ScenarioConfig> Presets = new()
    {
        ["Default"] = CreateDefaultConfig(),
        ["VWAP Long Test"] = CreateVwapLongTestConfig(),
        ["VWAP Short Test"] = CreateVwapShortTestConfig(),
        ["High Volume Breakout"] = CreateHighVolumeBreakoutConfig(),
        ["Low Confidence Range"] = CreateLowConfidenceRangeConfig(),
        ["ATR Test High Vol"] = CreateAtrTestHighVolConfig(),
        ["Trend Reversal"] = CreateTrendReversalConfig(),
        ["Crash Scenario"] = CreateCrashScenarioConfig()
    };

    public ScenarioService(
        IServiceProvider serviceProvider,
        ILogger<ScenarioService> logger,
        MarketSimulationEngine simulationEngine)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _simulationEngine = simulationEngine;

        // Load persisted config on startup
        _ = Task.Run(LoadPersistedConfigAsync);
    }

    #region IScenarioService Implementation

    public ScenarioConfig GetActiveScenario()
    {
        lock (_lock)
        {
            return _activeConfig;
        }
    }

    public bool IsScenarioEnabled()
    {
        lock (_lock)
        {
            return _isEnabled;
        }
    }

    public List<ScenarioPresetDto> GetPresets()
    {
        return Presets.Select(kvp => new ScenarioPresetDto
        {
            Name = kvp.Key,
            Description = kvp.Value.Description,
            IsBuiltIn = true,
            TotalBars = kvp.Value.Regimes.Sum(r => r.Bars),
            Regimes = kvp.Value.Regimes.Select(r => r.Type.ToString()).ToList(),
            Patterns = kvp.Value.Overlays.Select(o => o.Type.ToString()).ToList()
        }).ToList();
    }

    public async Task<ScenarioStateDto> GetStateAsync()
    {
        lock (_lock)
        {
            return new ScenarioStateDto
            {
                IsEnabled = _isEnabled,
                ActiveConfig = _activeConfig,
                AvailablePresets = Presets.Keys.ToList()
            };
        }
    }

    public async Task ApplyPresetAsync(string presetName)
    {
        if (!Presets.TryGetValue(presetName, out var preset))
        {
            throw new ArgumentException($"Unknown preset: {presetName}");
        }

        lock (_lock)
        {
            _activeConfig = CloneConfig(preset);
            _isEnabled = true;
        }

        _simulationEngine.ResetState();
        await PersistConfigAsync();
    }

    public async Task ApplyCustomAsync(ScenarioConfig config)
    {
        var customConfig = config with { Name = "Custom" };

        lock (_lock)
        {
            _activeConfig = customConfig;
            _isEnabled = true;
        }

        _simulationEngine.ResetState();
        await PersistConfigAsync();
    }

    public async Task ResetAsync()
    {
        lock (_lock)
        {
            _activeConfig = CreateDefaultConfig();
            _isEnabled = false;
        }

        _simulationEngine.ResetState();
        await PersistConfigAsync();
    }

    public async Task SetEnabledAsync(bool enabled)
    {
        lock (_lock)
        {
            _isEnabled = enabled;
        }

        await PersistConfigAsync();
    }

    #endregion

    #region Persistence

    private async Task LoadPersistedConfigAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var entity = await db.ScenarioConfigs.FirstOrDefaultAsync(e => e.Id == 1);
            if (entity != null)
            {
                var config = JsonSerializer.Deserialize<ScenarioConfig>(entity.ConfigJson);
                if (config != null)
                {
                    lock (_lock)
                    {
                        _activeConfig = config;
                        _isEnabled = entity.IsEnabled;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load persisted scenario config, using defaults");
        }
    }

    private async Task PersistConfigAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            ScenarioConfig configToSave;
            bool isEnabled;
            lock (_lock)
            {
                configToSave = _activeConfig;
                isEnabled = _isEnabled;
            }

            var entity = await db.ScenarioConfigs.FirstOrDefaultAsync(e => e.Id == 1);
            if (entity == null)
            {
                entity = new ScenarioConfigEntity { Id = 1 };
                db.ScenarioConfigs.Add(entity);
            }

            entity.ActivePreset = configToSave.Name;
            entity.ConfigJson = JsonSerializer.Serialize(configToSave);
            entity.IsEnabled = isEnabled;
            entity.UpdatedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist scenario config");
        }
    }

    #endregion

    #region Preset Configurations

    private static ScenarioConfig CreateDefaultConfig() => new()
    {
        Name = "Default",
        Description = "Default random market behavior with moderate volatility",
        Seed = null,
        BaseVolatility = 0.02m,
        Regimes = new List<RegimePhase>
        {
            new() { Type = MarketRegime.RANGE, Bars = 500 }
        },
        Overlays = new List<PatternOverlayConfig>()
    };

    private static ScenarioConfig CreateVwapLongTestConfig() => new()
    {
        Name = "VWAP Long Test",
        Description = "200 bars range → BREAKOUT above VWAP → 300 bars uptrend with pullback. Tests LONG signal generation.",
        Seed = 42,
        BaseVolatility = 0.015m,
        Regimes = new List<RegimePhase>
        {
            new() { Type = MarketRegime.RANGE, Bars = 200, VolumeMultiplier = 0.9m },
            new() { Type = MarketRegime.TREND_UP, Bars = 300, VolumeMultiplier = 1.3m }
        },
        Overlays = new List<PatternOverlayConfig>
        {
            new() { Type = PatternOverlayType.BREAKOUT, AtBar = 180, Direction = "UP", VolumeBoost = 2.5m, NoiseBars = 3 },
            new() { Type = PatternOverlayType.PULLBACK, AtBar = 200, ToBar = 240, Direction = "UP", DepthATR = 0.8m, VolumeBoost = 1.2m }
        }
    };

    private static ScenarioConfig CreateVwapShortTestConfig() => new()
    {
        Name = "VWAP Short Test",
        Description = "200 bars range → BREAKOUT below VWAP → 300 bars downtrend. Tests SHORT signal generation.",
        Seed = 43,
        BaseVolatility = 0.018m,
        Regimes = new List<RegimePhase>
        {
            new() { Type = MarketRegime.RANGE, Bars = 200, VolumeMultiplier = 0.9m },
            new() { Type = MarketRegime.TREND_DOWN, Bars = 300, VolumeMultiplier = 1.4m }
        },
        Overlays = new List<PatternOverlayConfig>
        {
            new() { Type = PatternOverlayType.BREAKOUT, AtBar = 180, Direction = "DOWN", VolumeBoost = 2.5m, NoiseBars = 3 }
        }
    };

    private static ScenarioConfig CreateHighVolumeBreakoutConfig() => new()
    {
        Name = "High Volume Breakout",
        Description = "Consolidation followed by massive volume breakout (3x average). Tests volume filter threshold.",
        Seed = 100,
        BaseVolatility = 0.012m,
        Regimes = new List<RegimePhase>
        {
            new() { Type = MarketRegime.LOW_VOL, Bars = 150, VolumeMultiplier = 0.7m },
            new() { Type = MarketRegime.RANGE, Bars = 100, VolumeMultiplier = 0.8m },
            new() { Type = MarketRegime.TREND_UP, Bars = 200, VolumeMultiplier = 1.5m }
        },
        Overlays = new List<PatternOverlayConfig>
        {
            new() { Type = PatternOverlayType.BREAKOUT, AtBar = 250, Direction = "UP", VolumeBoost = 3.0m, NoiseBars = 2 }
        }
    };

    private static ScenarioConfig CreateLowConfidenceRangeConfig() => new()
    {
        Name = "Low Confidence Range",
        Description = "High volatility, low volume range-bound market. Should NOT trigger strong signals.",
        Seed = 200,
        BaseVolatility = 0.035m, // >3% = low confidence penalty
        Regimes = new List<RegimePhase>
        {
            new() { Type = MarketRegime.HIGH_VOL, Bars = 300, VolumeMultiplier = 0.7m }, // Below 1.2x threshold
            new() { Type = MarketRegime.RANGE, Bars = 200, Volatility = 0.04m, VolumeMultiplier = 0.6m }
        },
        Overlays = new List<PatternOverlayConfig>()
    };

    private static ScenarioConfig CreateAtrTestHighVolConfig() => new()
    {
        Name = "ATR Test High Vol",
        Description = "High volatility phase for testing ATR-based SL/TP calculations. Wide stops expected.",
        Seed = 300,
        BaseVolatility = 0.025m,
        Regimes = new List<RegimePhase>
        {
            new() { Type = MarketRegime.RANGE, Bars = 100 },
            new() { Type = MarketRegime.HIGH_VOL, Bars = 200, Volatility = 0.04m },
            new() { Type = MarketRegime.TREND_UP, Bars = 200, Volatility = 0.03m }
        },
        Overlays = new List<PatternOverlayConfig>
        {
            new() { Type = PatternOverlayType.BREAKOUT, AtBar = 300, Direction = "UP", VolumeBoost = 2.0m }
        }
    };

    private static ScenarioConfig CreateTrendReversalConfig() => new()
    {
        Name = "Trend Reversal",
        Description = "Uptrend → Double Top → Downtrend. Tests reversal pattern detection.",
        Seed = 400,
        BaseVolatility = 0.02m,
        Regimes = new List<RegimePhase>
        {
            new() { Type = MarketRegime.TREND_UP, Bars = 200 },
            new() { Type = MarketRegime.RANGE, Bars = 100 },
            new() { Type = MarketRegime.TREND_DOWN, Bars = 200 }
        },
        Overlays = new List<PatternOverlayConfig>
        {
            new() { Type = PatternOverlayType.DOUBLE_TOP, AtBar = 180, ToBar = 280, Direction = "DOWN", VolumeBoost = 1.5m }
        }
    };

    private static ScenarioConfig CreateCrashScenarioConfig() => new()
    {
        Name = "Crash Scenario",
        Description = "Sudden market crash with extreme volatility and volume. Tests risk management.",
        Seed = 999,
        BaseVolatility = 0.02m,
        GapProbability = 0.3m,
        MaxGapPercent = 0.05m,
        Regimes = new List<RegimePhase>
        {
            new() { Type = MarketRegime.TREND_UP, Bars = 150, VolumeMultiplier = 1.0m },
            new() { Type = MarketRegime.CRASH, Bars = 50, VolumeMultiplier = 3.0m },
            new() { Type = MarketRegime.HIGH_VOL, Bars = 100, VolumeMultiplier = 2.0m },
            new() { Type = MarketRegime.RANGE, Bars = 200, VolumeMultiplier = 1.2m }
        },
        Overlays = new List<PatternOverlayConfig>
        {
            new() { Type = PatternOverlayType.GAP_AND_GO, AtBar = 150, Direction = "DOWN", VolumeBoost = 4.0m }
        }
    };

    private static ScenarioConfig CloneConfig(ScenarioConfig source)
    {
        var json = JsonSerializer.Serialize(source);
        return JsonSerializer.Deserialize<ScenarioConfig>(json) ?? CreateDefaultConfig();
    }

    #endregion
}
