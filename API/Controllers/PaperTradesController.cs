using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using API.Models;
using API.Data;
using API.Hubs;
using API.Services;

namespace API.Controllers;

/// <summary>
/// Paper trades controller for managing simulated trades.
/// Pushes trade updates via SignalR.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PaperTradesController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly PaperTradeService _tradeService;
    private readonly SignalService _signalService;
    private readonly RiskManagementService _riskService;
    private readonly AccountService _accountService;
    private readonly IHubContext<TradingHub> _hubContext;
    private readonly ILogger<PaperTradesController> _logger;

    public PaperTradesController(
        ApplicationDbContext context,
        PaperTradeService tradeService,
        SignalService signalService,
        RiskManagementService riskService,
        AccountService accountService,
        IHubContext<TradingHub> hubContext,
        ILogger<PaperTradesController> logger)
    {
        _context = context;
        _tradeService = tradeService;
        _signalService = signalService;
        _riskService = riskService;
        _accountService = accountService;
        _hubContext = hubContext;
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
    /// Creates a new paper trade with explicit parameters.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PaperTrade), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PaperTrade>> CreateTrade([FromBody] CreateTradeRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Symbol))
            return BadRequest("Symbol is required");

        if (request.EntryPrice <= 0)
            return BadRequest("Entry price must be greater than zero");

        if (request.PositionSize <= 0)
            return BadRequest("Position size must be greater than zero");

        try
        {
            // Create signal from request
            var signal = new TradeSignal
            {
                Symbol = request.Symbol,
                Direction = request.Direction,
                Entry = request.EntryPrice,
                StopLoss = request.StopLoss,
                TakeProfit = request.TakeProfit,
                Confidence = request.Confidence,
                Reasons = request.Reasons
            };

            // Create risk calculation from request
            var riskCalc = new RiskCalculationDto
            {
                Symbol = request.Symbol,
                EntryPrice = request.EntryPrice,
                StopLoss = request.StopLoss,
                TakeProfit = request.TakeProfit,
                PositionSize = request.PositionSize,
                InvestAmount = request.InvestAmount,
                IsAllowed = true,
                RiskPercent = request.RiskPercent
            };

            // Open trade
            var result = await _tradeService.OpenTradeAsync(signal, riskCalc);

            if (!result.Success)
            {
                return BadRequest(new { error = result.Message });
            }

            _logger.LogInformation("Created trade for {Symbol}: {Message}", request.Symbol, result.Message);

            // Push trade update and account update via SignalR
            await _hubContext.Clients.All.SendAsync(TradingHubMethods.ReceiveTradeUpdate, result.Trade);
            var account = await _accountService.GetOrCreateAccountAsync();
            await _hubContext.Clients.All.SendAsync(TradingHubMethods.ReceiveAccountUpdate, account);

            return CreatedAtAction(nameof(GetOpenTrades), new { id = result.Trade!.Id }, result.Trade);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating trade for {Symbol}", request.Symbol);
            return StatusCode(500, "Failed to create trade");
        }
    }

    /// <summary>
    /// Gets trade history (closed trades from TradeHistory table).
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(List<TradeHistory>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TradeHistory>>> GetTradeHistory(
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
                return BadRequest(new
                {
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

            // Push trade update and account update via SignalR
            await _hubContext.Clients.All.SendAsync(TradingHubMethods.ReceiveTradeUpdate, result.Trade);
            var account = await _accountService.GetOrCreateAccountAsync();
            await _hubContext.Clients.All.SendAsync(TradingHubMethods.ReceiveAccountUpdate, account);

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
    /// If exitPrice is not provided, uses current market price.
    /// </summary>
    [HttpPost("{id}/close")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CloseTrade(
        int id,
        [FromQuery] decimal? exitPrice = null,
        [FromQuery] string reason = "CLOSED_MANUAL")
    {
        try
        {
            // If no exit price provided, get current market price
            if (!exitPrice.HasValue || exitPrice.Value <= 0)
            {
                var trade = await _context.PaperTrades.FindAsync(id);
                if (trade == null)
                {
                    return NotFound(new { error = "Trade not found" });
                }

                // Get current price from market data
                var candles = await _context.PaperTrades
                    .Where(t => t.Id == id)
                    .Select(t => t.Symbol)
                    .FirstOrDefaultAsync();

                if (candles == null)
                {
                    return NotFound(new { error = "Trade not found" });
                }

                // Use signal service to get current price
                var signal = await _signalService.GenerateSignalAsync(trade.Symbol, 5);
                exitPrice = signal.Entry; // Use current entry price as exit price
            }

            var result = await _tradeService.CloseTradeAsync(id, exitPrice.Value, reason);

            if (!result.Success)
            {
                return NotFound(new { error = result.Message });
            }

            // Get the closed trade to push via SignalR
            var closedTrade = await _context.PaperTrades.FindAsync(id);
            if (closedTrade != null)
            {
                await _hubContext.Clients.All.SendAsync(
                    TradingHubMethods.ReceiveTradeClosed,
                    new { Trade = closedTrade, Reason = reason });

                var account = await _accountService.GetOrCreateAccountAsync();
                await _hubContext.Clients.All.SendAsync(TradingHubMethods.ReceiveAccountUpdate, account);
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
