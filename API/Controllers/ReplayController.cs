using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Controller for replay simulation control (Dev/Simulation only).
/// Provides endpoints to control replay mode and time advancement.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ReplayController : ControllerBase
{
    private readonly MarketTimeProvider _timeProvider;
    private readonly ReplayClockService _clockService;
    private readonly ILogger<ReplayController> _logger;

    public ReplayController(
        MarketTimeProvider timeProvider,
        ReplayClockService clockService,
        ILogger<ReplayController> logger)
    {
        _timeProvider = timeProvider;
        _clockService = clockService;
        _logger = logger;
    }

    /// <summary>
    /// Gets the current replay state.
    /// </summary>
    /// <returns>Current replay state including mode, time, speed, and running status</returns>
    [HttpGet("state")]
    public ActionResult<ReplayStateResponse> GetState()
    {
        var state = _timeProvider.GetReplayState();
        
        var response = new ReplayStateResponse
        {
            Mode = state.Mode == MarketMode.Live ? "LIVE" : "REPLAY",
            CurrentTime = state.CurrentTime,
            ReplayStartTime = state.ReplayStartTime,
            Speed = state.Speed,
            IsRunning = state.IsRunning
        };

        return Ok(response);
    }

    /// <summary>
    /// Starts the replay simulation.
    /// </summary>
    [HttpPost("start")]
    public IActionResult Start()
    {
        var mode = _timeProvider.GetMode();
        
        if (mode != MarketMode.Replay)
        {
            return BadRequest(new { error = "Cannot start replay in Live mode. Switch to Replay mode first." });
        }

        _clockService.Start();
        _logger.LogInformation("Replay started via API");
        
        return Ok(new { message = "Replay started", state = GetStateResponse() });
    }

    /// <summary>
    /// Pauses the replay simulation.
    /// </summary>
    [HttpPost("pause")]
    public IActionResult Pause()
    {
        _clockService.Pause();
        _logger.LogInformation("Replay paused via API");
        
        return Ok(new { message = "Replay paused", state = GetStateResponse() });
    }

    /// <summary>
    /// Resets the replay to the start time.
    /// </summary>
    [HttpPost("reset")]
    public IActionResult Reset()
    {
        _clockService.Reset();
        _logger.LogInformation("Replay reset via API");
        
        return Ok(new { message = "Replay reset", state = GetStateResponse() });
    }

    /// <summary>
    /// Sets the replay speed multiplier.
    /// </summary>
    /// <param name="request">Speed request containing the speed multiplier</param>
    [HttpPost("speed")]
    public IActionResult SetSpeed([FromBody] SetSpeedRequest request)
    {
        if (request.Speed <= 0)
        {
            return BadRequest(new { error = "Speed must be greater than 0" });
        }

        if (request.Speed > 100)
        {
            return BadRequest(new { error = "Speed cannot exceed 100x" });
        }

        _clockService.SetSpeed(request.Speed);
        _logger.LogInformation("Replay speed set to {Speed}x via API", request.Speed);
        
        return Ok(new { message = $"Speed set to {request.Speed}x", state = GetStateResponse() });
    }

    /// <summary>
    /// Sets the replay start time.
    /// </summary>
    /// <param name="request">Request containing the start time</param>
    [HttpPost("time")]
    public IActionResult SetReplayTime([FromBody] SetReplayTimeRequest request)
    {
        var mode = _timeProvider.GetMode();
        
        if (mode != MarketMode.Replay)
        {
            return BadRequest(new { error = "Cannot set replay time in Live mode" });
        }

        if (request.StartTime > DateTime.UtcNow)
        {
            return BadRequest(new { error = "Start time cannot be in the future" });
        }

        _clockService.SetReplayStartTime(request.StartTime);
        _logger.LogInformation("Replay start time set to {Time} via API", request.StartTime);
        
        return Ok(new { message = "Replay time set", state = GetStateResponse() });
    }

    /// <summary>
    /// Sets the market mode (Live or Replay).
    /// </summary>
    /// <param name="request">Request containing the mode</param>
    [HttpPost("mode")]
    public IActionResult SetMode([FromBody] SetModeRequest request)
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

        // Pause replay when switching modes
        if (_timeProvider.GetMode() == MarketMode.Replay)
        {
            _clockService.Pause();
        }

        _timeProvider.SetMode(mode.Value);
        _logger.LogInformation("Market mode set to {Mode} via API", request.Mode);
        
        return Ok(new { message = $"Mode set to {request.Mode}", state = GetStateResponse() });
    }

    /// <summary>
    /// Helper method to get current state as response DTO.
    /// </summary>
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
