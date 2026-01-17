using API.Data;

namespace API.Services;

/// <summary>
/// Service for validating stock symbols.
/// MVP: Uses Yahoo Finance to check if symbol exists.
/// Note: Always uses real-time Yahoo Finance data, even in Replay mode,
/// to ensure symbols can be validated regardless of current replay time.
/// </summary>
public class SymbolValidationService
{
    private readonly YahooFinanceMarketDataProvider _yahooProvider;
    private readonly ILogger<SymbolValidationService> _logger;

    public SymbolValidationService(YahooFinanceMarketDataProvider yahooProvider, ILogger<SymbolValidationService> logger)
    {
        _yahooProvider = yahooProvider;
        _logger = logger;
    }

    /// <summary>
    /// Validates if a stock symbol exists and has valid data.
    /// Always uses real Yahoo Finance data, bypassing replay time filtering.
    /// </summary>
    /// <param name="symbol">The symbol to validate (e.g., AAPL, MSFT)</param>
    /// <returns>True if valid, false otherwise</returns>
    public async Task<bool> IsValidSymbolAsync(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            return false;

        try
        {
            _logger.LogInformation("Validating symbol: {Symbol} (using real-time data)", symbol);

            // Try to fetch recent candles (1 day, 5min interval)
            // Always uses YahooFinanceMarketDataProvider directly to avoid replay time filtering
            var candles = await _yahooProvider.GetCandlesAsync(symbol, 5, "1d");

            // Valid if we got at least some candles
            var isValid = candles.Count > 0;

            _logger.LogInformation("Symbol {Symbol} validation result: {IsValid} ({CandleCount} candles)", 
                symbol, isValid, candles.Count);

            return isValid;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Symbol validation failed for {Symbol}: {Message}", symbol, ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Validates symbol format (1-6 uppercase letters).
    /// </summary>
    public bool IsValidFormat(string symbol)
    {
        if (string.IsNullOrWhiteSpace(symbol))
            return false;

        return System.Text.RegularExpressions.Regex.IsMatch(symbol, @"^[A-Z0-9](?:[A-Z0-9]{0,5}|[A-Z0-9]{0,4}[.-][A-Z0-9]{1,4})$");
    }
}
