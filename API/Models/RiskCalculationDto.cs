namespace API.Models;

/// <summary>
/// DTO for risk calculation results.
/// Used to calculate position size and investment amount based on risk management rules.
/// </summary>
public class RiskCalculationDto
{
    /// <summary>
    /// Symbol for which risk is calculated.
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Entry price for the trade.
    /// </summary>
    public decimal EntryPrice { get; set; }

    /// <summary>
    /// Stop loss price.
    /// </summary>
    public decimal StopLoss { get; set; }

    /// <summary>
    /// Take profit price.
    /// </summary>
    public decimal TakeProfit { get; set; }

    /// <summary>
    /// Calculated investment amount (capital to allocate for this trade).
    /// </summary>
    public decimal InvestAmount { get; set; }

    /// <summary>
    /// Calculated position size (number of shares/units).
    /// </summary>
    public decimal PositionSize { get; set; }

    /// <summary>
    /// Risk amount in currency (potential loss if stop loss is hit).
    /// </summary>
    public decimal RiskAmount { get; set; }

    /// <summary>
    /// Risk percentage relative to account balance.
    /// </summary>
    public decimal RiskPercent { get; set; }

    /// <summary>
    /// Reward amount in currency (potential profit if take profit is hit).
    /// </summary>
    public decimal RewardAmount { get; set; }

    /// <summary>
    /// Risk/Reward ratio (e.g., 1:2 means risking $1 to make $2).
    /// </summary>
    public decimal RiskRewardRatio { get; set; }

    /// <summary>
    /// Whether the trade is allowed based on risk rules.
    /// </summary>
    public bool IsAllowed { get; set; }

    /// <summary>
    /// Reasons why trade might not be allowed or important risk information.
    /// </summary>
    public List<string> Messages { get; set; } = new();
}
