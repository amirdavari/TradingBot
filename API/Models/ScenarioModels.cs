using System.Text.Json.Serialization;

namespace API.Models;

#region Enums

/// <summary>
/// Market regime types that control simulation parameters.
/// </summary>
public enum MarketRegime
{
    /// <summary>Strong upward trend with positive drift</summary>
    TREND_UP,

    /// <summary>Strong downward trend with negative drift</summary>
    TREND_DOWN,

    /// <summary>Sideways/consolidation with mean-reversion</summary>
    RANGE,

    /// <summary>High volatility regime (e.g., earnings, news)</summary>
    HIGH_VOL,

    /// <summary>Low volatility regime (calm market)</summary>
    LOW_VOL,

    /// <summary>Crash/panic selling regime</summary>
    CRASH,

    /// <summary>News spike - sudden move with quick reversion</summary>
    NEWS_SPIKE
}

/// <summary>
/// Pattern overlay types for testing specific chart patterns.
/// </summary>
public enum PatternOverlayType
{
    /// <summary>Breakout from range with volume spike</summary>
    BREAKOUT,

    /// <summary>Pullback after trend move</summary>
    PULLBACK,

    /// <summary>Double top reversal pattern</summary>
    DOUBLE_TOP,

    /// <summary>Double bottom reversal pattern</summary>
    DOUBLE_BOTTOM,

    /// <summary>Head and shoulders reversal</summary>
    HEAD_SHOULDERS,

    /// <summary>Ascending/descending/symmetric triangle</summary>
    TRIANGLE,

    /// <summary>Flag/pennant continuation pattern</summary>
    FLAG,

    /// <summary>Gap and go momentum pattern</summary>
    GAP_AND_GO,

    /// <summary>Mean reversion bounce from support/resistance</summary>
    MEAN_REVERSION
}

/// <summary>
/// Triangle subtypes for pattern overlay
/// </summary>
public enum TriangleType
{
    ASCENDING,
    DESCENDING,
    SYMMETRIC
}

#endregion

#region Configuration Models

/// <summary>
/// Defines a regime phase within a scenario.
/// </summary>
public class RegimePhase
{
    /// <summary>Type of market regime</summary>
    public MarketRegime Type { get; set; } = MarketRegime.RANGE;

    /// <summary>Number of bars this regime lasts</summary>
    public int Bars { get; set; } = 100;

    /// <summary>Volatility override (0 = use regime default)</summary>
    public decimal? Volatility { get; set; }

    /// <summary>Drift override for trends (-1 to 1, 0 = use regime default)</summary>
    public decimal? Drift { get; set; }

    /// <summary>Volume multiplier (1.0 = normal)</summary>
    public decimal VolumeMultiplier { get; set; } = 1.0m;
}

/// <summary>
/// Defines a pattern overlay to inject into the simulation.
/// </summary>
public class PatternOverlayConfig
{
    /// <summary>Type of pattern to generate</summary>
    public PatternOverlayType Type { get; set; }

    /// <summary>Bar number where pattern starts (relative to scenario start)</summary>
    public int AtBar { get; set; }

    /// <summary>Bar number where pattern ends (for multi-bar patterns)</summary>
    public int? ToBar { get; set; }

    /// <summary>Direction of the pattern move (UP/DOWN)</summary>
    public string Direction { get; set; } = "UP";

    /// <summary>Volume boost multiplier at pattern trigger</summary>
    public decimal VolumeBoost { get; set; } = 2.0m;

    /// <summary>Depth of pullback in ATR multiples</summary>
    public decimal? DepthATR { get; set; }

    /// <summary>Triangle subtype (for TRIANGLE patterns)</summary>
    public TriangleType? TriangleSubtype { get; set; }

    /// <summary>Noise tolerance in bars (±) for realistic timing</summary>
    public int NoiseBars { get; set; } = 2;
}

/// <summary>
/// Complete scenario configuration for market simulation.
/// Using record for immutability and 'with' expression support.
/// </summary>
public record ScenarioConfig
{
    /// <summary>Unique name for the scenario</summary>
    public string Name { get; init; } = "Default";

    /// <summary>Optional description</summary>
    public string? Description { get; init; }

    /// <summary>Random seed for reproducibility (null = random)</summary>
    public int? Seed { get; init; }

    /// <summary>Symbol to apply scenario to (null = all symbols)</summary>
    public string? Symbol { get; init; }

    /// <summary>Starting price (null = use symbol's base price)</summary>
    public decimal? StartPrice { get; init; }

    /// <summary>Timeframe in minutes (1, 5, 15)</summary>
    public int Timeframe { get; set; } = 5;

    /// <summary>Sequence of regime phases</summary>
    public List<RegimePhase> Regimes { get; init; } = new();

    /// <summary>Pattern overlays to inject</summary>
    public List<PatternOverlayConfig> Overlays { get; init; } = new();

    /// <summary>Base volatility (default 2%)</summary>
    public decimal BaseVolatility { get; init; } = 0.02m;

    /// <summary>Gap probability between sessions (0-1)</summary>
    public decimal GapProbability { get; init; } = 0.1m;

    /// <summary>Maximum gap size as percentage</summary>
    public decimal MaxGapPercent { get; init; } = 0.03m;

    /// <summary>Whether scenario is active</summary>
    public bool IsActive { get; init; } = true;
}

#endregion

#region Regime Parameters

/// <summary>
/// Parameters that control market behavior for each regime.
/// </summary>
public class RegimeParameters
{
    /// <summary>Volatility multiplier (1.0 = base volatility)</summary>
    public decimal VolatilityMultiplier { get; set; } = 1.0m;

    /// <summary>Drift per bar (positive = up, negative = down)</summary>
    public decimal Drift { get; set; } = 0m;

    /// <summary>Mean reversion strength (0 = none, 1 = strong)</summary>
    public decimal MeanReversion { get; set; } = 0m;

    /// <summary>Gap probability modifier</summary>
    public decimal GapProbabilityModifier { get; set; } = 1.0m;

    /// <summary>Fat tail probability (large moves)</summary>
    public decimal FatTailProbability { get; set; } = 0.05m;

    /// <summary>Volume multiplier</summary>
    public decimal VolumeMultiplier { get; set; } = 1.0m;

    /// <summary>Gets default parameters for a regime type</summary>
    public static RegimeParameters GetDefaults(MarketRegime regime) => regime switch
    {
        MarketRegime.TREND_UP => new RegimeParameters
        {
            VolatilityMultiplier = 1.0m,
            Drift = 0.001m,  // +0.1% per bar average
            MeanReversion = 0.1m,
            VolumeMultiplier = 1.2m
        },
        MarketRegime.TREND_DOWN => new RegimeParameters
        {
            VolatilityMultiplier = 1.2m,  // Downtrends often more volatile
            Drift = -0.001m,
            MeanReversion = 0.1m,
            VolumeMultiplier = 1.3m
        },
        MarketRegime.RANGE => new RegimeParameters
        {
            VolatilityMultiplier = 0.8m,
            Drift = 0m,
            MeanReversion = 0.5m,  // Strong mean reversion
            VolumeMultiplier = 0.9m
        },
        MarketRegime.HIGH_VOL => new RegimeParameters
        {
            VolatilityMultiplier = 2.0m,
            Drift = 0m,
            MeanReversion = 0.2m,
            FatTailProbability = 0.15m,
            VolumeMultiplier = 1.8m
        },
        MarketRegime.LOW_VOL => new RegimeParameters
        {
            VolatilityMultiplier = 0.5m,
            Drift = 0m,
            MeanReversion = 0.3m,
            FatTailProbability = 0.02m,
            VolumeMultiplier = 0.6m
        },
        MarketRegime.CRASH => new RegimeParameters
        {
            VolatilityMultiplier = 3.0m,
            Drift = -0.005m,  // -0.5% per bar
            MeanReversion = 0.05m,  // Little mean reversion during crash
            FatTailProbability = 0.3m,
            VolumeMultiplier = 3.0m
        },
        MarketRegime.NEWS_SPIKE => new RegimeParameters
        {
            VolatilityMultiplier = 2.5m,
            Drift = 0m,  // Direction set by overlay
            MeanReversion = 0.4m,  // Quick reversion after spike
            FatTailProbability = 0.2m,
            VolumeMultiplier = 2.5m
        },
        _ => new RegimeParameters()
    };
}

#endregion

#region DTOs

/// <summary>
/// DTO for scenario preset (summary without full config)
/// </summary>
public class ScenarioPresetDto
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
    public bool IsBuiltIn { get; set; }
    public int TotalBars { get; set; }
    public List<string> Regimes { get; set; } = new();
    public List<string> Patterns { get; set; } = new();
}

/// <summary>
/// DTO for current scenario state
/// </summary>
public class ScenarioStateDto
{
    public bool IsEnabled { get; set; }
    public ScenarioConfig? ActiveConfig { get; set; }
    public List<string> AvailablePresets { get; set; } = new();
    public List<SymbolScenarioAssignment> SymbolAssignments { get; set; } = new();
}

/// <summary>
/// DTO for symbol-to-scenario assignment
/// </summary>
public class SymbolScenarioAssignment
{
    public string Symbol { get; set; } = "";
    public string ScenarioPreset { get; set; } = "";
    public string Strategy { get; set; } = "VWAP Momentum";
}

/// <summary>
/// Request to apply a scenario
/// </summary>
public class ApplyScenarioRequest
{
    /// <summary>Name of preset to apply (or "custom" for custom config)</summary>
    public string PresetName { get; set; } = "";

    /// <summary>Custom configuration (when PresetName = "custom")</summary>
    public ScenarioConfig? CustomConfig { get; set; }
}

#endregion

#region Database Entity

/// <summary>
/// Database entity for persisting active scenario configuration.
/// Uses singleton pattern (Id = 1).
/// </summary>
public class ScenarioConfigEntity
{
    /// <summary>Singleton ID (always 1)</summary>
    public int Id { get; set; } = 1;

    /// <summary>Name of active preset (or "custom")</summary>
    public string ActivePreset { get; set; } = "Default";

    /// <summary>Full configuration as JSON string</summary>
    public string ConfigJson { get; set; } = "{}";

    /// <summary>Whether scenario simulation is enabled</summary>
    public bool IsEnabled { get; set; } = false;

    /// <summary>Last update timestamp</summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

#endregion
