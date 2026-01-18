using API.Data;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

/// <summary>
/// Service for risk and position management in paper trading.
/// Calculates position sizes, validates trades, and enforces risk rules.
/// </summary>
public class RiskManagementService
{
    private readonly AccountService _accountService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RiskManagementService> _logger;

    public RiskManagementService(
        AccountService accountService,
        ApplicationDbContext context,
        ILogger<RiskManagementService> logger)
    {
        _accountService = accountService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets or creates risk settings from database.
    /// </summary>
    private async Task<RiskSettingsEntity> GetOrCreateRiskSettingsAsync()
    {
        var settings = await _context.RiskSettings.FirstOrDefaultAsync();
        if (settings == null)
        {
            // Create default settings
            settings = new RiskSettingsEntity
            {
                Id = 1,
                DefaultRiskPercent = 1.0m,
                MaxRiskPercent = 2.0m,
                MinRiskRewardRatio = 1.5m,
                MaxCapitalPercent = 20.0m,
                UpdatedAt = DateTime.UtcNow
            };
            _context.RiskSettings.Add(settings);
            await _context.SaveChangesAsync();
        }
        return settings;
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
        var settings = await GetOrCreateRiskSettingsAsync();
        var riskPercent = customRiskPercent ?? settings.DefaultRiskPercent;

        var result = new RiskCalculationDto
        {
            Symbol = symbol,
            EntryPrice = entryPrice,
            StopLoss = stopLoss,
            TakeProfit = takeProfit,
            RiskPercent = riskPercent,
            MaxCapitalPercent = settings.MaxCapitalPercent
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

        if (result.RiskRewardRatio < settings.MinRiskRewardRatio)
        {
            result.Messages.Add($"Risk/Reward ratio {result.RiskRewardRatio:F2} is below minimum {settings.MinRiskRewardRatio:F2}");
        }

        // Validate risk percent
        if (riskPercent > settings.MaxRiskPercent)
        {
            result.IsAllowed = false;
            result.Messages.Add($"Risk percent {riskPercent}% exceeds maximum {settings.MaxRiskPercent}%");
            return result;
        }

        // 1) Calculate maximum risk amount (target)
        var maxRiskAmount = account.Balance * (riskPercent / 100m);

        // 2) Calculate ideal position size based on risk
        var positionSizeByRisk = maxRiskAmount / riskPerShare;

        // 3) Calculate maximum capital allowed per trade
        var maxCapitalPerTrade = account.AvailableCash * (settings.MaxCapitalPercent / 100m);
        var positionSizeByCapital = maxCapitalPerTrade / entryPrice;

        // 4) Calculate maximum shares affordable with available cash
        var maxSharesByCash = account.AvailableCash / entryPrice;

        // 5) Take the minimum (limiting factor)
        result.PositionSize = Math.Min(Math.Min(positionSizeByRisk, positionSizeByCapital), maxSharesByCash);

        // 6) Check if any position is possible (minimum 0.001 shares for fractional trading)
        if (result.PositionSize < 0.001m)
        {
            result.IsAllowed = false;
            result.Messages.Add($"Position size too small. Entry: {entryPrice:F2}, Available: {account.AvailableCash:F2}");
            result.LimitingFactor = "NONE";
            result.RiskUtilization = 0;
            return result;
        }

        // Round to 4 decimal places (supports fractional shares)
        result.PositionSize = Math.Round(result.PositionSize, 4);

        // 7) Determine limiting factor
        if (result.PositionSize >= positionSizeByRisk && result.PositionSize >= positionSizeByCapital)
        {
            result.LimitingFactor = "CASH";
        }
        else if (result.PositionSize >= positionSizeByCapital && result.PositionSize < positionSizeByRisk)
        {
            result.LimitingFactor = "CAPITAL";
        }
        else
        {
            result.LimitingFactor = "RISK";
        }

        // 8) Calculate actual investment amount
        result.InvestAmount = result.PositionSize * entryPrice;

        // 9) Calculate actual risk (may be less than target if cash-limited)
        result.RiskAmount = result.PositionSize * riskPerShare;

        // 10) Calculate risk utilization
        result.RiskUtilization = maxRiskAmount > 0 ? result.RiskAmount / maxRiskAmount : 0;

        // 11) Calculate reward amount
        result.RewardAmount = result.PositionSize * rewardPerShare;

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
        
        if (result.LimitingFactor == "CASH")
        {
            result.Messages.Add($"Actual Risk: €{result.RiskAmount:F2} (target: €{maxRiskAmount:F2})");
        }
        else if (result.LimitingFactor == "CAPITAL")
        {
            result.Messages.Add($"Actual Risk: €{result.RiskAmount:F2} (target: €{maxRiskAmount:F2})");
        }

        _logger.LogInformation(
            "Risk calculated for {Symbol}: Position={Position}, InvestAmount={InvestAmount}, Risk={Risk}, LimitingFactor={LimitingFactor}, RiskUtil={RiskUtil:P0}",
            symbol, result.PositionSize, result.InvestAmount, result.RiskAmount, result.LimitingFactor, result.RiskUtilization);

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
        var settings = await GetOrCreateRiskSettingsAsync();

        // Check available cash
        if (investAmount > account.AvailableCash)
        {
            return (false, $"Insufficient available cash. Required: {investAmount:F2}, Available: {account.AvailableCash:F2}");
        }

        // Check risk amount
        var maxRiskAmount = account.Balance * (settings.MaxRiskPercent / 100m);
        if (riskAmount > maxRiskAmount)
        {
            return (false, $"Risk amount {riskAmount:F2} exceeds maximum {maxRiskAmount:F2}");
        }

        return (true, "Trade validated successfully");
    }

    /// <summary>
    /// Gets current risk settings.
    /// </summary>
    public async Task<RiskSettings> GetRiskSettingsAsync()
    {
        var settings = await GetOrCreateRiskSettingsAsync();
        return new RiskSettings
        {
            DefaultRiskPercent = settings.DefaultRiskPercent,
            MaxRiskPercent = settings.MaxRiskPercent,
            MinRiskRewardRatio = settings.MinRiskRewardRatio,
            MaxCapitalPercent = settings.MaxCapitalPercent
        };
    }

    /// <summary>
    /// Updates risk settings.
    /// </summary>
    public async Task<RiskSettings> UpdateRiskSettingsAsync(RiskSettings newSettings)
    {
        var settings = await GetOrCreateRiskSettingsAsync();
        
        settings.DefaultRiskPercent = newSettings.DefaultRiskPercent;
        settings.MaxRiskPercent = newSettings.MaxRiskPercent;
        settings.MinRiskRewardRatio = newSettings.MinRiskRewardRatio;
        settings.MaxCapitalPercent = newSettings.MaxCapitalPercent;
        settings.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return newSettings;
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
    public decimal MaxCapitalPercent { get; set; }
}
