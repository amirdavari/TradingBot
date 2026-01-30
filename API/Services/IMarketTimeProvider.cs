namespace API.Services;

/// <summary>
/// Provides abstracted time source and market mode state.
/// </summary>
public interface IMarketTimeProvider
{
    /// <summary>
    /// Gets the current market time (always UTC now).
    /// </summary>
    DateTime GetCurrentTime();

    /// <summary>
    /// Gets the current market mode (Live or Mock).
    /// </summary>
    Models.MarketMode GetMode();
}
