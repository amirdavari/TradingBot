using API.Models;
using API.Services;

namespace API.Data;

/// <summary>
/// News provider for Replay mode using mock data.
/// Filters news based on replay time to ensure only past news is shown.
/// </summary>
public class ReplayMockNewsProvider : INewsProvider
{
    private readonly MockNewsProvider _mockProvider;
    private readonly IMarketTimeProvider _timeProvider;
    private readonly ILogger<ReplayMockNewsProvider> _logger;

    public ReplayMockNewsProvider(
        MockNewsProvider mockProvider,
        IMarketTimeProvider timeProvider,
        ILogger<ReplayMockNewsProvider> logger)
    {
        _mockProvider = mockProvider;
        _timeProvider = timeProvider;
        _logger = logger;
    }

    /// <summary>
    /// Gets news items filtered by current replay time.
    /// Returns only news where PublishedAt <= ReplayState.CurrentTime.
    /// </summary>
    public async Task<List<NewsItem>> GetNewsAsync(string symbol, int count = 10)
    {
        _logger.LogInformation("ReplayMockNewsProvider.GetNewsAsync called for {Symbol}, count={Count}", symbol, count);
        
        // Get all mock news (request more to compensate for filtering)
        var allNews = await _mockProvider.GetNewsAsync(symbol, count * 3);

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
            "Replay mode: Filtered {FilteredCount} of {TotalCount} mock news items for {Symbol} (Replay time: {CurrentTime})",
            filteredCount, totalNews, symbol, currentTime);

        return filteredNews;
    }
}
