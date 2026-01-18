using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Models;
using API.Data;
using API.Services;

namespace API.Controllers;

/// <summary>
/// Paper trades controller for managing simulated trades.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PaperTradesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly PaperTradeService _tradeService;
    private readonly SignalService _signalService;
    private readonly RiskManagementService _riskService;
    private readonly ILogger<PaperTradesController> _logger;

    public PaperTradesController(
        ApplicationDbContext context,
        PaperTradeService tradeService,
        SignalService signalService,
        RiskManagementService riskService,
        ILogger<PaperTradesController> logger)
    {
        _context = context;
        _tradeService = tradeService;
        _signalService = signalService;
        _riskService = riskService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all open trades.
    /// </summary>
    [HttpGet("open")]
    [ProducesResponseType(typeof(List<PaperTrade>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PaperTrade>>> GetOpenTrades()
    {
        try
        {
            var trades = await _tradeService.GetOpenTradesAsync();
            return Ok(trades);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching open trades");
            return StatusCode(500, "Failed to fetch open trades");
        }
    }

    /// <summary>
    /// Gets trade history (closed trades).
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(List<PaperTrade>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<PaperTrade>>> GetTradeHistory(
        [FromQuery] int limit = 50)
    {
        try
        {
            var trades = await _tradeService.GetTradeHistoryAsync(limit);
            return Ok(trades);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching trade history");
            return StatusCode(500, "Failed to fetch trade history");
        }
    }

    /// <summary>
    /// Automatically opens a trade based on current signal and risk calculation.
    /// </summary>
    [HttpPost("auto-execute")]
    [ProducesResponseType(typeof(PaperTrade), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaperTrade>> AutoExecuteTrade(
        [FromQuery] string symbol,
        [FromQuery] int timeframe = 5,
        [FromQuery] decimal? riskPercent = null)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            return BadRequest("Symbol is required");

        try
        {
            // Get current signal
            var signal = await _signalService.GenerateSignalAsync(symbol, timeframe);

            if (signal.Direction == "NONE")
            {
                return BadRequest("No valid signal for this symbol");
            }

            // Calculate risk
            var riskCalc = await _riskService.CalculateTradeRiskAsync(
                symbol, signal.Entry, signal.StopLoss, signal.TakeProfit, riskPercent);

            if (!riskCalc.IsAllowed)
            {
                return BadRequest(new { 
                    error = "Trade not allowed",
                    messages = riskCalc.Messages 
                });
            }

            // Open trade
            var result = await _tradeService.OpenTradeAsync(signal, riskCalc);

            if (!result.Success)
            {
                return BadRequest(new { error = result.Message });
            }

            _logger.LogInformation("Auto-executed trade for {Symbol}: {Message}", symbol, result.Message);

            return CreatedAtAction(nameof(GetOpenTrades), new { id = result.Trade!.Id }, result.Trade);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error auto-executing trade for {Symbol}", symbol);
            return StatusCode(500, "Failed to execute trade");
        }
    }

    /// <summary>
    /// Manually closes an open trade.
    /// </summary>
    [HttpPost("{id}/close")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CloseTrade(
        int id,
        [FromQuery] decimal exitPrice,
        [FromQuery] string reason = "CLOSED_MANUAL")
    {
        if (exitPrice <= 0)
            return BadRequest("Exit price must be greater than zero");

        try
        {
            var result = await _tradeService.CloseTradeAsync(id, exitPrice, reason);

            if (!result.Success)
            {
                return NotFound(new { error = result.Message });
            }

            return Ok(new { message = result.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing trade #{Id}", id);
            return StatusCode(500, "Failed to close trade");
        }
    }

    /// <summary>
    /// Gets unrealized PnL for an open trade.
    /// </summary>
    [HttpGet("{id}/unrealized-pnl")]
    [ProducesResponseType(typeof(decimal), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<decimal>> GetUnrealizedPnL(int id)
    {
        try
        {
            var trade = await _context.PaperTrades.FindAsync(id);

            if (trade == null)
            {
                return NotFound("Trade not found");
            }

            var pnl = await _tradeService.CalculateUnrealizedPnLAsync(trade);

            return Ok(pnl);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating unrealized PnL for trade #{Id}", id);
            return StatusCode(500, "Failed to calculate unrealized PnL");
        }
    }

    /// <summary>
    /// Gets trade statistics.
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(TradeStatistics), StatusCodes.Status200OK)]
    public async Task<ActionResult<TradeStatistics>> GetStatistics()
    {
        try
        {
            var stats = await _tradeService.GetTradeStatisticsAsync();
            return Ok(stats);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating trade statistics");
            return StatusCode(500, "Failed to calculate statistics");
        }
    }
}
