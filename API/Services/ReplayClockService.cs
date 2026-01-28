using API.Data;
using API.Hubs;
using API.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

/// <summary>
/// Background service that controls the replay clock.
/// Advances CurrentTime based on Speed when replay is running.
/// This service ONLY manages time - no market data or business logic.
/// Pushes state updates via SignalR, including scanner results periodically.
/// </summary>
public class ReplayClockService : BackgroundService
{
    private readonly MarketTimeProvider _timeProvider;
    private readonly IHubContext<TradingHub> _hubContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ReplayClockService> _logger;
    private readonly TimeSpan _tickInterval = TimeSpan.FromSeconds(1);
    private readonly TimeSpan _scannerInterval = TimeSpan.FromSeconds(10);
    private DateTime _lastScannerBroadcast = DateTime.MinValue;

    public ReplayClockService(
        MarketTimeProvider timeProvider,
        IHubContext<TradingHub> hubContext,
        IServiceProvider serviceProvider,
        ILogger<ReplayClockService> logger)
    {
        _timeProvider = timeProvider;
        _hubContext = hubContext;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Starts the replay simulation.
    /// </summary>
    public async Task StartAsync()
    {
        _timeProvider.UpdateReplayState(state =>
        {
            if (state.Mode == MarketMode.Replay)
            {
                state.IsRunning = true;
                _logger.LogInformation("Replay started at {Time} with speed {Speed}x",
                    state.CurrentTime, state.Speed);
            }
            else
            {
                _logger.LogWarning("Cannot start replay in Live mode");
            }
        });
        await BroadcastStateAsync();
    }

    /// <summary>
    /// Pauses the replay simulation.
    /// </summary>
    public async Task PauseAsync()
    {
        _timeProvider.UpdateReplayState(state =>
        {
            state.IsRunning = false;
            _logger.LogInformation("Replay paused at {Time}", state.CurrentTime);
        });
        await BroadcastStateAsync();
    }

    /// <summary>
    /// Resets the replay to the start time.
    /// </summary>
    public async Task ResetAsync()
    {
        _timeProvider.UpdateReplayState(state =>
        {
            state.CurrentTime = state.ReplayStartTime;
            state.IsRunning = false;
            _logger.LogInformation("Replay reset to {Time}", state.ReplayStartTime);
        });
        await BroadcastStateAsync();
    }

    /// <summary>
    /// Sets the replay speed multiplier.
    /// </summary>
    /// <param name="speed">Speed multiplier (1.0 = real-time, 5.0 = 5x speed, etc.)</param>
    public async Task SetSpeedAsync(double speed)
    {
        if (speed <= 0)
        {
            _logger.LogWarning("Invalid speed {Speed}. Speed must be positive.", speed);
            return;
        }

        _timeProvider.UpdateReplayState(state =>
        {
            state.Speed = speed;
            _logger.LogInformation("Replay speed set to {Speed}x", speed);
        });
        await BroadcastStateAsync();
    }

    /// <summary>
    /// Sets the replay start time and current time.
    /// </summary>
    public async Task SetReplayStartTimeAsync(DateTime startTime)
    {
        _timeProvider.UpdateReplayState(state =>
        {
            state.ReplayStartTime = startTime;
            state.CurrentTime = startTime;
            state.IsRunning = false;
            _logger.LogInformation("Replay start time set to {Time}", startTime);
        });
        await BroadcastStateAsync();
    }

    /// <summary>
    /// Broadcasts the current replay state to all connected clients.
    /// </summary>
    private async Task BroadcastStateAsync()
    {
        var state = _timeProvider.GetReplayState();
        var response = new ReplayStateResponse
        {
            Mode = state.Mode.ToString().ToUpper(),
            CurrentTime = state.CurrentTime,
            ReplayStartTime = state.ReplayStartTime,
            Speed = state.Speed,
            IsRunning = state.IsRunning
        };

        _logger.LogInformation("Broadcasting replay state via SignalR: {Time}, IsRunning: {IsRunning}",
            response.CurrentTime, response.IsRunning);
        await _hubContext.Clients.All.SendAsync(TradingHubMethods.ReceiveReplayState, response);
    }

    /// <summary>
    /// Broadcasts scanner results to all connected clients.
    /// Scans the user's watchlist symbols, not just default symbols.
    /// </summary>
    private async Task BroadcastScannerResultsAsync()
    {
        try
        {
            using var scope = _serviceProvider.CreateScope();
            var scannerService = scope.ServiceProvider.GetRequiredService<ScannerService>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Get watchlist symbols from database
            var watchlistSymbols = await dbContext.WatchlistSymbols
                .Select(w => w.Symbol)
                .ToArrayAsync();

            if (watchlistSymbols.Length == 0)
            {
                _logger.LogDebug("No watchlist symbols to scan");
                return;
            }

            _logger.LogInformation("Scanning {Count} watchlist symbols via SignalR (Replay)", watchlistSymbols.Length);

            // Scan watchlist symbols (not default symbols)
            var results = await scannerService.ScanStocksAsync(watchlistSymbols, timeframe: 5);

            await _hubContext.Clients.All.SendAsync(TradingHubMethods.ReceiveScanResults, results);
            _logger.LogDebug("Broadcasted {Count} scanner results via SignalR", results.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error broadcasting scanner results");
        }
    }

    /// <summary>
    /// Broadcasts chart refresh notification to all connected clients.
    /// </summary>
    private async Task BroadcastChartRefreshAsync()
    {
        _logger.LogInformation("Broadcasting chart refresh via SignalR");
        await _hubContext.Clients.All.SendAsync(TradingHubMethods.ReceiveChartRefresh, new { symbols = Array.Empty<string>() });
    }

    /// <summary>
    /// Background task that advances the replay clock.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ReplayClockService started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_tickInterval, stoppingToken);

                var replayState = _timeProvider.GetReplayState();

                // Only advance time if in Replay mode and running
                if (replayState.Mode == MarketMode.Replay && replayState.IsRunning)
                {
                    _timeProvider.UpdateReplayState(state =>
                    {
                        // Advance time: CurrentTime += Tick * Speed
                        var timeAdvance = TimeSpan.FromSeconds(_tickInterval.TotalSeconds * state.Speed);
                        state.CurrentTime = state.CurrentTime.Add(timeAdvance);

                        _logger.LogDebug("Replay time advanced to {Time} (Speed: {Speed}x)",
                            state.CurrentTime, state.Speed);
                    });

                    // Push updated state to all clients
                    await BroadcastStateAsync();

                    // Broadcast chart refresh so clients know to fetch new data
                    await BroadcastChartRefreshAsync();

                    // Broadcast scanner results periodically (every 10 seconds)
                    var now = DateTime.UtcNow;
                    if ((now - _lastScannerBroadcast) >= _scannerInterval)
                    {
                        _lastScannerBroadcast = now;
                        await BroadcastScannerResultsAsync();
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
                _logger.LogError(ex, "Error in ReplayClockService");
            }
        }

        _logger.LogInformation("ReplayClockService stopped");
    }
}
