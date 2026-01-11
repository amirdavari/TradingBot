namespace API.Models;

/// <summary>
/// Represents a news item with sentiment analysis.
/// </summary>
public class NewsItem
{
    /// <summary>
    /// Title of the news article.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Brief summary or excerpt.
    /// </summary>
    public string Summary { get; set; } = string.Empty;

    /// <summary>
    /// Sentiment classification: "positive", "neutral", or "negative".
    /// </summary>
    public string Sentiment { get; set; } = "neutral";

    /// <summary>
    /// Publication timestamp.
    /// </summary>
    public DateTime PublishedAt { get; set; }

    /// <summary>
    /// Source or URL of the news article.
    /// </summary>
    public string Source { get; set; } = string.Empty;
}
