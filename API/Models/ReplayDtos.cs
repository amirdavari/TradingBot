namespace API.Models;

/// <summary>
/// DTO for setting replay speed.
/// </summary>
public class SetSpeedRequest
{
    public double Speed { get; set; }
}

/// <summary>
/// DTO for setting replay start time.
/// </summary>
public class SetReplayTimeRequest
{
    public DateTime StartTime { get; set; }
}

/// <summary>
/// DTO for setting market mode.
/// </summary>
public class SetModeRequest
{
    public string Mode { get; set; } = "LIVE"; // "LIVE" or "REPLAY"
}

/// <summary>
/// DTO for replay state response.
/// </summary>
public class ReplayStateResponse
{
    public string Mode { get; set; } = "LIVE";
    public DateTime CurrentTime { get; set; }
    public DateTime ReplayStartTime { get; set; }
    public double Speed { get; set; }
    public bool IsRunning { get; set; }
}
