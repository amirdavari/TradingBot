using API.Data;
using API.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

/// <summary>
/// Background service that broadcasts chart refresh and scanner results.
/// Pushes ReceiveChartRefresh every second and ReceiveScanResults every 10 seconds.
/// </summary>
public class LiveChartRefreshService : BackgroundService
{
    private readonly IHubContext<TradingHub> _hubContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _refreshInterval = TimeSpan.FromSeconds(1);
    private readonly TimeSpan _scannerInterval = TimeSpan.FromSeconds(10);
    private DateTime _lastScannerBroadcast = DateTime.MinValue;

    public LiveChartRefreshService(
        IHubContext<TradingHub> hubContext,
        IServiceProvider serviceProvider)
    {
        _hubContext = hubContext;
        _serviceProvider = serviceProvider;
    }

    private async Task BroadcastScannerResultsAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var scannerService = scope.ServiceProvider.GetRequiredService<ScannerService>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            var watchlistSymbols = await dbContext.WatchlistSymbols
                .Select(w => w.Symbol)
                .ToArrayAsync(stoppingToken);

            if (watchlistSymbols.Length == 0) return;

            // Get user-selected timeframe from settings (default: 5)
            var settings = await dbContext.RiskSettings.FirstOrDefaultAsync(stoppingToken);
            var timeframe = settings?.SelectedTimeframe ?? 5;
            
            // Validate timeframe (must be 1, 5, or 15)
            if (timeframe is not (1 or 5 or 15)) timeframe = 5;

            var results = await scannerService.ScanStocksAsync(watchlistSymbols, timeframe);

            await _hubContext.Clients.All.SendAsync(
                TradingHubMethods.ReceiveScanResults,
                results,
                stoppingToken);
        }
        catch { /* Ignore scanner errors */ }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_refreshInterval, stoppingToken);

                // Broadcast chart refresh
                await _hubContext.Clients.All.SendAsync(
                    TradingHubMethods.ReceiveChartRefresh,
                    new { symbols = Array.Empty<string>() },
                    stoppingToken);

                // Broadcast scanner results periodically
                var now = DateTime.UtcNow;
                if ((now - _lastScannerBroadcast) >= _scannerInterval)
                {
                    _lastScannerBroadcast = now;
                    await BroadcastScannerResultsAsync(stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch { /* Ignore errors */ }
        }
    }
}
