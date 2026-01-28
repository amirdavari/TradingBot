using API.Models;

namespace API.Services;

/// <summary>
/// Engine for generating realistic market simulation data.
/// Implements three-layer architecture: Base Process + Regime + Pattern Overlays.
/// </summary>
public class MarketSimulationEngine
{
    private readonly ILogger<MarketSimulationEngine> _logger;
    private readonly ISimulationSettingsService _settingsService;

    // EWMA volatility state (per symbol)
    private readonly Dictionary<string, decimal> _ewmaVolatility = new();
    private const decimal EwmaLambda = 0.94m; // EWMA decay factor

    public MarketSimulationEngine(
        ILogger<MarketSimulationEngine> logger,
        ISimulationSettingsService settingsService)
    {
        _logger = logger;
        _settingsService = settingsService;
    }

    #region Main Generation Method

    /// <summary>
    /// Generates a sequence of candles based on scenario configuration.
    /// </summary>
    public List<Candle> GenerateCandles(
        ScenarioConfig config,
        decimal startPrice,
        DateTime startTime,
        int totalBars,
        int timeframeMinutes,
        DateTime currentTime)
    {
        // Get current simulation settings
        var settings = _settingsService.GetSettingsAsync().GetAwaiter().GetResult();

        var candles = new List<Candle>();
        var seed = config.Seed ?? (config.Symbol?.GetHashCode() ?? 0) ^ (int)(startTime.Ticks / TimeSpan.TicksPerDay);
        var random = new Random(seed);

        // Initialize state
        var currentPrice = startPrice;
        var currentVolatility = config.BaseVolatility;
        var regimeIndex = 0;
        var barInRegime = 0;
        var prevClose = startPrice;

        // Build regime schedule
        var regimeSchedule = BuildRegimeSchedule(config.Regimes, totalBars);

        for (int bar = 0; bar < totalBars; bar++)
        {
            var candleTime = startTime.AddMinutes(bar * timeframeMinutes);

            // Don't generate future candles
            if (candleTime > currentTime)
                break;

            // Get current regime and parameters
            var (regime, regimeParams) = GetRegimeAtBar(regimeSchedule, bar, config);

            // Check for pattern overlays at this bar
            var overlay = GetOverlayAtBar(config.Overlays, bar, random);

            // Generate return using base process + regime
            var dailyReturn = GenerateReturn(
                currentPrice,
                prevClose,
                currentVolatility,
                regimeParams,
                overlay,
                random,
                config,
                settings);

            // Apply EWMA volatility clustering
            currentVolatility = UpdateEwmaVolatility(config.Symbol ?? "DEFAULT", dailyReturn, config.BaseVolatility);

            // Calculate new close price
            var newClose = currentPrice * (1m + dailyReturn);

            // Generate OHLC from return
            var candle = GenerateOhlcFromReturn(
                candleTime,
                prevClose,
                newClose,
                currentVolatility,
                regimeParams,
                overlay,
                random,
                timeframeMinutes,
                bar == totalBars - 1 && candleTime == RoundDownToTimeframe(currentTime, timeframeMinutes),
                currentTime,
                config,
                settings);

            candles.Add(candle);

            // Update state for next bar
            prevClose = candle.Close;
            currentPrice = candle.Close;
        }

        return candles;
    }

    #endregion

    #region Base Process Generator

    /// <summary>
    /// Generates a return with volatility clustering, fat tails, and regime effects.
    /// </summary>
    private decimal GenerateReturn(
        decimal currentPrice,
        decimal prevClose,
        decimal currentVolatility,
        RegimeParameters regimeParams,
        PatternOverlayConfig? overlay,
        Random random,
        ScenarioConfig config,
        SimulationSettings settings)
    {
        // Base return from normal distribution (Box-Muller transform)
        var u1 = random.NextDouble();
        var u2 = random.NextDouble();
        var normalReturn = (decimal)(Math.Sqrt(-2.0 * Math.Log(Math.Max(0.0001, u1))) * Math.Cos(2.0 * Math.PI * u2));

        // Scale by volatility and regime - configurable scaling for smoother moves
        var scaledVolatility = currentVolatility * regimeParams.VolatilityMultiplier * settings.VolatilityScale;
        var baseReturn = normalReturn * scaledVolatility;

        // Add drift from regime (configurable scale)
        baseReturn += regimeParams.Drift * settings.DriftScale;

        // Apply mean reversion (configurable strength)
        if (regimeParams.MeanReversion > 0 && prevClose > 0)
        {
            var deviation = (currentPrice - prevClose) / prevClose;
            baseReturn -= deviation * regimeParams.MeanReversion * settings.MeanReversionStrength;
        }

        // Fat tail injection - configurable probability and size
        if (random.NextDouble() < (double)(regimeParams.FatTailProbability * settings.FatTailMultiplier))
        {
            var tailRange = settings.FatTailMaxSize - settings.FatTailMinSize;
            var tailMultiplier = settings.FatTailMinSize + (decimal)random.NextDouble() * tailRange;
            baseReturn *= tailMultiplier * (random.NextDouble() > 0.5 ? 1 : -1);
        }

        // Pattern overlay effects
        if (overlay != null)
        {
            baseReturn = ApplyPatternOverlay(baseReturn, overlay, random, currentPrice, settings);
        }

        // Clamp to configurable bounds
        return Math.Max(-settings.MaxReturnPerBar, Math.Min(settings.MaxReturnPerBar, baseReturn));
    }

    /// <summary>
    /// Applies pattern overlay effects to the base return.
    /// </summary>
    private decimal ApplyPatternOverlay(decimal baseReturn, PatternOverlayConfig overlay, Random random, decimal currentPrice, SimulationSettings settings)
    {
        var direction = overlay.Direction.ToUpperInvariant() == "UP" ? 1m : -1m;
        var noise = (decimal)(random.NextDouble() - 0.5) * 0.0005m; // Tiny noise
        var strength = settings.PatternOverlayStrength;

        return overlay.Type switch
        {
            PatternOverlayType.BREAKOUT => direction * 0.003m * strength + noise, // Gentle breakout move
            PatternOverlayType.PULLBACK => -direction * 0.001m * (overlay.DepthATR ?? 0.8m) * strength + noise,
            PatternOverlayType.GAP_AND_GO => direction * 0.004m * strength + noise, // Gap continuation
            PatternOverlayType.MEAN_REVERSION => -baseReturn * 0.3m * strength + noise, // Partial reversion
            PatternOverlayType.DOUBLE_TOP or PatternOverlayType.DOUBLE_BOTTOM => baseReturn * 0.5m * strength, // Reduced momentum
            _ => baseReturn + direction * 0.001m * strength
        };
    }

    /// <summary>
    /// Updates EWMA volatility estimate for volatility clustering.
    /// </summary>
    private decimal UpdateEwmaVolatility(string symbol, decimal latestReturn, decimal baseVol)
    {
        var absReturn = Math.Abs(latestReturn);

        if (!_ewmaVolatility.TryGetValue(symbol, out var prevVol))
        {
            prevVol = baseVol;
        }

        // EWMA: σ²_t = λ * σ²_(t-1) + (1-λ) * r²_t
        var newVol = EwmaLambda * prevVol + (1m - EwmaLambda) * absReturn;

        // Clamp to reasonable bounds
        newVol = Math.Max(baseVol * 0.3m, Math.Min(baseVol * 3m, newVol));

        _ewmaVolatility[symbol] = newVol;
        return newVol;
    }

    #endregion

    #region OHLC Generation

    /// <summary>
    /// Generates OHLC candle from close-to-close return.
    /// </summary>
    private Candle GenerateOhlcFromReturn(
        DateTime time,
        decimal prevClose,
        decimal newClose,
        decimal volatility,
        RegimeParameters regimeParams,
        PatternOverlayConfig? overlay,
        Random random,
        int timeframeMinutes,
        bool isLiveCandle,
        DateTime currentTime,
        ScenarioConfig config,
        SimulationSettings settings)
    {
        // Open: previous close + potential gap
        var open = prevClose;
        if (ShouldGenerateGap(time, random, config, regimeParams))
        {
            var gapSize = (decimal)(random.NextDouble() * (double)config.MaxGapPercent);
            var gapDirection = random.NextDouble() > 0.5 ? 1m : -1m;
            open = prevClose * (1m + gapDirection * gapSize);
        }

        // Close: from return calculation
        var close = newClose;

        // If live candle, interpolate based on time into candle
        if (isLiveCandle)
        {
            var secondsIntoCandle = (int)(currentTime - time).TotalSeconds;
            var totalSeconds = timeframeMinutes * 60;
            var progress = Math.Min(1.0, secondsIntoCandle / (double)totalSeconds);

            // Interpolate close based on progress
            close = open + (newClose - open) * (decimal)progress;

            // Configurable tick noise for live feel
            var tickNoise = volatility * settings.LiveTickNoise * (decimal)(random.NextDouble() - 0.5);
            close += tickNoise;
        }

        // High/Low: based on intrabar range proportional to volatility (configurable)
        var range = Math.Abs(open - close) * settings.HighLowRangeMultiplier + volatility * Math.Max(open, close) * (decimal)(0.1 + random.NextDouble() * 0.2);

        var high = Math.Max(open, close) + range * (decimal)random.NextDouble() * settings.HighLowRangeMultiplier;
        var low = Math.Min(open, close) - range * (decimal)random.NextDouble() * settings.HighLowRangeMultiplier;

        // Ensure consistency
        high = Math.Max(high, Math.Max(open, close));
        low = Math.Min(low, Math.Min(open, close));

        // Volume calculation
        var volume = CalculateVolume(time, timeframeMinutes, regimeParams, overlay, random, isLiveCandle, currentTime);

        return new Candle
        {
            Time = time,
            Open = Math.Round(open, 2),
            High = Math.Round(high, 2),
            Low = Math.Round(low, 2),
            Close = Math.Round(close, 2),
            Volume = volume
        };
    }

    /// <summary>
    /// Determines if a gap should occur (typically at session boundaries).
    /// </summary>
    private bool ShouldGenerateGap(DateTime time, Random random, ScenarioConfig config, RegimeParameters regimeParams)
    {
        // Higher probability at day boundaries (9:00 CET for DAX)
        var isSessionStart = time.Hour == 9 && time.Minute == 0;
        var baseProbability = isSessionStart ? config.GapProbability * 3m : config.GapProbability * 0.1m;

        return random.NextDouble() < (double)(baseProbability * regimeParams.GapProbabilityModifier);
    }

    /// <summary>
    /// Calculates volume with session effects and pattern boosts.
    /// </summary>
    private long CalculateVolume(
        DateTime time,
        int timeframe,
        RegimeParameters regimeParams,
        PatternOverlayConfig? overlay,
        Random random,
        bool isLiveCandle,
        DateTime currentTime)
    {
        // Base volume by timeframe
        var baseVolume = timeframe switch
        {
            1 => 50_000L,
            5 => 200_000L,
            15 => 500_000L,
            _ => 100_000L
        };

        // Session-based multiplier (DAX hours: 9:00-17:30 CET, roughly 8:00-16:30 UTC)
        var hour = time.Hour;
        double sessionMultiplier = hour switch
        {
            >= 8 and < 10 => 1.8 + random.NextDouble() * 0.4,  // Open
            >= 15 and < 17 => 1.6 + random.NextDouble() * 0.4, // Close
            >= 10 and < 15 => 0.8 + random.NextDouble() * 0.4, // Mid-day
            _ => 0.2 + random.NextDouble() * 0.3               // Outside hours
        };

        // Regime multiplier
        var regimeMultiplier = (double)regimeParams.VolumeMultiplier;

        // Pattern overlay boost
        var patternBoost = overlay != null ? (double)overlay.VolumeBoost : 1.0;

        var totalVolume = (long)(baseVolume * sessionMultiplier * regimeMultiplier * patternBoost);

        // For live candle, scale by progress
        if (isLiveCandle)
        {
            var secondsIntoCandle = (int)(currentTime - time).TotalSeconds;
            var totalSeconds = timeframe * 60;
            var progress = Math.Max(0.1, secondsIntoCandle / (double)totalSeconds);
            totalVolume = (long)(totalVolume * progress);
        }

        return totalVolume;
    }

    #endregion

    #region Regime State Machine

    /// <summary>
    /// Builds a bar-indexed schedule of regimes from config.
    /// </summary>
    private List<(int StartBar, int EndBar, MarketRegime Regime, RegimePhase Phase)> BuildRegimeSchedule(
        List<RegimePhase> phases,
        int totalBars)
    {
        var schedule = new List<(int, int, MarketRegime, RegimePhase)>();

        if (phases.Count == 0)
        {
            // Default: single RANGE regime
            schedule.Add((0, totalBars - 1, MarketRegime.RANGE, new RegimePhase { Type = MarketRegime.RANGE, Bars = totalBars }));
            return schedule;
        }

        var currentBar = 0;
        foreach (var phase in phases)
        {
            var endBar = Math.Min(currentBar + phase.Bars - 1, totalBars - 1);
            schedule.Add((currentBar, endBar, phase.Type, phase));
            currentBar = endBar + 1;

            if (currentBar >= totalBars) break;
        }

        // Fill remaining bars with last regime
        if (currentBar < totalBars && schedule.Count > 0)
        {
            var lastEntry = schedule[^1];
            var lastPhase = lastEntry.Item4;
            schedule.Add((currentBar, totalBars - 1, lastPhase.Type, lastPhase));
        }

        return schedule;
    }

    /// <summary>
    /// Gets regime and parameters for a specific bar.
    /// </summary>
    private (MarketRegime Regime, RegimeParameters Params) GetRegimeAtBar(
        List<(int StartBar, int EndBar, MarketRegime Regime, RegimePhase Phase)> schedule,
        int bar,
        ScenarioConfig config)
    {
        foreach (var (startBar, endBar, regime, phase) in schedule)
        {
            if (bar >= startBar && bar <= endBar)
            {
                var baseParams = RegimeParameters.GetDefaults(regime);

                // Apply phase overrides
                if (phase.Volatility.HasValue)
                    baseParams.VolatilityMultiplier = phase.Volatility.Value / config.BaseVolatility;
                if (phase.Drift.HasValue)
                    baseParams.Drift = phase.Drift.Value;
                baseParams.VolumeMultiplier *= phase.VolumeMultiplier;

                return (regime, baseParams);
            }
        }

        return (MarketRegime.RANGE, RegimeParameters.GetDefaults(MarketRegime.RANGE));
    }

    /// <summary>
    /// Gets active pattern overlay at a specific bar (with noise tolerance).
    /// </summary>
    private PatternOverlayConfig? GetOverlayAtBar(List<PatternOverlayConfig> overlays, int bar, Random random)
    {
        foreach (var overlay in overlays)
        {
            // Apply noise tolerance
            var noise = random.Next(-overlay.NoiseBars, overlay.NoiseBars + 1);
            var triggerBar = overlay.AtBar + noise;
            var endBar = overlay.ToBar.HasValue ? overlay.ToBar.Value + noise : triggerBar;

            if (bar >= triggerBar && bar <= endBar)
            {
                return overlay;
            }
        }

        return null;
    }

    #endregion

    #region Helpers

    private static DateTime RoundDownToTimeframe(DateTime time, int timeframeMinutes)
    {
        var totalMinutes = (long)time.TimeOfDay.TotalMinutes;
        var roundedMinutes = (totalMinutes / timeframeMinutes) * timeframeMinutes;
        return time.Date.AddMinutes(roundedMinutes);
    }

    /// <summary>
    /// Resets EWMA volatility state (call when scenario changes).
    /// </summary>
    public void ResetState()
    {
        _ewmaVolatility.Clear();
    }

    #endregion
}
