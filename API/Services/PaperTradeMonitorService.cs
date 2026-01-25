using API.Hubs;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;

namespace API.Services;

/// <summary>
/// Background service that monitors open paper trades and automatically closes them
/// when stop loss or take profit conditions are met.
/// Pushes trade close notifications via SignalR.
/// </summary>
public class PaperTradeMonitorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<TradingHub> _hubContext;
    private readonly ILogger<PaperTradeMonitorService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(5); // Check every 5 seconds

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
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking trades");
        }
    }
}
