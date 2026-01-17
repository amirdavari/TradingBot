using API.Data;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers;

/// <summary>
/// API Controller for managing the user's watchlist.
/// MVP: Single watchlist, persistent in SQLite.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class WatchlistController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly SymbolValidationService _validationService;
    private readonly IMarketTimeProvider _timeProvider;
    private readonly ILogger<WatchlistController> _logger;

    public WatchlistController(
        ApplicationDbContext context, 
        SymbolValidationService validationService,
        IMarketTimeProvider timeProvider,
        ILogger<WatchlistController> logger)
    {
        _context = context;
        _validationService = validationService;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <summary>
    /// Gets all symbols in the watchlist.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<WatchlistSymbol>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<WatchlistSymbol>>> GetWatchlist()
    {
        var symbols = await _context.WatchlistSymbols
            .OrderBy(s => s.Symbol)
            .ToListAsync();

        return symbols;
    }

    /// <summary>
    /// Adds a symbol to the watchlist.
    /// </summary>
    /// <param name="request">Symbol to add (will be uppercased)</param>
    [HttpPost]
    [ProducesResponseType(typeof(WatchlistSymbol), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<WatchlistSymbol>> AddSymbol([FromBody] AddSymbolRequest request)
    {
        // Validate symbol
        if (string.IsNullOrWhiteSpace(request.Symbol))
            return BadRequest("Symbol is required");

        var symbol = request.Symbol.Trim().ToUpper();

        // MVP validation: A-Z only, 1-6 characters
        if (!_validationService.IsValidFormat(symbol))
            return BadRequest("Symbol must be 1-6 uppercase letters (A-Z)");

        // Check if already exists
        var exists = await _context.WatchlistSymbols
            .AnyAsync(s => s.Symbol == symbol);

        if (exists)
            return Conflict($"Symbol '{symbol}' is already in watchlist");

        // Validate that the symbol exists in market data
        var isValidSymbol = await _validationService.IsValidSymbolAsync(symbol);
        if (!isValidSymbol)
        {
            _logger.LogWarning("Attempted to add invalid symbol: {Symbol}", symbol);
            return BadRequest($"Symbol '{symbol}' is not valid or has no available data. Please check the symbol and try again.");
        }

        // Add to watchlist
        var watchlistSymbol = new WatchlistSymbol
        {
            Symbol = symbol,
            CreatedAt = _timeProvider.GetCurrentTime()
        };

        _context.WatchlistSymbols.Add(watchlistSymbol);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Added symbol {Symbol} to watchlist", symbol);

        return CreatedAtAction(nameof(GetWatchlist), new { id = watchlistSymbol.Id }, watchlistSymbol);
    }

    /// <summary>
    /// Removes a symbol from the watchlist.
    /// </summary>
    /// <param name="symbol">Symbol to remove</param>
    [HttpDelete("{symbol}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteSymbol(string symbol)
    {
        var upperSymbol = symbol.ToUpper();

        var watchlistSymbol = await _context.WatchlistSymbols
            .FirstOrDefaultAsync(s => s.Symbol == upperSymbol);

        if (watchlistSymbol == null)
            return NotFound($"Symbol '{upperSymbol}' not found in watchlist");

        _context.WatchlistSymbols.Remove(watchlistSymbol);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Removed symbol {Symbol} from watchlist", upperSymbol);

        return NoContent();
    }
}

/// <summary>
/// Request model for adding a symbol to the watchlist.
/// </summary>
public record AddSymbolRequest
{
    public required string Symbol { get; init; }
}
