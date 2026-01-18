namespace API.Models;

/// <summary>
/// Request model for creating a new paper trade.
/// </summary>
public class CreateTradeRequest
{
    public string Symbol { get; set; } = string.Empty;
    public string Direction { get; set; } = string.Empty;
    public decimal EntryPrice { get; set; }
    public decimal StopLoss { get; set; }
    public decimal TakeProfit { get; set; }
    public decimal PositionSize { get; set; }
    public decimal InvestAmount { get; set; }
    public int Confidence { get; set; }
    public List<string> Reasons { get; set; } = new();
    public decimal RiskPercent { get; set; }
}
