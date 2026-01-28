using API.Hubs;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace API.Controllers;

/// <summary>
/// Controller for managing market simulation scenarios.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ScenarioController : ControllerBase
{
    private readonly IScenarioService _scenarioService;
    private readonly ISimulationSettingsService _simulationSettingsService;
    private readonly IHubContext<TradingHub> _hubContext;

    public ScenarioController(
        IScenarioService scenarioService,
        ISimulationSettingsService simulationSettingsService,
        IHubContext<TradingHub> hubContext)
    {
        _scenarioService = scenarioService;
        _simulationSettingsService = simulationSettingsService;
        _hubContext = hubContext;
    }

    [HttpGet("presets")]
    public ActionResult<List<ScenarioPresetDto>> GetPresets()
    {
        var presets = _scenarioService.GetPresets();
        return Ok(presets);
    }

    [HttpGet("current")]
    public async Task<ActionResult<ScenarioStateDto>> GetCurrentState()
    {
        var state = await _scenarioService.GetStateAsync();
        return Ok(state);
    }

    [HttpPost("apply/{presetName}")]
    public async Task<ActionResult<ScenarioStateDto>> ApplyPreset(string presetName)
    {
        try
        {
            await _scenarioService.ApplyPresetAsync(presetName);
            var state = await _scenarioService.GetStateAsync();
            await _hubContext.Clients.All.SendAsync(TradingHubMethods.ReceiveScenarioChange, state);
            return Ok(state);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("custom")]
    public async Task<ActionResult<ScenarioStateDto>> ApplyCustom([FromBody] ScenarioConfig config)
    {
        if (config == null)
        {
            return BadRequest(new { error = "Configuration is required" });
        }

        await _scenarioService.ApplyCustomAsync(config);
        var state = await _scenarioService.GetStateAsync();
        await _hubContext.Clients.All.SendAsync(TradingHubMethods.ReceiveScenarioChange, state);
        return Ok(state);
    }

    [HttpPost("reset")]
    public async Task<ActionResult<ScenarioStateDto>> Reset()
    {
        await _scenarioService.ResetAsync();
        var state = await _scenarioService.GetStateAsync();
        await _hubContext.Clients.All.SendAsync(TradingHubMethods.ReceiveScenarioChange, state);
        return Ok(state);
    }

    [HttpPost("enabled/{enabled}")]
    public async Task<ActionResult<ScenarioStateDto>> SetEnabled(bool enabled)
    {
        await _scenarioService.SetEnabledAsync(enabled);
        var state = await _scenarioService.GetStateAsync();
        await _hubContext.Clients.All.SendAsync(TradingHubMethods.ReceiveScenarioChange, state);
        return Ok(state);
    }

    /// <summary>
    /// Redistributes scenarios randomly to all symbols.
    /// </summary>
    [HttpPost("redistribute")]
    public async Task<ActionResult<ScenarioStateDto>> RedistributeScenarios()
    {
        _scenarioService.RedistributeScenarios();
        var state = await _scenarioService.GetStateAsync();
        await _hubContext.Clients.All.SendAsync(TradingHubMethods.ReceiveScenarioChange, state);
        await _hubContext.Clients.All.SendAsync(TradingHubMethods.ReceiveChartRefresh, new { symbols = Array.Empty<string>() });
        return Ok(state);
    }

    /// <summary>
    /// Gets symbol-to-scenario assignments.
    /// </summary>
    [HttpGet("symbol-assignments")]
    public ActionResult<List<SymbolScenarioAssignment>> GetSymbolAssignments()
    {
        var assignments = _scenarioService.GetSymbolAssignments();
        return Ok(assignments);
    }

    /// <summary>
    /// Gets current simulation settings.
    /// </summary>
    [HttpGet("settings")]
    public async Task<ActionResult<SimulationSettings>> GetSimulationSettings()
    {
        var settings = await _simulationSettingsService.GetSettingsAsync();
        return Ok(settings);
    }

    /// <summary>
    /// Updates simulation settings.
    /// </summary>
    [HttpPut("settings")]
    public async Task<ActionResult<SimulationSettings>> UpdateSimulationSettings([FromBody] SimulationSettings settings)
    {
        if (settings == null)
        {
            return BadRequest(new { error = "Settings are required" });
        }

        var updated = await _simulationSettingsService.UpdateSettingsAsync(settings);
        await _hubContext.Clients.All.SendAsync(TradingHubMethods.ReceiveChartRefresh, new { symbols = Array.Empty<string>() });
        return Ok(updated);
    }

    /// <summary>
    /// Resets simulation settings to defaults.
    /// </summary>
    [HttpPost("settings/reset")]
    public async Task<ActionResult<SimulationSettings>> ResetSimulationSettings()
    {
        var defaults = await _simulationSettingsService.ResetToDefaultsAsync();
        await _hubContext.Clients.All.SendAsync(TradingHubMethods.ReceiveChartRefresh, new { symbols = Array.Empty<string>() });
        return Ok(defaults);
    }
}
