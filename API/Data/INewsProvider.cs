using API.Models;

namespace API.Data;

/// <summary>
/// Interface for providing news data related to stocks.
/// </summary>
public interface INewsProvider
{
    /// <summary>
    /// Gets news items for a specific stock symbol.
    /// </summary>
    /// <param name="symbol">Stock symbol (e.g., "AAPL")</param>
    /// <param name="count">Maximum number of news items to retrieve (default: 10)</param>
    /// <returns>List of news items, sorted by publication date (newest first)</returns>
    Task<List<NewsItem>> GetNewsAsync(string symbol, int count = 10);
}
