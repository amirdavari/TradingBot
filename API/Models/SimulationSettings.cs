namespace API.Models;

/// <summary>
/// Configurable settings for the market simulation engine.
/// These control how realistic/volatile the mock data generation is.
/// </summary>
public class SimulationSettings
{
    /// <summary>
    /// Volatility scaling factor (0.05 = very smooth, 0.5 = very volatile)
    /// Default: 0.15 (15% of base volatility applied per bar)
    /// </summary>
    public decimal VolatilityScale { get; set; } = 0.15m;

    /// <summary>
    /// Drift scaling factor - how much trend drift affects price per bar
    /// Default: 0.1 (10% of configured drift)
    /// </summary>
    public decimal DriftScale { get; set; } = 0.1m;

    /// <summary>
    /// Mean reversion strength (0 = none, 1 = strong pull back to mean)
    /// Default: 0.3
    /// </summary>
    public decimal MeanReversionStrength { get; set; } = 0.3m;

    /// <summary>
    /// Fat tail probability multiplier (0 = no fat tails, 1 = normal, 2 = double)
    /// Default: 0.1 (10% of normal fat tail probability)
    /// </summary>
    public decimal FatTailMultiplier { get; set; } = 0.1m;

    /// <summary>
    /// Fat tail size range: min multiplier when fat tail occurs
    /// Default: 1.5
    /// </summary>
    public decimal FatTailMinSize { get; set; } = 1.5m;

    /// <summary>
    /// Fat tail size range: max multiplier when fat tail occurs
    /// Default: 2.5
    /// </summary>
    public decimal FatTailMaxSize { get; set; } = 2.5m;

    /// <summary>
    /// Maximum return per bar (clamping limit)
    /// Default: 0.02 (2%)
    /// </summary>
    public decimal MaxReturnPerBar { get; set; } = 0.02m;

    /// <summary>
    /// Live candle tick noise factor (how much the current candle jitters)
    /// Default: 0.01 (1% of volatility)
    /// </summary>
    public decimal LiveTickNoise { get; set; } = 0.01m;

    /// <summary>
    /// High/Low range multiplier (how wide the candle wicks are)
    /// Default: 0.3
    /// </summary>
    public decimal HighLowRangeMultiplier { get; set; } = 0.3m;

    /// <summary>
    /// Pattern overlay strength multiplier
    /// Default: 1.0 (normal), 0.5 = half strength, 2.0 = double
    /// </summary>
    public decimal PatternOverlayStrength { get; set; } = 1.0m;
}

/// <summary>
/// Database entity for persisting simulation settings.
/// Uses singleton pattern (Id = 1).
/// </summary>
public class SimulationSettingsEntity
{
    public int Id { get; set; } = 1;
    public decimal VolatilityScale { get; set; } = 0.15m;
    public decimal DriftScale { get; set; } = 0.1m;
    public decimal MeanReversionStrength { get; set; } = 0.3m;
    public decimal FatTailMultiplier { get; set; } = 0.1m;
    public decimal FatTailMinSize { get; set; } = 1.5m;
    public decimal FatTailMaxSize { get; set; } = 2.5m;
    public decimal MaxReturnPerBar { get; set; } = 0.02m;
    public decimal LiveTickNoise { get; set; } = 0.01m;
    public decimal HighLowRangeMultiplier { get; set; } = 0.3m;
    public decimal PatternOverlayStrength { get; set; } = 1.0m;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
