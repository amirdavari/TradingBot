using API.Data;
using API.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// API Controller for retrieving candlestick (OHLCV) data.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class CandlesController : ControllerBase
{
    private readonly IMarketDataProvider _marketDataProvider;
    private readonly ILogger<CandlesController> _logger;
    private static readonly string[] ValidPeriods = ["1d", "2d", "5d", "1mo"];

    public CandlesController(IMarketDataProvider marketDataProvider, ILogger<CandlesController> logger)
    {
        _marketDataProvider = marketDataProvider;
        _logger = logger;
    }

    /// <summary>
    /// Gets candlestick data for a specific stock symbol.
    /// </summary>
    /// <param name="symbol">Stock symbol (e.g., "AAPL")</param>
    /// <param name="timeframe">Timeframe in minutes (default: 5). Supported: 1, 5, 15</param>
    /// <param name="period">Time period (default: "1d"). Examples: "1d", "5d", "1mo"</param>
    [HttpGet("{symbol}")]
    [ProducesResponseType(typeof(List<Candle>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<List<Candle>>> GetCandles(
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
            var candles = await _marketDataProvider.GetCandlesAsync(symbol, timeframe, period);

            if (candles == null || candles.Count == 0)
            {
                _logger.LogWarning("No candle data found for {Symbol}", symbol);
                return NotFound($"No data available for symbol '{symbol}'");
            }

            return candles;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Error fetching candle data for {Symbol}", symbol);
            return NotFound($"Unable to fetch data for symbol '{symbol}'. Symbol may not exist or data source unavailable.");
        }
    }
}
