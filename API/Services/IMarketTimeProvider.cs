namespace API.Services;

/// <summary>
/// Provides abstracted time source that works in both Live and Replay modes.
/// This ensures business logic doesn't depend on DateTime.Now/UtcNow directly.
/// </summary>
public interface IMarketTimeProvider
{
    /// <summary>
    /// Gets the current market time.
    /// In Live mode: returns DateTime.UtcNow
    /// In Replay mode: returns ReplayState.CurrentTime
    /// </summary>
    DateTime GetCurrentTime();

    /// <summary>
    /// Gets the current market mode.
    /// </summary>
    Models.MarketMode GetMode();
}
