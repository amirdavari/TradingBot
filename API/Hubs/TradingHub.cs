using API.Models;
using Microsoft.AspNetCore.SignalR;

namespace API.Hubs;

/// <summary>
/// SignalR Hub for real-time trading updates.
/// Pushes updates for replay state, trades, and account changes.
/// </summary>
public class TradingHub : Hub
{
    private readonly ILogger<TradingHub> _logger;

    public TradingHub(ILogger<TradingHub> logger)
    {
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        _logger.LogInformation("Client connected: {ConnectionId}", Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("Client disconnected: {ConnectionId}", Context.ConnectionId);
        await base.OnDisconnectedAsync(exception);
    }
}

/// <summary>
/// Static class defining the client method names for type safety.
/// Frontend must implement handlers for these methods.
/// </summary>
public static class TradingHubMethods
{
    /// <summary>
    /// Sent when replay state changes (time advance, start/pause/reset, mode change).
    /// Payload: ReplayStateResponse
    /// </summary>
    public const string ReceiveReplayState = "ReceiveReplayState";

    /// <summary>
    /// Sent when a trade is opened or updated.
    /// Payload: PaperTrade
    /// </summary>
    public const string ReceiveTradeUpdate = "ReceiveTradeUpdate";

    /// <summary>
    /// Sent when a trade is automatically closed (SL/TP hit).
    /// Payload: { Trade: PaperTrade, Reason: string }
    /// </summary>
    public const string ReceiveTradeClosed = "ReceiveTradeClosed";

    /// <summary>
    /// Sent when account balance changes.
    /// Payload: Account
    /// </summary>
    public const string ReceiveAccountUpdate = "ReceiveAccountUpdate";

    /// <summary>
    /// Sent when scanner results are updated during replay.
    /// Payload: List<ScanResult>
    /// </summary>
    public const string ReceiveScanResults = "ReceiveScanResults";

    /// <summary>
    /// Sent when chart data should be refreshed (time advanced significantly).
    /// Payload: { symbols: string[] } - affected symbols, empty for all
    /// </summary>
    public const string ReceiveChartRefresh = "ReceiveChartRefresh";

    /// <summary>
    /// Sent when scenario configuration changes.
    /// Payload: ScenarioStateDto
    /// </summary>
    public const string ReceiveScenarioChange = "ReceiveScenarioChange";
}
