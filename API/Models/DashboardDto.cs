namespace API.Models;

/// <summary>
/// Dashboard data response containing all information needed for the dashboard view.
/// </summary>
public class DashboardResponse
{
    /// <summary>
    /// Chart candle data.
    /// </summary>
    public List<Candle> Candles { get; set; } = new();

    /// <summary>
    /// Trade signal with entry, stop-loss, and take-profit levels.
    /// </summary>
    public TradeSignal Signal { get; set; } = new();

    /// <summary>
    /// Recent news for the symbol.
    /// </summary>
    public List<NewsItem> News { get; set; } = new();
}

/// <summary>
/// Request DTO for creating a paper trade.
/// </summary>
public class CreatePaperTradeRequest
{
    /// <summary>
    /// Stock symbol (e.g., "AAPL").
    /// </summary>
    public string Symbol { get; set; } = string.Empty;

    /// <summary>
    /// Trade direction: "LONG" or "SHORT".
    /// </summary>
    public string Direction { get; set; } = string.Empty;

    /// <summary>
    /// Investment amount in EUR/USD.
    /// </summary>
    public decimal InvestAmount { get; set; }

    /// <summary>
    /// Timeframe used for the signal (1, 5, or 15 minutes).
    /// </summary>
    public int Timeframe { get; set; } = 5;
}

/// <summary>
/// Response DTO for a created paper trade.
/// </summary>
public class PaperTradeResponse
{
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public decimal EntryPrice { get; set; }
    public decimal StopLoss { get; set; }
    public decimal TakeProfit { get; set; }
    public int Quantity { get; set; }
    public decimal InvestAmount { get; set; }
    public int Confidence { get; set; }
    public List<string> Reasons { get; set; } = new();
    public string Status { get; set; } = string.Empty;
    public decimal? PnL { get; set; }
    public decimal? PnLPercent { get; set; }
    public DateTime OpenedAt { get; set; }
}
