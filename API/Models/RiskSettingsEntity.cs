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
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
