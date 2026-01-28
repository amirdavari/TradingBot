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

    /// <summary>Gets the scenario configuration for a specific symbol (distributed per-symbol)</summary>
    ScenarioConfig GetScenarioForSymbol(string symbol);

    /// <summary>Gets all symbol-to-scenario assignments</summary>
    List<SymbolScenarioAssignment> GetSymbolAssignments();

    /// <summary>Redistributes scenarios randomly to all symbols</summary>
    void RedistributeScenarios();

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

    // Per-symbol scenario assignments (randomized on startup/reset)
    private readonly Dictionary<string, string> _symbolScenarioMap = new(StringComparer.OrdinalIgnoreCase);
    private readonly Random _random = new();

    // Built-in presets
    private static readonly Dictionary<string, ScenarioConfig> Presets = new()
    {
        ["Realistic Day"] = CreateRealisticDayConfig(),
        ["Default"] = CreateDefaultConfig(),
        ["VWAP Long Setup"] = CreateVwapLongTestConfig(),
        ["VWAP Short Setup"] = CreateVwapShortTestConfig(),
        ["Volume Breakout"] = CreateHighVolumeBreakoutConfig(),
        ["Choppy Sideways"] = CreateLowConfidenceRangeConfig(),
        ["Volatile Session"] = CreateAtrTestHighVolConfig(),
        ["Trend Reversal"] = CreateTrendReversalConfig(),
        ["Flash Crash"] = CreateCrashScenarioConfig()
    };

    public ScenarioService(
        IServiceProvider serviceProvider,
        ILogger<ScenarioService> logger,
        MarketSimulationEngine simulationEngine)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        _simulationEngine = simulationEngine;

        // Initialize symbol-scenario assignments randomly
        InitializeSymbolAssignments();

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

    public ScenarioConfig GetScenarioForSymbol(string symbol)
    {
        lock (_lock)
        {
            if (_symbolScenarioMap.TryGetValue(symbol, out var presetName) && Presets.TryGetValue(presetName, out var config))
            {
                return config with { Symbol = symbol };
            }
            // Fallback to default
            return CreateDefaultConfig() with { Symbol = symbol };
        }
    }

    public List<SymbolScenarioAssignment> GetSymbolAssignments()
    {
        lock (_lock)
        {
            return _symbolScenarioMap.Select(kvp => new SymbolScenarioAssignment
            {
                Symbol = kvp.Key,
                ScenarioPreset = kvp.Value,
                Strategy = "VWAP Momentum" // Currently only one strategy
            }).OrderBy(a => a.Symbol).ToList();
        }
    }

    public void RedistributeScenarios()
    {
        lock (_lock)
        {
            _symbolScenarioMap.Clear();
            InitializeSymbolAssignmentsInternal();
        }
        _logger.LogInformation("Redistributed scenarios to {Count} symbols", _symbolScenarioMap.Count);
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
                AvailablePresets = Presets.Keys.ToList(),
                SymbolAssignments = GetSymbolAssignments()
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

    #region Symbol Assignment

    // List of all supported symbols for mock data
    private static readonly string[] AllSymbols = new[]
    {
        "SAP.DE", "SIE.DE", "ALV.DE", "BAS.DE", "IFX.DE", "BMW.DE", "MBG.DE", "VOW3.DE",
        "DTE.DE", "RWE.DE", "EOAN.DE", "MUV2.DE", "CBK.DE", "DBK.DE", "ENR.DE", "ADS.DE",
        "BAYN.DE", "HEI.DE", "ZAL.DE", "DB1.DE", "RHM.DE", "MTX.DE", "AIR.DE", "SRT3.DE",
        "SY1.DE", "HEN3.DE", "1COV.DE", "P911.DE", "VNA.DE", "FRE.DE", "HFG.DE", "DHER.DE",
        "BEI.DE", "HNR1.DE", "BNR.DE", "SHL.DE", "FME.DE", "MRK.DE", "QIA.DE", "PAH3.DE",
        "TMV.DE", "AIXA.DE", "S92.DE", "EVT.DE", "AFX.DE", "NEM.DE", "WAF.DE", "JEN.DE",
        "COK.DE", "GFT.DE", "NA9.DE", "SMHN.DE"
    };

    private void InitializeSymbolAssignments()
    {
        lock (_lock)
        {
            InitializeSymbolAssignmentsInternal();
        }
    }

    private void InitializeSymbolAssignmentsInternal()
    {
        var presetNames = Presets.Keys.ToList();
        foreach (var symbol in AllSymbols)
        {
            // Randomly assign a preset to each symbol
            var presetIndex = _random.Next(presetNames.Count);
            _symbolScenarioMap[symbol] = presetNames[presetIndex];
        }
        _logger.LogInformation("Initialized scenario assignments for {Count} symbols", AllSymbols.Length);
    }

    #endregion

    #region Preset Configurations

    private static ScenarioConfig CreateRealisticDayConfig() => new()
    {
        Name = "Realistic Day",
        Description = "Simulates a typical trading day: quiet open → mid-morning activity → lunch lull → afternoon push",
        Seed = 1001,
        BaseVolatility = 0.008m,
        Regimes = new List<RegimePhase>
        {
            new() { Type = MarketRegime.LOW_VOL, Bars = 40, VolumeMultiplier = 0.7m, Volatility = 0.006m, Drift = 0.0001m },
            new() { Type = MarketRegime.RANGE, Bars = 60, VolumeMultiplier = 1.0m, Volatility = 0.009m, Drift = 0.0002m },
            new() { Type = MarketRegime.TREND_UP, Bars = 50, VolumeMultiplier = 1.2m, Volatility = 0.010m, Drift = 0.0004m },
            new() { Type = MarketRegime.LOW_VOL, Bars = 30, VolumeMultiplier = 0.6m, Volatility = 0.005m, Drift = 0.0m },
            new() { Type = MarketRegime.RANGE, Bars = 40, VolumeMultiplier = 0.8m, Volatility = 0.007m, Drift = -0.0001m },
            new() { Type = MarketRegime.TREND_UP, Bars = 60, VolumeMultiplier = 1.3m, Volatility = 0.011m, Drift = 0.0003m },
            new() { Type = MarketRegime.HIGH_VOL, Bars = 20, VolumeMultiplier = 1.5m, Volatility = 0.014m, Drift = 0.0002m }
        },
        Overlays = new List<PatternOverlayConfig>()
    };

    private static ScenarioConfig CreateDefaultConfig() => new()
    {
        Name = "Default",
        Description = "Gentle random walk with low volatility, suitable for basic testing",
        Seed = null,
        BaseVolatility = 0.010m,
        Regimes = new List<RegimePhase>
        {
            new() { Type = MarketRegime.RANGE, Bars = 200, VolumeMultiplier = 1.0m, Volatility = 0.009m },
            new() { Type = MarketRegime.TREND_UP, Bars = 150, VolumeMultiplier = 1.1m, Volatility = 0.010m, Drift = 0.0002m },
            new() { Type = MarketRegime.RANGE, Bars = 150, VolumeMultiplier = 0.9m, Volatility = 0.008m }
        },
        Overlays = new List<PatternOverlayConfig>()
    };

    private static ScenarioConfig CreateVwapLongTestConfig() => new()
    {
        Name = "VWAP Long Setup",
        Description = "Gradual accumulation below VWAP → steady breakout → healthy uptrend with natural pullbacks",
        Seed = 42,
        BaseVolatility = 0.009m,
        Regimes = new List<RegimePhase>
        {
            new() { Type = MarketRegime.RANGE, Bars = 80, VolumeMultiplier = 0.8m, Volatility = 0.007m, Drift = -0.0001m },
            new() { Type = MarketRegime.LOW_VOL, Bars = 40, VolumeMultiplier = 0.7m, Volatility = 0.005m, Drift = 0.0001m },
            new() { Type = MarketRegime.RANGE, Bars = 30, VolumeMultiplier = 1.1m, Volatility = 0.008m, Drift = 0.0002m },
            new() { Type = MarketRegime.TREND_UP, Bars = 100, VolumeMultiplier = 1.3m, Volatility = 0.011m, Drift = 0.0005m },
            new() { Type = MarketRegime.RANGE, Bars = 30, VolumeMultiplier = 1.0m, Volatility = 0.008m, Drift = 0.0001m },
            new() { Type = MarketRegime.TREND_UP, Bars = 70, VolumeMultiplier = 1.2m, Volatility = 0.010m, Drift = 0.0004m }
        },
        Overlays = new List<PatternOverlayConfig>
        {
            new() { Type = PatternOverlayType.BREAKOUT, AtBar = 145, Direction = "UP", VolumeBoost = 1.6m, NoiseBars = 5 },
            new() { Type = PatternOverlayType.PULLBACK, AtBar = 250, ToBar = 280, Direction = "UP", DepthATR = 0.5m, VolumeBoost = 1.1m }
        }
    };

    private static ScenarioConfig CreateVwapShortTestConfig() => new()
    {
        Name = "VWAP Short Setup",
        Description = "Distribution above VWAP → gradual breakdown → controlled downtrend with dead cat bounces",
        Seed = 43,
        BaseVolatility = 0.010m,
        Regimes = new List<RegimePhase>
        {
            new() { Type = MarketRegime.RANGE, Bars = 60, VolumeMultiplier = 0.9m, Volatility = 0.008m, Drift = 0.0001m },
            new() { Type = MarketRegime.TREND_UP, Bars = 40, VolumeMultiplier = 0.8m, Volatility = 0.007m, Drift = 0.0002m },
            new() { Type = MarketRegime.RANGE, Bars = 50, VolumeMultiplier = 1.0m, Volatility = 0.009m, Drift = -0.0001m },
            new() { Type = MarketRegime.TREND_DOWN, Bars = 80, VolumeMultiplier = 1.3m, Volatility = 0.012m, Drift = -0.0005m },
            new() { Type = MarketRegime.RANGE, Bars = 30, VolumeMultiplier = 0.9m, Volatility = 0.008m, Drift = 0.0001m },
            new() { Type = MarketRegime.TREND_DOWN, Bars = 60, VolumeMultiplier = 1.2m, Volatility = 0.011m, Drift = -0.0004m }
        },
        Overlays = new List<PatternOverlayConfig>
        {
            new() { Type = PatternOverlayType.BREAKOUT, AtBar = 145, Direction = "DOWN", VolumeBoost = 1.5m, NoiseBars = 4 }
        }
    };

    private static ScenarioConfig CreateHighVolumeBreakoutConfig() => new()
    {
        Name = "Volume Breakout",
        Description = "Extended quiet consolidation → volume builds gradually → breakout with sustained follow-through",
        Seed = 100,
        BaseVolatility = 0.007m,
        Regimes = new List<RegimePhase>
        {
            new() { Type = MarketRegime.LOW_VOL, Bars = 60, VolumeMultiplier = 0.6m, Volatility = 0.005m, Drift = 0.0m },
            new() { Type = MarketRegime.RANGE, Bars = 50, VolumeMultiplier = 0.7m, Volatility = 0.006m, Drift = 0.0001m },
            new() { Type = MarketRegime.LOW_VOL, Bars = 40, VolumeMultiplier = 0.8m, Volatility = 0.006m, Drift = 0.0001m },
            new() { Type = MarketRegime.RANGE, Bars = 30, VolumeMultiplier = 1.2m, Volatility = 0.008m, Drift = 0.0002m },
            new() { Type = MarketRegime.TREND_UP, Bars = 80, VolumeMultiplier = 1.5m, Volatility = 0.012m, Drift = 0.0006m },
            new() { Type = MarketRegime.RANGE, Bars = 40, VolumeMultiplier = 1.2m, Volatility = 0.009m, Drift = 0.0002m }
        },
        Overlays = new List<PatternOverlayConfig>
        {
            new() { Type = PatternOverlayType.BREAKOUT, AtBar = 175, Direction = "UP", VolumeBoost = 1.8m, NoiseBars = 3 }
        }
    };

    private static ScenarioConfig CreateLowConfidenceRangeConfig() => new()
    {
        Name = "Choppy Sideways",
        Description = "Frustrating sideways chop with false breakouts. Tests signal filtering in unclear conditions.",
        Seed = 200,
        BaseVolatility = 0.012m,
        Regimes = new List<RegimePhase>
        {
            new() { Type = MarketRegime.RANGE, Bars = 50, VolumeMultiplier = 0.8m, Volatility = 0.010m, Drift = 0.0001m },
            new() { Type = MarketRegime.TREND_UP, Bars = 25, VolumeMultiplier = 0.9m, Volatility = 0.011m, Drift = 0.0003m },
            new() { Type = MarketRegime.RANGE, Bars = 40, VolumeMultiplier = 0.7m, Volatility = 0.009m, Drift = -0.0001m },
            new() { Type = MarketRegime.TREND_DOWN, Bars = 25, VolumeMultiplier = 0.9m, Volatility = 0.011m, Drift = -0.0003m },
            new() { Type = MarketRegime.RANGE, Bars = 60, VolumeMultiplier = 0.8m, Volatility = 0.010m, Drift = 0.0m },
            new() { Type = MarketRegime.TREND_UP, Bars = 20, VolumeMultiplier = 0.85m, Volatility = 0.010m, Drift = 0.0002m },
            new() { Type = MarketRegime.RANGE, Bars = 50, VolumeMultiplier = 0.75m, Volatility = 0.009m, Drift = -0.0001m }
        },
        Overlays = new List<PatternOverlayConfig>()
    };

    private static ScenarioConfig CreateAtrTestHighVolConfig() => new()
    {
        Name = "Volatile Session",
        Description = "Elevated volatility day with wider price swings. Tests ATR-based position sizing.",
        Seed = 300,
        BaseVolatility = 0.014m,
        Regimes = new List<RegimePhase>
        {
            new() { Type = MarketRegime.RANGE, Bars = 40, VolumeMultiplier = 1.0m, Volatility = 0.010m, Drift = 0.0m },
            new() { Type = MarketRegime.HIGH_VOL, Bars = 60, VolumeMultiplier = 1.3m, Volatility = 0.018m, Drift = 0.0002m },
            new() { Type = MarketRegime.TREND_UP, Bars = 50, VolumeMultiplier = 1.4m, Volatility = 0.016m, Drift = 0.0005m },
            new() { Type = MarketRegime.HIGH_VOL, Bars = 40, VolumeMultiplier = 1.2m, Volatility = 0.020m, Drift = -0.0002m },
            new() { Type = MarketRegime.RANGE, Bars = 50, VolumeMultiplier = 1.1m, Volatility = 0.014m, Drift = 0.0001m },
            new() { Type = MarketRegime.TREND_DOWN, Bars = 40, VolumeMultiplier = 1.3m, Volatility = 0.017m, Drift = -0.0004m }
        },
        Overlays = new List<PatternOverlayConfig>
        {
            new() { Type = PatternOverlayType.BREAKOUT, AtBar = 95, Direction = "UP", VolumeBoost = 1.4m }
        }
    };

    private static ScenarioConfig CreateTrendReversalConfig() => new()
    {
        Name = "Trend Reversal",
        Description = "Healthy uptrend loses momentum → distribution top → gradual breakdown into downtrend",
        Seed = 400,
        BaseVolatility = 0.010m,
        Regimes = new List<RegimePhase>
        {
            new() { Type = MarketRegime.TREND_UP, Bars = 70, VolumeMultiplier = 1.2m, Volatility = 0.011m, Drift = 0.0005m },
            new() { Type = MarketRegime.TREND_UP, Bars = 40, VolumeMultiplier = 1.0m, Volatility = 0.010m, Drift = 0.0003m },
            new() { Type = MarketRegime.RANGE, Bars = 50, VolumeMultiplier = 0.9m, Volatility = 0.009m, Drift = 0.0m },
            new() { Type = MarketRegime.RANGE, Bars = 40, VolumeMultiplier = 1.1m, Volatility = 0.011m, Drift = -0.0001m },
            new() { Type = MarketRegime.TREND_DOWN, Bars = 60, VolumeMultiplier = 1.3m, Volatility = 0.013m, Drift = -0.0005m },
            new() { Type = MarketRegime.RANGE, Bars = 30, VolumeMultiplier = 1.0m, Volatility = 0.010m, Drift = -0.0002m }
        },
        Overlays = new List<PatternOverlayConfig>
        {
            new() { Type = PatternOverlayType.DOUBLE_TOP, AtBar = 100, ToBar = 160, Direction = "DOWN", VolumeBoost = 1.2m }
        }
    };

    private static ScenarioConfig CreateCrashScenarioConfig() => new()
    {
        Name = "Flash Crash",
        Description = "Normal trading → sudden sharp selloff → panic → gradual stabilization and recovery",
        Seed = 999,
        BaseVolatility = 0.012m,
        GapProbability = 0.1m,
        MaxGapPercent = 0.02m,
        Regimes = new List<RegimePhase>
        {
            new() { Type = MarketRegime.RANGE, Bars = 50, VolumeMultiplier = 0.9m, Volatility = 0.008m, Drift = 0.0001m },
            new() { Type = MarketRegime.TREND_UP, Bars = 40, VolumeMultiplier = 1.0m, Volatility = 0.009m, Drift = 0.0003m },
            new() { Type = MarketRegime.CRASH, Bars = 20, VolumeMultiplier = 2.0m, Volatility = 0.030m, Drift = -0.0020m },
            new() { Type = MarketRegime.HIGH_VOL, Bars = 40, VolumeMultiplier = 1.6m, Volatility = 0.022m, Drift = -0.0005m },
            new() { Type = MarketRegime.RANGE, Bars = 50, VolumeMultiplier = 1.2m, Volatility = 0.015m, Drift = 0.0m },
            new() { Type = MarketRegime.TREND_UP, Bars = 40, VolumeMultiplier = 1.1m, Volatility = 0.012m, Drift = 0.0004m },
            new() { Type = MarketRegime.RANGE, Bars = 60, VolumeMultiplier = 1.0m, Volatility = 0.010m, Drift = 0.0001m }
        },
        Overlays = new List<PatternOverlayConfig>
        {
            new() { Type = PatternOverlayType.GAP_AND_GO, AtBar = 88, Direction = "DOWN", VolumeBoost = 1.8m }
        }
    };

    private static ScenarioConfig CloneConfig(ScenarioConfig source)
    {
        var json = JsonSerializer.Serialize(source);
        return JsonSerializer.Deserialize<ScenarioConfig>(json) ?? CreateDefaultConfig();
    }

    #endregion
}
