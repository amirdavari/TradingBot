using Microsoft.AspNetCore.Mvc;
using API.Models;
using API.Data;
using API.Services;

namespace API.Controllers;

/// <summary>
/// Dashboard controller providing all data needed for the dashboard view.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IMarketDataProvider _marketDataProvider;
    private readonly INewsProvider _newsProvider;
    private readonly SignalService _signalService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IMarketDataProvider marketDataProvider,
        INewsProvider newsProvider,
        SignalService signalService,
        ILogger<DashboardController> logger)
    {
        _marketDataProvider = marketDataProvider;
        _newsProvider = newsProvider;
        _signalService = signalService;
        _logger = logger;
    }

    /// <summary>
    /// Get all dashboard data for a specific symbol.
    /// Includes chart data, trade signal, and news.
    /// </summary>
    /// <param name="symbol">Stock symbol (e.g., AAPL)</param>
    /// <param name="timeframe">Timeframe in minutes (1, 5, or 15). Default is 5.</param>
    /// <param name="period">Time period for candles (e.g., 1d, 5d, 1mo). Default is 1d.</param>
    /// <returns>Dashboard data including candles, signal, and news</returns>
    [HttpGet("{symbol}")]
    public async Task<ActionResult<DashboardResponse>> GetDashboardData(
        string symbol,
        [FromQuery] int timeframe = 5,
        [FromQuery] string period = "1d")
    {
        try
        {
            _logger.LogInformation("Fetching dashboard data for {Symbol} (timeframe: {Timeframe}, period: {Period})", 
                symbol, timeframe, period);

            // Fetch data in parallel
            var candlesTask = _marketDataProvider.GetCandlesAsync(symbol, timeframe, period);
            var signalTask = _signalService.GenerateSignalAsync(symbol, timeframe);
            var newsTask = _newsProvider.GetNewsAsync(symbol, 5);

            await Task.WhenAll(candlesTask, signalTask, newsTask);

            var response = new DashboardResponse
            {
                Candles = await candlesTask,
                Signal = await signalTask,
                News = await newsTask
            };

            _logger.LogInformation("Successfully fetched dashboard data for {Symbol}: {CandleCount} candles, signal: {Direction}", 
                symbol, response.Candles.Count, response.Signal.Direction);

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dashboard data for {Symbol}", symbol);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}
