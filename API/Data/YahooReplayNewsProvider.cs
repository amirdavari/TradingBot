using API.Models;
using API.Services;

namespace API.Data;

/// <summary>
/// News provider for Replay mode.
/// Wraps YahooNewsProvider and filters news based on replay time.
/// </summary>
public class YahooReplayNewsProvider : INewsProvider
{
    private readonly YahooNewsProvider _yahooProvider;
    private readonly IMarketTimeProvider _timeProvider;
    private readonly ILogger<YahooReplayNewsProvider> _logger;

    public YahooReplayNewsProvider(
        YahooNewsProvider yahooProvider,
        IMarketTimeProvider timeProvider,
        ILogger<YahooReplayNewsProvider> logger)
    {
        _yahooProvider = yahooProvider;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <summary>
    /// Gets news items filtered by current replay time.
    /// Returns only news where PublishedAt <= ReplayState.CurrentTime.
    /// </summary>
    public async Task<List<NewsItem>> GetNewsAsync(string symbol, int count = 10)
    {
        _logger.LogInformation("YahooReplayNewsProvider.GetNewsAsync called for {Symbol}, count={Count}", symbol, count);
        
        // Load all news from Yahoo Finance
        var allNews = await _yahooProvider.GetNewsAsync(symbol, count * 2); // Request more to compensate for filtering

        // Get current replay time
        var currentTime = _timeProvider.GetCurrentTime();

        // Filter news strictly: only return news published by current replay time
        var filteredNews = allNews
            .Where(n => n.PublishedAt <= currentTime)
            .Take(count)
            .ToList();

        var totalNews = allNews.Count;
        var filteredCount = filteredNews.Count;

        _logger.LogInformation(
            "Replay mode: Filtered {FilteredCount} of {TotalCount} news items for {Symbol} (Replay time: {CurrentTime})",
            filteredCount, totalNews, symbol, currentTime);

        return filteredNews;
    }
}
