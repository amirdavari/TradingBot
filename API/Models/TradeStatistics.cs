namespace API.Models;

/// <summary>
/// Statistics for paper trading performance.
/// </summary>
public class TradeStatistics
{
    public int TotalTrades { get; set; }
    public int WinningTrades { get; set; }
    public int LosingTrades { get; set; }
    public decimal WinRate { get; set; } // Percentage (0-100)
    public decimal TotalPnL { get; set; }
    public decimal AverageWin { get; set; }
    public decimal AverageLoss { get; set; }
    public decimal AverageR { get; set; } // Average Risk/Reward ratio
    public decimal MaxDrawdown { get; set; } // Maximum drawdown
    public decimal ProfitFactor { get; set; } // Total wins / Total losses
    public PaperTrade? BestTrade { get; set; }
    public PaperTrade? WorstTrade { get; set; }
}
