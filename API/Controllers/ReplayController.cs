using API.Hubs;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace API.Controllers;

/// <summary>
/// Controller for market mode management (Live/Mock).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ReplayController : ControllerBase
{
    private readonly MarketTimeProvider _timeProvider;
    private readonly IHubContext<TradingHub> _hubContext;

    public ReplayController(
        MarketTimeProvider timeProvider,
        IHubContext<TradingHub> hubContext)
    {
        _timeProvider = timeProvider;
        _hubContext = hubContext;
    }

    /// <summary>
    /// Gets the current market mode state.
    /// </summary>
    [HttpGet("state")]
    public ActionResult<ReplayStateResponse> GetState()
    {
        var state = _timeProvider.GetReplayState();
        return Ok(new ReplayStateResponse
        {
            Mode = state.Mode == MarketMode.Live ? "LIVE" : "REPLAY",
            CurrentTime = state.CurrentTime,
            ReplayStartTime = state.ReplayStartTime,
            Speed = state.Speed,
            IsRunning = state.IsRunning
        });
    }

    /// <summary>
    /// Sets the market mode (Live or Mock/Replay).
    /// </summary>
    [HttpPost("mode")]
    public async Task<IActionResult> SetMode([FromBody] SetModeRequest request)
    {
        var mode = request.Mode.ToUpper() switch
        {
            "LIVE" => MarketMode.Live,
            "REPLAY" => MarketMode.Replay,
            _ => (MarketMode?)null
        };

        if (mode == null)
        {
            return BadRequest(new { error = "Invalid mode. Must be 'LIVE' or 'REPLAY'" });
        }

        _timeProvider.SetMode(mode.Value);

        // Clear rate limit cache for Yahoo provider
        Data.YahooFinanceMarketDataProvider.ClearRateLimitCache();

        // Broadcast mode change via SignalR
        var stateResponse = GetStateResponse();
        await _hubContext.Clients.All.SendAsync(TradingHubMethods.ReceiveReplayState, stateResponse);

        // Trigger chart refresh
        await _hubContext.Clients.All.SendAsync(TradingHubMethods.ReceiveChartRefresh, new { symbols = Array.Empty<string>() });

        return Ok(new { message = $"Mode set to {request.Mode}", state = stateResponse });
    }

    private ReplayStateResponse GetStateResponse()
    {
        var state = _timeProvider.GetReplayState();
        return new ReplayStateResponse
        {
            Mode = state.Mode == MarketMode.Live ? "LIVE" : "REPLAY",
            CurrentTime = state.CurrentTime,
            ReplayStartTime = state.ReplayStartTime,
            Speed = state.Speed,
            IsRunning = state.IsRunning
        };
    }
}
