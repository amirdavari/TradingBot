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
    /// <param name="symbols">List of symbols to scan. Can be provided as comma-separated or multiple query parameters.</param>
    /// <param name="timeframe">Timeframe in minutes (default: 1). Supported: 1, 5, 15</param>
    [HttpGet]
    [ProducesResponseType(typeof(List<ScanResult>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<ScanResult>>> ScanStocks(
        [FromQuery] string[]? symbols = null,
        [FromQuery] int timeframe = 1)
    {
        if (timeframe is not (1 or 5 or 15))
            return BadRequest("Timeframe must be 1, 5, or 15 minutes");

        _logger.LogInformation("Scanner request with {Count} symbols and timeframe {Timeframe}", 
            symbols?.Length ?? 0, timeframe);

        if (symbols != null && symbols.Length > 0)
        {
            _logger.LogInformation("Scanning symbols: {Symbols}", string.Join(", ", symbols));
        }

        var results = await _scannerService.ScanStocksAsync(symbols, timeframe);

        _logger.LogInformation("Scanner found {Count} candidates", results.Count);

        return results;
    }
}
