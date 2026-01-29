namespace API.Models;

/// <summary>
/// Entity for persisting risk management settings.
/// </summary>
public class RiskSettingsEntity
{
    public int Id { get; set; }
    
    /// <summary>
    /// Default risk percentage per trade (e.g., 1%).
    /// </summary>
    public decimal DefaultRiskPercent { get; set; } = 1.0m;
    
    /// <summary>
    /// Maximum risk percentage allowed per trade (e.g., 2%).
    /// </summary>
    public decimal MaxRiskPercent { get; set; } = 2.0m;
    
    /// <summary>
    /// Minimum risk/reward ratio required (e.g., 1.5).
    /// </summary>
    public decimal MinRiskRewardRatio { get; set; } = 1.5m;
    
    /// <summary>
    /// Maximum capital allocation per trade as percentage (e.g., 20%).
    /// </summary>
    public decimal MaxCapitalPercent { get; set; } = 20.0m;
    
    // === Auto-Trade Settings ===
    
    /// <summary>
    /// Whether auto-trading is enabled.
    /// </summary>
    public bool AutoTradeEnabled { get; set; } = false;
    
    /// <summary>
    /// Minimum confidence level required for auto-trade (0-100).
    /// </summary>
    public int AutoTradeMinConfidence { get; set; } = 100;
    
    /// <summary>
    /// Risk percentage to use for auto-trades.
    /// </summary>
    public decimal AutoTradeRiskPercent { get; set; } = 1.0m;
    
    /// <summary>
    /// Maximum number of concurrent auto-trades allowed.
    /// </summary>
    public int AutoTradeMaxConcurrent { get; set; } = 3;
    
    // === Dashboard Settings ===
    
    /// <summary>
    /// Selected chart timeframe in minutes (1, 5, or 15).
    /// Used by background services for scanner updates.
    /// </summary>
    public int SelectedTimeframe { get; set; } = 5;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
