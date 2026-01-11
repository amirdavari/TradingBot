using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// API Controller for generating trading signals.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SignalsController : ControllerBase
{
    private readonly SignalService _signalService;
    private readonly ILogger<SignalsController> _logger;
    private static readonly string[] ValidPeriods = ["1d", "5d", "1mo"];

    public SignalsController(SignalService signalService, ILogger<SignalsController> logger)
    {
        _signalService = signalService;
        _logger = logger;
    }

    /// <summary>
    /// Gets a trading signal for a specific stock symbol.
    /// </summary>
    /// <param name="symbol">Stock symbol (e.g., "AAPL")</param>
    /// <param name="timeframe">Timeframe in minutes (default: 5). Supported: 1, 5, 15</param>
    /// <param name="period">Time period (default: "1d"). Examples: "1d", "5d", "1mo"</param>
    [HttpGet("{symbol}")]
    [ProducesResponseType(typeof(TradeSignal), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TradeSignal>> GetSignal(
        string symbol,
        [FromQuery] int timeframe = 5,
        [FromQuery] string period = "1d")
    {
        if (string.IsNullOrWhiteSpace(symbol))
            return BadRequest("Symbol is required");

        if (timeframe is not (1 or 5 or 15))
            return BadRequest("Timeframe must be 1, 5, or 15 minutes");

        if (!ValidPeriods.Contains(period, StringComparer.OrdinalIgnoreCase))
            return BadRequest($"Period must be one of: {string.Join(", ", ValidPeriods)}");

        try
        {
            var signal = await _signalService.GenerateSignalAsync(symbol, timeframe, period);

            if (signal.Entry == 0 && signal.Direction == "NONE")
            {
                _logger.LogWarning("No signal generated for {Symbol} - insufficient data", symbol);
                return NotFound($"No data available for symbol '{symbol}' or insufficient data to generate signal");
            }

            return signal;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching data for {Symbol}", symbol);
            return NotFound($"Unable to fetch data for symbol '{symbol}'. Symbol may not exist or data source unavailable.");
        }
    }
}
