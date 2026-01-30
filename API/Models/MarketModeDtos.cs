namespace API.Models;

/// <summary>
/// DTO for setting market mode.
/// </summary>
public class SetModeRequest
{
    public string Mode { get; set; } = "LIVE"; // "LIVE" or "MOCK"
}

/// <summary>
/// DTO for market mode state response.
/// </summary>
public class MarketModeResponse
{
    public string Mode { get; set; } = "LIVE";
    public DateTime CurrentTime { get; set; }
}
