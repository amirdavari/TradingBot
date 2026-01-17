using API.Models;
using API.Services;
using System.Collections.Concurrent;
using System.Globalization;
using System.Text.Json;

namespace API.Data;

/// <summary>
/// Provides market data from Yahoo Finance.
/// </summary>
public class YahooFinanceMarketDataProvider : IMarketDataProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<YahooFinanceMarketDataProvider> _logger;
    private readonly IMarketTimeProvider _timeProvider;

    private static readonly SemaphoreSlim _rateLimiter = new(1, 1);
    private static readonly ConcurrentDictionary<string, DateTime> _lastRequests = new();
    private static readonly TimeSpan MinRequestInterval = TimeSpan.FromMilliseconds(500);

    public YahooFinanceMarketDataProvider(
        HttpClient httpClient, 
        ILogger<YahooFinanceMarketDataProvider> logger,
        IMarketTimeProvider timeProvider)
    {
        _httpClient = httpClient;
        _logger = logger;
        _timeProvider = timeProvider;

        // Set User-Agent to avoid blocking
        if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36");
        }
    }

    /// <summary>
    /// Clears the rate limiting cache. Useful when switching between modes.
    /// </summary>
    public static void ClearRateLimitCache()
    {
        _lastRequests.Clear();
    }

    public async Task<List<Candle>> GetCandlesAsync(string symbol, int timeframe, string period = "1d")
    {
        // Rate limiting to avoid 429 errors (per symbol)
        // Use real time (UtcNow) for rate limiting, not replay time
        await _rateLimiter.WaitAsync();
        try
        {
            var now = DateTime.UtcNow;
            var lastRequestTime = _lastRequests.GetOrAdd(symbol, DateTime.MinValue);
            var timeSinceLastRequest = now - lastRequestTime;
            
            if (timeSinceLastRequest < MinRequestInterval)
            {
                var delay = MinRequestInterval - timeSinceLastRequest;
                _logger.LogDebug("Rate limiting: waiting {Delay}ms for {Symbol}", delay.TotalMilliseconds, symbol);
                await Task.Delay(delay);
            }
            
            _lastRequests[symbol] = DateTime.UtcNow;
        }
        finally
        {
            _rateLimiter.Release();
        }

        try
        {
            // Map timeframe to Yahoo Finance interval
            var interval = MapTimeframeToInterval(timeframe);

            // Get current time (respects replay mode)
            var currentTime = _timeProvider.GetCurrentTime();
            var mode = _timeProvider.GetMode();

            // Build Yahoo Finance query URL with absolute timestamps
            var url = BuildYahooFinanceUrl(symbol, interval, period, currentTime);

            _logger.LogInformation("Fetching candles for {Symbol} | Mode: {Mode} | Interval: {Interval} | Period: {Period} | Reference time: {Time}",
                symbol, mode, interval, period, currentTime);

            // Add timeout to prevent hanging (10 seconds)
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            var response = await _httpClient.GetAsync(url, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch data from Yahoo Finance. Status: {StatusCode}, URL: {Url}",
                    response.StatusCode, url);
                return new List<Candle>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var candles = ParseYahooFinanceResponse(content);

            // Sort chronologically (ascending by time)
            candles = candles.OrderBy(c => c.Time).ToList();

            _logger.LogInformation("Successfully loaded {Count} candles for {Symbol}", candles.Count, symbol);

            return candles;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching candles for {Symbol}", symbol);
            return new List<Candle>();
        }
    }

    private string MapTimeframeToInterval(int timeframe)
    {
        return timeframe switch
        {
            1 => "1m",
            5 => "5m",
            15 => "15m",
            _ => "5m" // Default to 5 minutes
        };
    }

    private string BuildYahooFinanceUrl(string symbol, string interval, string period, DateTime referenceTime)
    {
        // Yahoo Finance v8 Chart API
        var baseUrl = "https://query1.finance.yahoo.com/v8/finance/chart";
        
        // Convert period string to TimeSpan
        var timeSpan = ParsePeriodToTimeSpan(period);
        
        // Calculate absolute timestamps: from (referenceTime - period) to referenceTime
        var endTime = referenceTime;
        var startTime = referenceTime.Subtract(timeSpan);
        
        // Convert to Unix timestamps
        var period1 = new DateTimeOffset(startTime).ToUnixTimeSeconds();
        var period2 = new DateTimeOffset(endTime).ToUnixTimeSeconds();
        
        // Use absolute timestamps instead of range parameter
        var url = $"{baseUrl}/{symbol}?interval={interval}&period1={period1}&period2={period2}";
        
        _logger.LogInformation("Yahoo Finance URL for {Symbol}: {StartTime:yyyy-MM-dd HH:mm} to {EndTime:yyyy-MM-dd HH:mm} (Period: {Period}, {Days} days)",
            symbol, startTime, endTime, period, timeSpan.TotalDays);
        
        return url;
    }

    private TimeSpan ParsePeriodToTimeSpan(string period)
    {
        // Parse period strings like "1d", "5d", "1mo", "60d"
        if (period.EndsWith("d"))
        {
            var days = int.Parse(period.TrimEnd('d'));
            return TimeSpan.FromDays(days);
        }
        else if (period.EndsWith("mo"))
        {
            var months = int.Parse(period.TrimEnd("mo".ToCharArray()));
            return TimeSpan.FromDays(months * 30); // Approximate
        }
        else if (period.EndsWith("y"))
        {
            var years = int.Parse(period.TrimEnd('y'));
            return TimeSpan.FromDays(years * 365); // Approximate
        }
        else
        {
            _logger.LogWarning("Unknown period format: {Period}, defaulting to 1 day", period);
            return TimeSpan.FromDays(1);
        }
    }

    private List<Candle> ParseYahooFinanceResponse(string jsonContent)
    {
        var candles = new List<Candle>();

        try
        {
            using var document = JsonDocument.Parse(jsonContent);
            var root = document.RootElement;

            // Navigate to chart.result[0]
            if (!root.TryGetProperty("chart", out var chart) ||
                !chart.TryGetProperty("result", out var results) ||
                results.GetArrayLength() == 0)
            {
                return candles;
            }

            var result = results[0];

            // Get timestamp array
            if (!result.TryGetProperty("timestamp", out var timestamps))
            {
                return candles;
            }

            // Get indicators.quote[0]
            if (!result.TryGetProperty("indicators", out var indicators) ||
                !indicators.TryGetProperty("quote", out var quotes) ||
                quotes.GetArrayLength() == 0)
            {
                return candles;
            }

            var quote = quotes[0];

            // Extract OHLCV arrays
            var opens = GetDecimalArray(quote, "open");
            var highs = GetDecimalArray(quote, "high");
            var lows = GetDecimalArray(quote, "low");
            var closes = GetDecimalArray(quote, "close");
            var volumes = GetLongArray(quote, "volume");

            // Create candle objects
            int count = timestamps.GetArrayLength();
            for (int i = 0; i < count; i++)
            {
                // Skip if any value is null
                if (opens[i] == null || highs[i] == null || lows[i] == null ||
                    closes[i] == null || volumes[i] == null)
                {
                    continue;
                }

                var timestamp = timestamps[i].GetInt64();
                var time = DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;

                candles.Add(new Candle
                {
                    Time = time,
                    Open = opens[i]!.Value,
                    High = highs[i]!.Value,
                    Low = lows[i]!.Value,
                    Close = closes[i]!.Value,
                    Volume = volumes[i]!.Value
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Yahoo Finance response");
        }

        return candles;
    }

    private decimal?[] GetDecimalArray(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return Array.Empty<decimal?>();
        }

        var array = new decimal?[property.GetArrayLength()];
        int index = 0;

        foreach (var item in property.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.Number)
            {
                array[index] = Math.Round(item.GetDecimal(), 2);
            }
            else
            {
                array[index] = null;
            }
            index++;
        }

        return array;
    }

    private long?[] GetLongArray(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return Array.Empty<long?>();
        }

        var array = new long?[property.GetArrayLength()];
        int index = 0;

        foreach (var item in property.EnumerateArray())
        {
            if (item.ValueKind == JsonValueKind.Number)
            {
                array[index] = item.GetInt64();
            }
            else
            {
                array[index] = null;
            }
            index++;
        }

        return array;
    }
}
