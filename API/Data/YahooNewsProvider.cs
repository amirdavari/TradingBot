using API.Models;
using System.Text.Json;

namespace API.Data;

/// <summary>
/// Provides news data from Yahoo Finance.
/// </summary>
public class YahooNewsProvider : INewsProvider
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<YahooNewsProvider> _logger;

    public YahooNewsProvider(HttpClient httpClient, ILogger<YahooNewsProvider> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<NewsItem>> GetNewsAsync(string symbol, int count = 10)
    {
        try
        {
            var url = $"https://query1.finance.yahoo.com/v1/finance/search?q={symbol}&quotesCount=0&newsCount={count}";

            _logger.LogInformation("Fetching news for {Symbol}", symbol);

            var response = await _httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch news from Yahoo Finance. Status: {StatusCode}",
                    response.StatusCode);
                return new List<NewsItem>();
            }

            var content = await response.Content.ReadAsStringAsync();
            var newsItems = ParseYahooNewsResponse(content, symbol);

            // Sort by publication date (newest first)
            newsItems = newsItems.OrderByDescending(n => n.PublishedAt).ToList();

            _logger.LogInformation("Successfully loaded {Count} news items for {Symbol}", newsItems.Count, symbol);

            return newsItems;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching news for {Symbol}", symbol);
            return new List<NewsItem>();
        }
    }

    private List<NewsItem> ParseYahooNewsResponse(string jsonContent, string symbol)
    {
        var newsItems = new List<NewsItem>();

        try
        {
            using var document = JsonDocument.Parse(jsonContent);
            var root = document.RootElement;

            // Navigate to news array
            if (!root.TryGetProperty("news", out var newsArray))
            {
                return newsItems;
            }

            foreach (var newsElement in newsArray.EnumerateArray())
            {
                try
                {
                    var title = GetStringProperty(newsElement, "title");
                    var summary = GetStringProperty(newsElement, "summary");

                    // Try to get the link/source
                    var source = GetStringProperty(newsElement, "link");
                    if (string.IsNullOrWhiteSpace(source))
                    {
                        source = GetStringProperty(newsElement, "publisher");
                    }

                    // Get timestamp (Unix timestamp in seconds)
                    var timestamp = GetLongProperty(newsElement, "providerPublishTime");
                    var publishedAt = timestamp > 0
                        ? DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime
                        : DateTime.UtcNow;

                    // Skip if we don't have at least a title
                    if (string.IsNullOrWhiteSpace(title))
                    {
                        continue;
                    }

                    var newsItem = new NewsItem
                    {
                        Title = title,
                        Summary = summary ?? string.Empty,
                        PublishedAt = publishedAt,
                        Source = source ?? "Yahoo Finance",
                        Sentiment = DetermineSentiment(title, summary) // Simple sentiment analysis
                    };

                    newsItems.Add(newsItem);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error parsing individual news item");
                    continue;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing Yahoo News response");
        }

        return newsItems;
    }

    /// <summary>
    /// Simple keyword-based sentiment analysis (MVP implementation).
    /// </summary>
    private string DetermineSentiment(string title, string? summary)
    {
        var text = $"{title} {summary}".ToLowerInvariant();

        // Positive keywords
        var positiveKeywords = new[]
        {
            "up", "surge", "gain", "profit", "growth", "rise", "high",
            "strong", "beat", "exceed", "record", "success", "bullish",
            "positive", "boost", "improve", "rally", "soar"
        };

        // Negative keywords
        var negativeKeywords = new[]
        {
            "down", "fall", "drop", "loss", "decline", "weak", "low",
            "miss", "cut", "bearish", "negative", "concern", "risk",
            "crash", "plunge", "tumble", "warning", "struggle"
        };

        int positiveScore = positiveKeywords.Count(keyword => text.Contains(keyword));
        int negativeScore = negativeKeywords.Count(keyword => text.Contains(keyword));

        if (positiveScore > negativeScore)
            return "positive";
        else if (negativeScore > positiveScore)
            return "negative";
        else
            return "neutral";
    }

    private string GetStringProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property) &&
            property.ValueKind == JsonValueKind.String)
        {
            return property.GetString() ?? string.Empty;
        }
        return string.Empty;
    }

    private long GetLongProperty(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property) &&
            property.ValueKind == JsonValueKind.Number)
        {
            return property.GetInt64();
        }
        return 0;
    }
}
