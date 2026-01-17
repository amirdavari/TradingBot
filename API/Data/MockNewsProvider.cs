using API.Models;

namespace API.Data;

/// <summary>
/// Mock news provider for testing and replay mode.
/// Provides sample historical news data.
/// </summary>
public class MockNewsProvider : INewsProvider
{
    private readonly ILogger<MockNewsProvider> _logger;

    public MockNewsProvider(ILogger<MockNewsProvider> logger)
    {
        _logger = logger;
    }

    public Task<List<NewsItem>> GetNewsAsync(string symbol, int count = 10)
    {
        _logger.LogInformation("MockNewsProvider: Generating mock news for {Symbol}", symbol);

        var newsItems = GenerateMockNews(symbol, count);
        
        return Task.FromResult(newsItems);
    }

    private List<NewsItem> GenerateMockNews(string symbol, int count)
    {
        var baseDate = DateTime.UtcNow.AddDays(-7); // Start from 7 days ago
        var newsItems = new List<NewsItem>();

        var newsTemplates = new[]
        {
            new { Title = "{0} Reports Strong Q4 Earnings", Sentiment = "positive" },
            new { Title = "{0} Announces New Product Launch", Sentiment = "positive" },
            new { Title = "{0} Shares Rise on Market Optimism", Sentiment = "positive" },
            new { Title = "Analysts Upgrade {0} Price Target", Sentiment = "positive" },
            new { Title = "{0} CEO Discusses Growth Strategy", Sentiment = "neutral" },
            new { Title = "{0} Trading Volume Increases", Sentiment = "neutral" },
            new { Title = "Market Watch: {0} in Focus", Sentiment = "neutral" },
            new { Title = "{0} Faces Regulatory Scrutiny", Sentiment = "negative" },
            new { Title = "{0} Reports Supply Chain Issues", Sentiment = "negative" },
            new { Title = "Concerns Grow Over {0} Market Position", Sentiment = "negative" }
        };

        for (int i = 0; i < Math.Min(count, newsTemplates.Length); i++)
        {
            var template = newsTemplates[i % newsTemplates.Length];
            var hoursAgo = i * 12; // Spread news over time (every 12 hours)
            
            newsItems.Add(new NewsItem
            {
                Title = string.Format(template.Title, symbol),
                Summary = $"Latest developments regarding {symbol} show interesting market movements and investor sentiment.",
                PublishedAt = baseDate.AddHours(-hoursAgo),
                Source = "Market News",
                Sentiment = template.Sentiment
            });
        }

        return newsItems.OrderByDescending(n => n.PublishedAt).ToList();
    }
}
