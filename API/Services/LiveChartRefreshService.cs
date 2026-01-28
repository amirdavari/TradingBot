using API.Data;
using API.Hubs;
using API.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

/// <summary>
/// Background service that broadcasts chart refresh and scanner results in Live mode.
/// In Live mode, pushes ReceiveChartRefresh every second and ReceiveScanResults every 10 seconds.
/// This replaces frontend polling with server-push for better efficiency.
/// </summary>
public class LiveChartRefreshService : BackgroundService
{
    private readonly IMarketTimeProvider _timeProvider;
    private readonly IHubContext<TradingHub> _hubContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<LiveChartRefreshService> _logger;
    // Yahoo Finance has ~15-20 min delay, so frequent polling is wasteful
    // 10 seconds is a good balance for UI responsiveness vs API load
    private readonly TimeSpan _refreshInterval = TimeSpan.FromSeconds(5);
    private readonly TimeSpan _scannerInterval = TimeSpan.FromSeconds(5);
    private DateTime _lastScannerBroadcast = DateTime.MinValue;

    public LiveChartRefreshService(
        IMarketTimeProvider timeProvider,
        IHubContext<TradingHub> hubContext,
        IServiceProvider serviceProvider,
        ILogger<LiveChartRefreshService> logger)
    {
        _timeProvider = timeProvider;
        _hubContext = hubContext;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Broadcasts scanner results to all connected clients.
    /// Scans the user's watchlist symbols, not just default symbols.
    /// </summary>
    private async Task BroadcastScannerResultsAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var scannerService = scope.ServiceProvider.GetRequiredService<ScannerService>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Get watchlist symbols from database
            var watchlistSymbols = await dbContext.WatchlistSymbols
                .Select(w => w.Symbol)
                .ToArrayAsync(stoppingToken);

            if (watchlistSymbols.Length == 0)
            {
                _logger.LogDebug("No watchlist symbols to scan");
                return;
            }

            _logger.LogInformation("Scanning {Count} watchlist symbols via SignalR", watchlistSymbols.Length);

            // Scan watchlist symbols (not default symbols)
            var results = await scannerService.ScanStocksAsync(watchlistSymbols, timeframe: 5);

            await _hubContext.Clients.All.SendAsync(
                TradingHubMethods.ReceiveScanResults, 
                results, 
                stoppingToken);
            _logger.LogDebug("Broadcasted {Count} scanner results via SignalR (Live mode)", results.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting scanner results in Live mode");
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LiveChartRefreshService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_refreshInterval, stoppingToken);

                // Only broadcast in Live mode
                if (_timeProvider.GetMode() == MarketMode.Live)
                {
                    _logger.LogDebug("Broadcasting chart refresh in Live mode via SignalR");
                    await _hubContext.Clients.All.SendAsync(
                        TradingHubMethods.ReceiveChartRefresh,
                        new { symbols = Array.Empty<string>() },
                        stoppingToken);

                    // Broadcast scanner results periodically (every 10 seconds)
                    var now = DateTime.UtcNow;
                    if ((now - _lastScannerBroadcast) >= _scannerInterval)
                    {
                        _lastScannerBroadcast = now;
                        await BroadcastScannerResultsAsync(stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LiveChartRefreshService");
            }
        }

        _logger.LogInformation("LiveChartRefreshService stopped");
    }
}
