using API.Data;
using API.Models;

namespace API.Services;

/// <summary>
/// Service for risk and position management in paper trading.
/// Calculates position sizes, validates trades, and enforces risk rules.
/// </summary>
public class RiskManagementService
{
    private readonly AccountService _accountService;
    private readonly ILogger<RiskManagementService> _logger;

    // Risk settings (can be made configurable later)
    private const decimal DEFAULT_RISK_PERCENT = 1.0m; // 1% of account per trade
    private const decimal MIN_RISK_REWARD_RATIO = 1.5m; // Minimum 1:1.5 risk/reward
    private const decimal MAX_RISK_PERCENT = 2.0m; // Maximum 2% per trade

    public RiskManagementService(
        AccountService accountService,
        ILogger<RiskManagementService> logger)
    {
        _accountService = accountService;
        _logger = logger;
    }

    /// <summary>
    /// Calculates risk, position size, and investment amount for a trade.
    /// </summary>
    public async Task<RiskCalculationDto> CalculateTradeRiskAsync(
        string symbol,
        decimal entryPrice,
        decimal stopLoss,
        decimal takeProfit,
        decimal? customRiskPercent = null)
    {
        var account = await _accountService.GetOrCreateAccountAsync();
        var riskPercent = customRiskPercent ?? DEFAULT_RISK_PERCENT;

        var result = new RiskCalculationDto
        {
            Symbol = symbol,
            EntryPrice = entryPrice,
            StopLoss = stopLoss,
            TakeProfit = takeProfit,
            RiskPercent = riskPercent
        };

        // Validate inputs
        if (entryPrice <= 0 || stopLoss <= 0 || takeProfit <= 0)
        {
            result.IsAllowed = false;
            result.Messages.Add("Invalid prices: All prices must be greater than zero");
            return result;
        }

        // Calculate risk per share (distance from entry to stop loss)
        var riskPerShare = Math.Abs(entryPrice - stopLoss);
        
        if (riskPerShare == 0)
        {
            result.IsAllowed = false;
            result.Messages.Add("Stop loss cannot be equal to entry price");
            return result;
        }

        // Calculate reward per share (distance from entry to take profit)
        var rewardPerShare = Math.Abs(takeProfit - entryPrice);

        // Calculate risk/reward ratio
        result.RiskRewardRatio = rewardPerShare / riskPerShare;

        if (result.RiskRewardRatio < MIN_RISK_REWARD_RATIO)
        {
            result.Messages.Add($"Risk/Reward ratio {result.RiskRewardRatio:F2} is below minimum {MIN_RISK_REWARD_RATIO:F2}");
        }

        // Validate risk percent
        if (riskPercent > MAX_RISK_PERCENT)
        {
            result.IsAllowed = false;
            result.Messages.Add($"Risk percent {riskPercent}% exceeds maximum {MAX_RISK_PERCENT}%");
            return result;
        }

        // Calculate risk amount (how much we're willing to lose)
        result.RiskAmount = account.Balance * (riskPercent / 100m);

        // Calculate position size (number of shares)
        result.PositionSize = Math.Floor(result.RiskAmount / riskPerShare);

        if (result.PositionSize <= 0)
        {
            result.IsAllowed = false;
            result.Messages.Add("Position size too small (less than 1 share)");
            return result;
        }

        // Calculate investment amount (capital needed)
        result.InvestAmount = result.PositionSize * entryPrice;

        // Calculate actual reward amount
        result.RewardAmount = result.PositionSize * rewardPerShare;

        // Check if we have enough available cash
        if (result.InvestAmount > account.AvailableCash)
        {
            result.IsAllowed = false;
            result.Messages.Add($"Insufficient available cash. Required: {result.InvestAmount:F2}, Available: {account.AvailableCash:F2}");
            return result;
        }

        // Validate stop loss direction
        var direction = takeProfit > entryPrice ? "LONG" : "SHORT";
        if (direction == "LONG" && stopLoss >= entryPrice)
        {
            result.IsAllowed = false;
            result.Messages.Add("For LONG trades, stop loss must be below entry price");
            return result;
        }
        if (direction == "SHORT" && stopLoss <= entryPrice)
        {
            result.IsAllowed = false;
            result.Messages.Add("For SHORT trades, stop loss must be above entry price");
            return result;
        }

        // All checks passed
        result.IsAllowed = true;
        result.Messages.Add($"Trade allowed: {result.PositionSize} shares @ {entryPrice:F2}");
        result.Messages.Add($"Risk: {result.RiskAmount:F2} ({riskPercent}% of balance)");
        result.Messages.Add($"Reward: {result.RewardAmount:F2}");
        result.Messages.Add($"R/R Ratio: 1:{result.RiskRewardRatio:F2}");

        _logger.LogInformation(
            "Risk calculated for {Symbol}: Position={Position}, InvestAmount={InvestAmount}, Risk={Risk}, R/R={RR}",
            symbol, result.PositionSize, result.InvestAmount, result.RiskAmount, result.RiskRewardRatio);

        return result;
    }

    /// <summary>
    /// Validates if a trade can be opened based on risk rules.
    /// </summary>
    public async Task<(bool IsValid, string Message)> ValidateTradeAsync(
        decimal investAmount,
        decimal riskAmount)
    {
        var account = await _accountService.GetOrCreateAccountAsync();

        // Check available cash
        if (investAmount > account.AvailableCash)
        {
            return (false, $"Insufficient available cash. Required: {investAmount:F2}, Available: {account.AvailableCash:F2}");
        }

        // Check risk amount
        var maxRiskAmount = account.Balance * (MAX_RISK_PERCENT / 100m);
        if (riskAmount > maxRiskAmount)
        {
            return (false, $"Risk amount {riskAmount:F2} exceeds maximum {maxRiskAmount:F2}");
        }

        return (true, "Trade validated successfully");
    }

    /// <summary>
    /// Gets current risk settings.
    /// </summary>
    public RiskSettings GetRiskSettings()
    {
        return new RiskSettings
        {
            DefaultRiskPercent = DEFAULT_RISK_PERCENT,
            MaxRiskPercent = MAX_RISK_PERCENT,
            MinRiskRewardRatio = MIN_RISK_REWARD_RATIO
        };
    }
}

/// <summary>
/// Risk management settings.
/// </summary>
public class RiskSettings
{
    public decimal DefaultRiskPercent { get; set; }
    public decimal MaxRiskPercent { get; set; }
    public decimal MinRiskRewardRatio { get; set; }
}
