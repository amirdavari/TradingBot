using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// API Controller for scanning stocks and identifying daytrading candidates.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ScannerController : ControllerBase
{
    private readonly ScannerService _scannerService;
    private readonly ILogger<ScannerController> _logger;

    public ScannerController(ScannerService scannerService, ILogger<ScannerController> logger)
    {
        _scannerService = scannerService;
        _logger = logger;
    }

    /// <summary>
    /// Scans multiple stocks and returns daytrading candidates sorted by score.
    /// </summary>
    /// <param name="symbols">Optional comma-separated list of symbols to scan. If not provided, scans default watchlist.</param>
    /// <param name="timeframe">Timeframe in minutes (default: 5). Supported: 1, 5, 15</param>
    [HttpGet]
    [ProducesResponseType(typeof(List<ScanResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<ScanResult>>> ScanStocks(
        [FromQuery] string? symbols = null,
        [FromQuery] int timeframe = 5)
    {
        if (timeframe is not (1 or 5 or 15))
            return BadRequest("Timeframe must be 1, 5, or 15 minutes");

        string[]? symbolArray = null;
        if (!string.IsNullOrWhiteSpace(symbols))
        {
            symbolArray = symbols.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (symbolArray.Length == 0)
                return BadRequest("Invalid symbols parameter");
        }

        _logger.LogInformation("Scanner request with timeframe {Timeframe}", timeframe);

        var results = await _scannerService.ScanStocksAsync(symbolArray, timeframe);

        _logger.LogInformation("Scanner found {Count} candidates", results.Count);

        return results;
    }
}
