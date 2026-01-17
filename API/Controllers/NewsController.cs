using API.Data;
using API.Models;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// API Controller for retrieving stock-related news.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class NewsController : ControllerBase
{
    private readonly INewsProvider _newsProvider;
    private readonly ILogger<NewsController> _logger;

    public NewsController(INewsProvider newsProvider, ILogger<NewsController> logger)
    {
        _newsProvider = newsProvider;
        _logger = logger;
    }

    /// <summary>
    /// Gets news items for a specific stock symbol.
    /// </summary>
    /// <param name="symbol">Stock symbol (e.g., AAPL)</param>
    /// <param name="count">Maximum number of news items to retrieve (default: 10, max: 20)</param>
    [HttpGet("{symbol}")]
    [ProducesResponseType(typeof(List<NewsItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<NewsItem>>> GetNews(
        string symbol,
        [FromQuery] int count = 10)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            return BadRequest("Symbol is required");

        if (count < 1 || count > 20)
            return BadRequest("Count must be between 1 and 20");

        _logger.LogInformation("Getting news for symbol: {Symbol}", symbol);

        var news = await _newsProvider.GetNewsAsync(symbol, count);

        return Ok(news);
    }
}
