using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// API Controller for risk and position management.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class RiskController : ControllerBase
{
    private readonly RiskManagementService _riskService;
    private readonly ILogger<RiskController> _logger;

    public RiskController(
        RiskManagementService riskService,
        ILogger<RiskController> logger)
    {
        _riskService = riskService;
        _logger = logger;
    }

    /// <summary>
    /// Calculates risk, position size, and investment amount for a trade.
    /// </summary>
    /// <param name="symbol">Stock symbol</param>
    /// <param name="entryPrice">Entry price</param>
    /// <param name="stopLoss">Stop loss price</param>
    /// <param name="takeProfit">Take profit price</param>
    /// <param name="riskPercent">Optional custom risk percentage (default: 1%)</param>
    [HttpGet("calculate")]
    [ProducesResponseType(typeof(RiskCalculationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RiskCalculationDto>> CalculateRisk(
        [FromQuery] string symbol,
        [FromQuery] decimal entryPrice,
        [FromQuery] decimal stopLoss,
        [FromQuery] decimal takeProfit,
        [FromQuery] decimal? riskPercent = null)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            return BadRequest("Symbol is required");

        if (entryPrice <= 0)
            return BadRequest("Entry price must be greater than zero");

        if (stopLoss <= 0)
            return BadRequest("Stop loss must be greater than zero");

        if (takeProfit <= 0)
            return BadRequest("Take profit must be greater than zero");

        if (riskPercent.HasValue && (riskPercent.Value <= 0 || riskPercent.Value > 10))
            return BadRequest("Risk percent must be between 0 and 10");

        try
        {
            var result = await _riskService.CalculateTradeRiskAsync(
                symbol, entryPrice, stopLoss, takeProfit, riskPercent);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating risk for {Symbol}", symbol);
            return StatusCode(500, "Failed to calculate risk");
        }
    }

    /// <summary>
    /// Gets current risk management settings.
    /// </summary>
    [HttpGet("settings")]
    [ProducesResponseType(typeof(RiskSettings), StatusCodes.Status200OK)]
    public async Task<ActionResult<RiskSettings>> GetSettings()
    {
        var settings = await _riskService.GetRiskSettingsAsync();
        return Ok(settings);
    }

    /// <summary>
    /// Updates risk management settings.
    /// </summary>
    [HttpPut("settings")]
    [ProducesResponseType(typeof(RiskSettings), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RiskSettings>> UpdateSettings([FromBody] RiskSettings settings)
    {
        if (settings.DefaultRiskPercent <= 0 || settings.DefaultRiskPercent > settings.MaxRiskPercent)
            return BadRequest("Invalid default risk percent");

        if (settings.MaxRiskPercent <= 0 || settings.MaxRiskPercent > 10)
            return BadRequest("Max risk percent must be between 0 and 10");

        if (settings.MinRiskRewardRatio < 0)
            return BadRequest("Min risk/reward ratio must be positive");

        if (settings.MaxCapitalPercent <= 0 || settings.MaxCapitalPercent > 100)
            return BadRequest("Max capital percent must be between 0 and 100");

        try
        {
            var updated = await _riskService.UpdateRiskSettingsAsync(settings);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating risk settings");
            return StatusCode(500, "Failed to update risk settings");
        }
    }
}
