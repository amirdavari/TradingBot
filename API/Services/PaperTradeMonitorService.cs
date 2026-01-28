using API.Hubs;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;

namespace API.Services;

/// <summary>
/// Background service that monitors open paper trades and automatically closes them
/// when stop loss or take profit conditions are met.
/// Also periodically broadcasts PnL updates for open trades via SignalR.
/// </summary>
public class PaperTradeMonitorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<TradingHub> _hubContext;
    private readonly ILogger<PaperTradeMonitorService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(5); // Check every 5 seconds
    private readonly TimeSpan _pnlUpdateInterval = TimeSpan.FromSeconds(10); // PnL updates every 10 seconds
    private DateTime _lastPnlUpdate = DateTime.MinValue;

    public PaperTradeMonitorService(
        IServiceProvider serviceProvider,
        IHubContext<TradingHub> hubContext,
        ILogger<PaperTradeMonitorService> logger)
    {
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("PaperTradeMonitorService started");

        // Wait a bit before starting to ensure services are initialized
        await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckTradesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in PaperTradeMonitorService");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("PaperTradeMonitorService stopped");
    }

    private async Task CheckTradesAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var tradeService = scope.ServiceProvider.GetRequiredService<PaperTradeService>();
        var accountService = scope.ServiceProvider.GetRequiredService<AccountService>();

        try
        {
            var closedTrades = await tradeService.CheckOpenTradesAsync();

            if (closedTrades.Count > 0)
            {
                _logger.LogInformation(
                    "Auto-closed {Count} trades: {Trades}",
                    closedTrades.Count,
                    string.Join(", ", closedTrades.Select(t => $"{t.Trade.Symbol}({t.Reason})")));

                // Push each closed trade to all clients
                foreach (var (trade, reason) in closedTrades)
                {
                    await _hubContext.Clients.All.SendAsync(
                        TradingHubMethods.ReceiveTradeClosed,
                        new { Trade = trade, Reason = reason });
                }

                // Push updated account balance
                var account = await accountService.GetOrCreateAccountAsync();
                await _hubContext.Clients.All.SendAsync(
                    TradingHubMethods.ReceiveAccountUpdate,
                    account);
            }

            // Periodically broadcast PnL updates for open trades
            var now = DateTime.UtcNow;
            if ((now - _lastPnlUpdate) >= _pnlUpdateInterval)
            {
                _lastPnlUpdate = now;
                await BroadcastPnLUpdatesAsync(tradeService);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking trades");
        }
    }

    /// <summary>
    /// Broadcasts unrealized PnL updates for all open trades via SignalR.
    /// </summary>
    private async Task BroadcastPnLUpdatesAsync(PaperTradeService tradeService)
    {
        try
        {
            var openTrades = await tradeService.GetOpenTradesAsync();
            
            if (openTrades.Count == 0)
            {
                return;
            }

            _logger.LogDebug("Broadcasting PnL updates for {Count} open trades", openTrades.Count);

            // Calculate and broadcast updated PnL for each trade
            foreach (var trade in openTrades)
            {
                var unrealizedPnL = await tradeService.CalculateUnrealizedPnLAsync(trade);
                var pnlPercent = trade.InvestAmount > 0 
                    ? (unrealizedPnL / trade.InvestAmount) * 100 
                    : 0;

                // Create updated trade object with current PnL
                var updatedTrade = new PaperTrade
                {
                    Id = trade.Id,
                    Symbol = trade.Symbol,
                    Direction = trade.Direction,
                    EntryPrice = trade.EntryPrice,
                    StopLoss = trade.StopLoss,
                    TakeProfit = trade.TakeProfit,
                    PositionSize = trade.PositionSize,
                    Quantity = trade.Quantity,
                    InvestAmount = trade.InvestAmount,
                    Confidence = trade.Confidence,
                    Reasons = trade.Reasons,
                    Status = trade.Status,
                    OpenedAt = trade.OpenedAt,
                    Notes = trade.Notes,
                    // Set unrealized PnL
                    PnL = unrealizedPnL,
                    PnLPercent = pnlPercent
                };

                await _hubContext.Clients.All.SendAsync(
                    TradingHubMethods.ReceiveTradeUpdate,
                    updatedTrade);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting PnL updates");
        }
    }
}
