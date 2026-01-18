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
    private readonly SignalService _signalService;
    private readonly IMarketDataProvider _marketDataProvider;
    private readonly ILogger<PaperTradesController> _logger;

    public PaperTradesController(
        ApplicationDbContext context,
        SignalService signalService,
        IMarketDataProvider marketDataProvider,
        ILogger<PaperTradesController> logger)
    {
        _context = context;
        _signalService = signalService;
        _marketDataProvider = marketDataProvider;
        _logger = logger;
    }

    /// <summary>
    /// Create a new paper BUY trade (LONG position).
    /// Backend calculates quantity, entry, stop-loss, and take-profit based on current signal.
    /// </summary>
    [HttpPost("buy")]
    public async Task<ActionResult<PaperTradeResponse>> CreateBuyTrade([FromBody] CreatePaperTradeRequest request)
    {
        try
        {
            if (request.Direction != "LONG")
            {
                return BadRequest(new { error = "Buy trades must have direction 'LONG'" });
            }

            return await CreatePaperTrade(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating BUY paper trade for {Symbol}", request.Symbol);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Create a new paper SELL trade (SHORT position).
    /// Backend calculates quantity, entry, stop-loss, and take-profit based on current signal.
    /// </summary>
    [HttpPost("sell")]
    public async Task<ActionResult<PaperTradeResponse>> CreateSellTrade([FromBody] CreatePaperTradeRequest request)
    {
        try
        {
            if (request.Direction != "SHORT")
            {
                return BadRequest(new { error = "Sell trades must have direction 'SHORT'" });
            }

            return await CreatePaperTrade(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating SELL paper trade for {Symbol}", request.Symbol);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all open paper trades.
    /// </summary>
    [HttpGet("open")]
    public async Task<ActionResult<List<PaperTradeResponse>>> GetOpenTrades()
    {
        try
        {
            var openTrades = await _context.PaperTrades
                .Where(t => t.Status == "OPEN")
                .OrderByDescending(t => t.OpenedAt)
                .ToListAsync();

            // Calculate current P&L for open trades
            var responses = new List<PaperTradeResponse>();
            foreach (var trade in openTrades)
            {
                var currentPrice = await GetCurrentPrice(trade.Symbol);
                var (pnl, pnlPercent) = CalculatePnL(trade, currentPrice);

                responses.Add(MapToResponse(trade, pnl, pnlPercent));
            }

            _logger.LogInformation("Retrieved {Count} open paper trades", responses.Count);
            return Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving open paper trades");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Get all paper trades (open and closed).
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<PaperTradeResponse>>> GetAllTrades()
    {
        try
        {
            var trades = await _context.PaperTrades
                .OrderByDescending(t => t.OpenedAt)
                .ToListAsync();

            var responses = new List<PaperTradeResponse>();
            foreach (var trade in trades)
            {
                decimal? pnl = trade.PnL;
                decimal? pnlPercent = trade.PnLPercent;

                // Calculate current P&L for open trades
                if (trade.Status == "OPEN")
                {
                    var currentPrice = await GetCurrentPrice(trade.Symbol);
                    (pnl, pnlPercent) = CalculatePnL(trade, currentPrice);
                }

                responses.Add(MapToResponse(trade, pnl, pnlPercent));
            }

            return Ok(responses);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving paper trades");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    /// <summary>
    /// Close a paper trade manually.
    /// </summary>
    [HttpPost("{id}/close")]
    public async Task<ActionResult<PaperTradeResponse>> CloseTrade(int id)
    {
        try
        {
            var trade = await _context.PaperTrades.FindAsync(id);
            if (trade == null)
            {
                return NotFound(new { error = $"Paper trade with ID {id} not found" });
            }

            if (trade.Status != "OPEN")
            {
                return BadRequest(new { error = "Trade is already closed" });
            }

            var currentPrice = await GetCurrentPrice(trade.Symbol);
            var (pnl, pnlPercent) = CalculatePnL(trade, currentPrice);

            trade.Status = "CLOSED_MANUAL";
            trade.ExitPrice = currentPrice;
            trade.PnL = pnl;
            trade.PnLPercent = pnlPercent;
            trade.ClosedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Closed paper trade {Id} for {Symbol} at {Price} with P&L: {PnL} ({PnLPercent}%)", 
                id, trade.Symbol, currentPrice, pnl, pnlPercent);

            return Ok(MapToResponse(trade, pnl, pnlPercent));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing paper trade {Id}", id);
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // ===== Private Helper Methods =====

    private async Task<ActionResult<PaperTradeResponse>> CreatePaperTrade(CreatePaperTradeRequest request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Symbol))
        {
            return BadRequest(new { error = "Symbol is required" });
        }

        if (request.InvestAmount <= 0)
        {
            return BadRequest(new { error = "Investment amount must be greater than 0" });
        }

        // Get current signal
        var signal = await _signalService.GenerateSignalAsync(request.Symbol, request.Timeframe);
        
        if (signal.Direction == "NONE")
        {
            return BadRequest(new { error = "No trading signal available for this symbol" });
        }

        if (signal.Direction != request.Direction)
        {
            return BadRequest(new { error = $"Signal direction ({signal.Direction}) does not match request direction ({request.Direction})" });
        }

        // Calculate quantity based on invest amount and entry price
        var quantity = (int)(request.InvestAmount / signal.Entry);
        if (quantity <= 0)
        {
            return BadRequest(new { error = "Investment amount too low for at least 1 share" });
        }

        // Create paper trade
        var paperTrade = new PaperTrade
        {
            Symbol = request.Symbol.ToUpper(),
            Direction = request.Direction,
            EntryPrice = signal.Entry,
            StopLoss = signal.StopLoss,
            TakeProfit = signal.TakeProfit,
            Quantity = quantity,
            Confidence = signal.Confidence,
            Reasons = signal.Reasons,
            Status = "OPEN",
            OpenedAt = DateTime.UtcNow
        };

        _context.PaperTrades.Add(paperTrade);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created {Direction} paper trade {Id} for {Symbol}: {Quantity} shares @ {Price}", 
            request.Direction, paperTrade.Id, request.Symbol, quantity, signal.Entry);

        var response = MapToResponse(paperTrade, null, null);
        response.InvestAmount = request.InvestAmount;

        return CreatedAtAction(nameof(GetAllTrades), new { id = paperTrade.Id }, response);
    }

    private async Task<decimal> GetCurrentPrice(string symbol)
    {
        try
        {
            // Get the most recent candle to determine current price
            var candles = await _marketDataProvider.GetCandlesAsync(symbol, 1, "1d");
            if (candles.Count > 0)
            {
                return candles.Last().Close;
            }

            _logger.LogWarning("No candles found for {Symbol}, using 0 as current price", symbol);
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting current price for {Symbol}", symbol);
            return 0;
        }
    }

    private (decimal pnl, decimal pnlPercent) CalculatePnL(PaperTrade trade, decimal currentPrice)
    {
        if (currentPrice == 0)
        {
            return (0, 0);
        }

        decimal pnl;
        decimal pnlPercent;

        if (trade.Direction == "LONG")
        {
            pnl = (currentPrice - trade.EntryPrice) * trade.Quantity;
            pnlPercent = ((currentPrice - trade.EntryPrice) / trade.EntryPrice) * 100;
        }
        else // SHORT
        {
            pnl = (trade.EntryPrice - currentPrice) * trade.Quantity;
            pnlPercent = ((trade.EntryPrice - currentPrice) / trade.EntryPrice) * 100;
        }

        return (Math.Round(pnl, 2), Math.Round(pnlPercent, 2));
    }

    private PaperTradeResponse MapToResponse(PaperTrade trade, decimal? pnl, decimal? pnlPercent)
    {
        return new PaperTradeResponse
        {
            Id = trade.Id,
            Symbol = trade.Symbol,
            Direction = trade.Direction,
            EntryPrice = trade.EntryPrice,
            StopLoss = trade.StopLoss,
            TakeProfit = trade.TakeProfit,
            Quantity = trade.Quantity,
            InvestAmount = trade.EntryPrice * trade.Quantity,
            Confidence = trade.Confidence,
            Reasons = trade.Reasons,
            Status = trade.Status,
            PnL = pnl,
            PnLPercent = pnlPercent,
            OpenedAt = trade.OpenedAt
        };
    }
}
