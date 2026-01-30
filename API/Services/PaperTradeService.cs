using API.Data;
using API.Models;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

/// <summary>
/// Service for managing paper trade lifecycle.
/// Handles opening, closing, and monitoring trades.
/// </summary>
public class PaperTradeService
{
    private readonly ApplicationDbContext _context;
    private readonly AccountService _accountService;
    private readonly RiskManagementService _riskService;
    private readonly IMarketDataProvider _marketDataProvider;
    private readonly IMarketTimeProvider _timeProvider;
    private readonly ILogger<PaperTradeService> _logger;

    public PaperTradeService(
        ApplicationDbContext context,
        AccountService accountService,
        RiskManagementService riskService,
        IMarketDataProvider marketDataProvider,
        IMarketTimeProvider timeProvider,
        ILogger<PaperTradeService> logger)
    {
        _context = context;
        _accountService = accountService;
        _riskService = riskService;
        _marketDataProvider = marketDataProvider;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <summary>
    /// Opens a new paper trade based on a signal and risk calculation.
    /// Multiple trades per symbol are allowed (e.g., different strategies, timeframes, or directions).
    /// </summary>
    public async Task<(bool Success, string Message, PaperTrade? Trade)> OpenTradeAsync(
        TradeSignal signal,
        RiskCalculationDto riskCalc)
    {
        // Validate signal
        if (signal.Direction == "NONE")
        {
            return (false, "No valid signal (direction is NONE)", null);
        }

        // Validate risk calculation
        if (!riskCalc.IsAllowed)
        {
            return (false, $"Trade not allowed: {string.Join(", ", riskCalc.Messages)}", null);
        }

        // Allocate capital
        try
        {
            await _accountService.AllocateCapitalAsync(riskCalc.InvestAmount);
        }
        catch (InvalidOperationException ex)
        {
            return (false, ex.Message, null);
        }

        // Create trade
        var trade = new PaperTrade
        {
            Symbol = signal.Symbol,
            Direction = signal.Direction,
            EntryPrice = signal.Entry,
            StopLoss = signal.StopLoss,
            TakeProfit = signal.TakeProfit,
            PositionSize = riskCalc.PositionSize,
            Quantity = (int)Math.Floor(riskCalc.PositionSize),
            InvestAmount = riskCalc.InvestAmount,
            Confidence = signal.Confidence,
            Reasons = signal.Reasons,
            Status = "OPEN",
            OpenedAt = _timeProvider.GetCurrentTime(),
            Notes = $"Risk: {riskCalc.RiskPercent}%, R/R: 1:{riskCalc.RiskRewardRatio:F2}"
        };

        _context.PaperTrades.Add(trade);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Opened {Direction} trade for {Symbol}: Entry={Entry}, SL={SL}, TP={TP}, Size={Size}, Invest={Invest}",
            trade.Direction, trade.Symbol, trade.EntryPrice, trade.StopLoss, trade.TakeProfit, 
            trade.PositionSize, trade.InvestAmount);

        return (true, "Trade opened successfully", trade);
    }

    /// <summary>
    /// Closes a paper trade manually or automatically.
    /// </summary>
    public async Task<(bool Success, string Message)> CloseTradeAsync(
        int tradeId, 
        decimal exitPrice, 
        string reason)
    {
        var trade = await _context.PaperTrades.FindAsync(tradeId);

        if (trade == null)
        {
            return (false, "Trade not found");
        }

        if (trade.Status != "OPEN")
        {
            return (false, $"Trade is not open (status: {trade.Status})");
        }

        // Calculate PnL
        var pnl = CalculatePnL(trade, exitPrice);

        // Update trade with final values before creating history
        trade.Status = reason;
        trade.ExitPrice = exitPrice;
        trade.PnL = pnl.Amount;
        trade.PnLPercent = pnl.Percent;
        trade.ClosedAt = _timeProvider.GetCurrentTime();

        // Release capital and update account balance
        await _accountService.ReleaseCapitalAsync(trade.InvestAmount, pnl.Amount);

        // Create trade history entry (copies all data from trade)
        await CreateTradeHistoryAsync(trade);

        // Delete the closed trade from PaperTrades table
        _context.PaperTrades.Remove(trade);
        await _context.SaveChangesAsync();

        _logger.LogInformation(
            "Closed and archived trade #{Id} for {Symbol}: Exit={Exit}, PnL={PnL} ({Percent}%), Reason={Reason}",
            tradeId, trade.Symbol, exitPrice, pnl.Amount, pnl.Percent, reason);

        return (true, $"Trade closed: PnL {pnl.Amount:F2} ({pnl.Percent:F2}%)");
    }

    /// <summary>
    /// Checks all open trades for stop loss or take profit conditions.
    /// </summary>
    public async Task<List<(PaperTrade Trade, string Reason)>> CheckOpenTradesAsync()
    {
        var openTrades = await GetOpenTradesAsync();
        var closedTrades = new List<(PaperTrade, string)>();

        foreach (var trade in openTrades)
        {
            try
            {
                // Get current price
                var candles = await _marketDataProvider.GetCandlesAsync(trade.Symbol, 1, "1d");
                if (candles.Count == 0)
                {
                    _logger.LogWarning("No candles available for {Symbol}, skipping check", trade.Symbol);
                    continue;
                }

                var currentPrice = candles.Last().Close;

                // Check stop loss and take profit
                var shouldClose = ShouldCloseTrade(trade, currentPrice, out var reason);

                if (shouldClose && reason != null)
                {
                    var result = await CloseTradeAsync(trade.Id, currentPrice, reason);
                    if (result.Success)
                    {
                        closedTrades.Add((trade, reason));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking trade #{Id} for {Symbol}", trade.Id, trade.Symbol);
            }
        }

        return closedTrades;
    }

    /// <summary>
    /// Gets all open trades.
    /// </summary>
    public async Task<List<PaperTrade>> GetOpenTradesAsync()
    {
        // Use AsNoTracking since we're only reading and calculating runtime PnL
        // PnL should never be saved to DB for open trades, only for closed ones
        var trades = await _context.PaperTrades
            .AsNoTracking()
            .Where(t => t.Status == "OPEN")
            .OrderBy(t => t.OpenedAt)
            .ToListAsync();

        // Calculate current unrealized PnL for each open trade (runtime only, not persisted)
        foreach (var trade in trades)
        {
            try
            {
                var candles = await _marketDataProvider.GetCandlesAsync(trade.Symbol, 1, "1d");
                if (candles.Count > 0)
                {
                    var currentPrice = candles.Last().Close;
                    var pnl = CalculatePnL(trade, currentPrice);
                    
                    // Set runtime PnL values (not persisted to DB for open trades)
                    trade.PnL = pnl.Amount;
                    trade.PnLPercent = pnl.Percent;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to calculate PnL for trade {Symbol}", trade.Symbol);
                // Set to 0 if calculation fails
                trade.PnL = 0;
                trade.PnLPercent = 0;
            }
        }

        return trades;
    }

    /// <summary>
    /// Gets trade history (closed trades).
    /// </summary>
    public async Task<List<TradeHistory>> GetTradeHistoryAsync(int limit = 50)
    {
        return await _context.TradeHistory
            .OrderByDescending(t => t.ClosedAt)
            .Take(limit)
            .ToListAsync();
    }

    /// <summary>
    /// Calculates unrealized PnL for an open trade.
    /// </summary>
    public async Task<decimal> CalculateUnrealizedPnLAsync(PaperTrade trade)
    {
        if (trade.Status != "OPEN")
        {
            return trade.PnL ?? 0;
        }

        try
        {
            var candles = await _marketDataProvider.GetCandlesAsync(trade.Symbol, 1, "1d");
            if (candles.Count == 0)
            {
                return 0;
            }

            var currentPrice = candles.Last().Close;
            var pnl = CalculatePnL(trade, currentPrice);
            return pnl.Amount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating unrealized PnL for trade #{Id}", trade.Id);
            return 0;
        }
    }

    /// <summary>
    /// Calculates PnL for a trade at a given exit price.
    /// </summary>
    private (decimal Amount, decimal Percent) CalculatePnL(PaperTrade trade, decimal exitPrice)
    {
        decimal pnlAmount;

        if (trade.Direction == "LONG")
        {
            pnlAmount = (exitPrice - trade.EntryPrice) * trade.PositionSize;
        }
        else // SHORT
        {
            pnlAmount = (trade.EntryPrice - exitPrice) * trade.PositionSize;
        }

        var pnlPercent = (pnlAmount / trade.InvestAmount) * 100m;

        return (pnlAmount, pnlPercent);
    }

    /// <summary>
    /// Checks if a trade should be closed based on current price.
    /// </summary>
    private bool ShouldCloseTrade(PaperTrade trade, decimal currentPrice, out string? reason)
    {
        reason = null;

        if (trade.Direction == "LONG")
        {
            // Check stop loss
            if (currentPrice <= trade.StopLoss)
            {
                reason = "CLOSED_SL";
                return true;
            }

            // Check take profit
            if (currentPrice >= trade.TakeProfit)
            {
                reason = "CLOSED_TP";
                return true;
            }
        }
        else // SHORT
        {
            // Check stop loss
            if (currentPrice >= trade.StopLoss)
            {
                reason = "CLOSED_SL";
                return true;
            }

            // Check take profit
            if (currentPrice <= trade.TakeProfit)
            {
                reason = "CLOSED_TP";
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Creates a trade history entry from a closed trade.
    /// </summary>
    private async Task CreateTradeHistoryAsync(PaperTrade trade)
    {
        var duration = trade.ClosedAt.HasValue 
            ? (int)(trade.ClosedAt.Value - trade.OpenedAt).TotalMinutes 
            : 0;

        var history = new TradeHistory
        {
            Symbol = trade.Symbol,
            Direction = trade.Direction,
            EntryPrice = trade.EntryPrice,
            ExitPrice = trade.ExitPrice ?? 0,
            StopLoss = trade.StopLoss,
            TakeProfit = trade.TakeProfit,
            Quantity = trade.Quantity,
            PositionSize = trade.PositionSize,
            InvestAmount = trade.InvestAmount,
            PnL = trade.PnL ?? 0,
            PnLPercent = trade.PnLPercent ?? 0,
            IsWinner = (trade.PnL ?? 0) > 0,
            ExitReason = trade.Status,
            Confidence = trade.Confidence,
            DurationMinutes = duration,
            OpenedAt = trade.OpenedAt,
            ClosedAt = trade.ClosedAt ?? _timeProvider.GetCurrentTime()
        };

        _context.TradeHistory.Add(history);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created trade history entry for trade #{Id}", trade.Id);
    }

    /// <summary>
    /// Calculates trade statistics from closed trades.
    /// </summary>
    public async Task<TradeStatistics> GetTradeStatisticsAsync()
    {
        var closedTrades = await _context.TradeHistory
            .OrderBy(t => t.ClosedAt)
            .ToListAsync();

        var stats = new TradeStatistics
        {
            TotalTrades = closedTrades.Count
        };

        if (closedTrades.Count == 0)
        {
            return stats;
        }

        // Basic counts
        stats.WinningTrades = closedTrades.Count(t => t.PnL > 0);
        stats.LosingTrades = closedTrades.Count(t => t.PnL < 0);
        stats.WinRate = stats.TotalTrades > 0 
            ? (decimal)stats.WinningTrades / stats.TotalTrades * 100 
            : 0;

        // PnL calculations
        stats.TotalPnL = closedTrades.Sum(t => t.PnL);
        
        var winningTrades = closedTrades.Where(t => t.PnL > 0).ToList();
        var losingTrades = closedTrades.Where(t => t.PnL < 0).ToList();
        
        stats.AverageWin = winningTrades.Count > 0 
            ? winningTrades.Average(t => t.PnL) 
            : 0;
        stats.AverageLoss = losingTrades.Count > 0 
            ? losingTrades.Average(t => t.PnL) 
            : 0;

        // Profit factor
        var totalWins = winningTrades.Sum(t => t.PnL);
        var totalLosses = Math.Abs(losingTrades.Sum(t => t.PnL));
        stats.ProfitFactor = totalLosses > 0 ? totalWins / totalLosses : 0;

        // Average R (Risk/Reward)
        var rValues = new List<decimal>();
        foreach (var trade in closedTrades)
        {
            var risk = Math.Abs(trade.EntryPrice - trade.StopLoss) * trade.Quantity;
            if (risk > 0)
            {
                rValues.Add(trade.PnL / risk);
            }
        }
        stats.AverageR = rValues.Count > 0 ? rValues.Average() : 0;

        // Max Drawdown
        decimal peak = 0;
        decimal maxDrawdown = 0;
        decimal runningPnL = 0;

        foreach (var trade in closedTrades)
        {
            runningPnL += trade.PnL;
            if (runningPnL > peak)
            {
                peak = runningPnL;
            }
            var drawdown = peak - runningPnL;
            if (drawdown > maxDrawdown)
            {
                maxDrawdown = drawdown;
            }
        }
        stats.MaxDrawdown = maxDrawdown;

        // Best and worst trades
        stats.BestTrade = closedTrades.OrderByDescending(t => t.PnL).FirstOrDefault();
        stats.WorstTrade = closedTrades.OrderBy(t => t.PnL).FirstOrDefault();

        return stats;
    }
}
