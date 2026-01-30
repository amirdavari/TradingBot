using API.Hubs;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace API.Controllers;

/// <summary>
/// Controller for market mode management (Live/Mock).
/// Route kept as "replay" for backwards compatibility.
/// </summary>
[ApiController]
[Route("api/replay")]
public class MarketModeController : ControllerBase
{
    private readonly MarketTimeProvider _timeProvider;
    private readonly IHubContext<TradingHub> _hubContext;

    public MarketModeController(
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
    public ActionResult<MarketModeResponse> GetState()
    {
        var state = _timeProvider.GetMarketState();
        return Ok(new MarketModeResponse
        {
            Mode = state.Mode == MarketMode.Live ? "LIVE" : "MOCK",
            CurrentTime = state.CurrentTime
        });
    }

    /// <summary>
    /// Sets the market mode (Live or Mock).
    /// </summary>
    [HttpPost("mode")]
    public async Task<IActionResult> SetMode([FromBody] SetModeRequest request)
    {
        var mode = request.Mode.ToUpper() switch
        {
            "LIVE" => MarketMode.Live,
            "MOCK" => MarketMode.Replay,
            "REPLAY" => MarketMode.Replay, // Keep for backwards compatibility
            _ => (MarketMode?)null
        };

        if (mode == null)
        {
            return BadRequest(new { error = "Invalid mode. Must be 'LIVE' or 'MOCK'" });
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

    private MarketModeResponse GetStateResponse()
    {
        var state = _timeProvider.GetMarketState();
        return new MarketModeResponse
        {
            Mode = state.Mode == MarketMode.Live ? "LIVE" : "MOCK",
            CurrentTime = state.CurrentTime
        };
    }
}
