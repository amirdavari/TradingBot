using API.Services;
using Microsoft.Extensions.Hosting;

namespace API.Services;

/// <summary>
/// Background service that monitors open paper trades and automatically closes them
/// when stop loss or take profit conditions are met.
/// </summary>
public class PaperTradeMonitorService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PaperTradeMonitorService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(5); // Check every 5 seconds

    public PaperTradeMonitorService(
        IServiceProvider serviceProvider,
        ILogger<PaperTradeMonitorService> logger)
    {
        _serviceProvider = serviceProvider;
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

        try
        {
            var closedTrades = await tradeService.CheckOpenTradesAsync();

            if (closedTrades.Count > 0)
            {
                _logger.LogInformation(
                    "Auto-closed {Count} trades: {Trades}",
                    closedTrades.Count,
                    string.Join(", ", closedTrades.Select(t => $"{t.Trade.Symbol}({t.Reason})")));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking trades");
        }
    }
}
