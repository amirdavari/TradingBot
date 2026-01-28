using API.Models;
using API.Services;

namespace API.Data;

/// <summary>
/// Mock implementation of market data provider that generates realistic real-time candles.
/// Uses MarketSimulationEngine for scenario-based generation with regimes and patterns.
/// Falls back to simple deterministic generation when scenarios are disabled.
/// </summary>
public class MockRealtimeCandleProvider : IMarketDataProvider
{
    private readonly IMarketTimeProvider _timeProvider;
    private readonly IScenarioService _scenarioService;
    private readonly MarketSimulationEngine _simulationEngine;
    private readonly ILogger<MockRealtimeCandleProvider> _logger;

    // Base prices for DAX stocks (deterministic starting point)
    // Prices in EUR, based on typical 2025/2026 levels
    private static readonly Dictionary<string, decimal> BasePrices = new(StringComparer.OrdinalIgnoreCase)
    {
        // DAX 40
        ["SAP.DE"] = 220.00m,
        ["SIE.DE"] = 185.00m,
        ["ALV.DE"] = 285.00m,
        ["BAS.DE"] = 45.00m,
        ["IFX.DE"] = 35.00m,
        ["BMW.DE"] = 95.00m,
        ["MBG.DE"] = 58.00m,
        ["VOW3.DE"] = 110.00m,
        ["DTE.DE"] = 28.00m,
        ["RWE.DE"] = 32.00m,
        ["EOAN.DE"] = 13.00m,
        ["MUV2.DE"] = 485.00m,
        ["CBK.DE"] = 17.00m,
        ["DBK.DE"] = 16.00m,
        ["ENR.DE"] = 28.00m,
        ["ADS.DE"] = 235.00m,
        ["BAYN.DE"] = 28.00m,
        ["HEI.DE"] = 115.00m,
        ["ZAL.DE"] = 32.00m,
        ["DB1.DE"] = 215.00m,
        ["RHM.DE"] = 580.00m,
        ["MTX.DE"] = 285.00m,
        ["AIR.DE"] = 155.00m,
        ["SRT3.DE"] = 245.00m,
        ["SY1.DE"] = 115.00m,
        ["HEN3.DE"] = 82.00m,
        ["1COV.DE"] = 55.00m,
        ["P911.DE"] = 68.00m,
        ["VNA.DE"] = 28.00m,
        ["FRE.DE"] = 35.00m,
        ["HFG.DE"] = 12.00m,
        ["DHER.DE"] = 28.00m,
        ["BEI.DE"] = 135.00m,
        ["HNR1.DE"] = 255.00m,
        ["BNR.DE"] = 65.00m,
        ["SHL.DE"] = 52.00m,
        ["FME.DE"] = 42.00m,
        ["MRK.DE"] = 165.00m,
        ["QIA.DE"] = 42.00m,
        ["PAH3.DE"] = 42.00m,
        // MDAX / Tech
        ["TMV.DE"] = 12.00m,
        ["AIXA.DE"] = 18.00m,
        ["S92.DE"] = 22.00m,
        ["EVT.DE"] = 8.00m,
        ["AFX.DE"] = 68.00m,
        ["NEM.DE"] = 95.00m,
        ["WAF.DE"] = 72.00m,
        ["JEN.DE"] = 28.00m,
        ["COK.DE"] = 28.00m,
        ["GFT.DE"] = 24.00m,
        ["NA9.DE"] = 85.00m,
        ["SMHN.DE"] = 58.00m
    };

    public MockRealtimeCandleProvider(
        IMarketTimeProvider timeProvider,
        IScenarioService scenarioService,
        MarketSimulationEngine simulationEngine,
        ILogger<MockRealtimeCandleProvider> logger)
    {
        _timeProvider = timeProvider;
        _scenarioService = scenarioService;
        _simulationEngine = simulationEngine;
        _logger = logger;
    }

    public Task<List<Candle>> GetCandlesAsync(string symbol, int timeframe, string period = "1d")
    {
        var currentTime = _timeProvider.GetCurrentTime();
        var mode = _timeProvider.GetMode();

        _logger.LogDebug("MockRealtimeCandleProvider: Generating candles for {Symbol}, timeframe={Timeframe}m, period={Period}, mode={Mode}, currentTime={Time}",
            symbol, timeframe, period, mode, currentTime);

        // Calculate how many candles to generate based on period
        var candleCount = CalculateCandleCount(period, timeframe);

        // Get base price for symbol
        var basePrice = GetBasePrice(symbol);

        List<Candle> candles;

        // Check if scenario simulation is enabled
        if (_scenarioService.IsScenarioEnabled())
        {
            var scenario = _scenarioService.GetActiveScenario();

            // Check if scenario applies to this symbol (or all symbols if null)
            if (scenario.Symbol == null || scenario.Symbol.Equals(symbol, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogDebug("Using scenario '{Scenario}' for {Symbol}", scenario.Name, symbol);

                // Calculate start time for scenario
                var latestCandleTime = RoundDownToTimeframe(currentTime, timeframe);
                var startTime = latestCandleTime.AddMinutes(-((candleCount - 1) * timeframe));

                // Use simulation engine with scenario config
                var scenarioConfig = scenario with { Symbol = symbol };
                if (!scenarioConfig.StartPrice.HasValue)
                {
                    scenarioConfig = scenarioConfig with { StartPrice = basePrice };
                }

                candles = _simulationEngine.GenerateCandles(
                    scenarioConfig,
                    scenarioConfig.StartPrice ?? basePrice,
                    startTime,
                    candleCount,
                    timeframe,
                    currentTime);
            }
            else
            {
                // Scenario doesn't apply to this symbol, use simple generation
                candles = GenerateSimpleCandles(symbol, timeframe, candleCount, basePrice, currentTime);
            }
        }
        else
        {
            // No scenario, use simple deterministic generation
            candles = GenerateSimpleCandles(symbol, timeframe, candleCount, basePrice, currentTime);
        }

        _logger.LogDebug("Generated {Count} candles for {Symbol}, latest candle time: {Time}",
            candles.Count, symbol, candles.LastOrDefault()?.Time);

        return Task.FromResult(candles);
    }

    private const decimal Volatility = 0.02m; // 2% typical intraday volatility (used for simple generation)

    /// <summary>
    /// Simple deterministic candle generation (fallback when no scenario is active).
    /// </summary>
    private List<Candle> GenerateSimpleCandles(string symbol, int timeframe, int candleCount, decimal basePrice, DateTime currentTime)
    {
        var candles = new List<Candle>();

        // Calculate start time (work backwards from current time)
        var latestCandleTime = RoundDownToTimeframe(currentTime, timeframe);
        var startTime = latestCandleTime.AddMinutes(-((candleCount - 1) * timeframe));

        // Track cumulative price using a running seed
        var cumulativePrice = basePrice;
        var symbolSeed = symbol.ToUpperInvariant().GetHashCode();

        for (int i = 0; i < candleCount; i++)
        {
            var candleTime = startTime.AddMinutes(i * timeframe);

            // Skip if candle is in the future
            if (candleTime > currentTime)
                break;

            // Check if this is the current (live) candle
            var isCurrentCandle = candleTime == latestCandleTime;

            // Deterministic seed based on symbol + candle timestamp
            var seed = symbolSeed ^ (int)(candleTime.Ticks / TimeSpan.TicksPerMinute);
            var random = new Random(seed);

            Candle candle;
            if (isCurrentCandle)
            {
                // Current candle: updates every second with live price movement
                candle = GenerateLiveCandle(candleTime, cumulativePrice, timeframe, currentTime, symbolSeed);
            }
            else
            {
                // Historical candle: fully deterministic
                candle = GenerateCandle(candleTime, cumulativePrice, timeframe, random);
            }

            candles.Add(candle);

            // Update cumulative price for next candle (random walk)
            var priceDrift = cumulativePrice * Volatility * (decimal)(random.NextDouble() - 0.5) * 0.5m;
            cumulativePrice = candle.Close + priceDrift;

            // Keep price within reasonable bounds (±50% of base)
            cumulativePrice = Math.Max(basePrice * 0.5m, Math.Min(basePrice * 1.5m, cumulativePrice));
        }

        return candles;
    }

    /// <summary>
    /// Generates a live candle that updates every second.
    /// The candle starts with Open at candle start, and Close moves each second.
    /// High/Low expand as new prices are reached.
    /// </summary>
    private Candle GenerateLiveCandle(DateTime candleTime, decimal openPrice, int timeframe, DateTime currentTime, int symbolSeed)
    {
        // Calculate how many seconds into this candle we are
        var secondsIntoCandle = (int)(currentTime - candleTime).TotalSeconds;
        var totalSecondsInCandle = timeframe * 60;

        // Use candle start time for deterministic Open
        var baseSeed = symbolSeed ^ (int)(candleTime.Ticks / TimeSpan.TicksPerMinute);
        var baseRandom = new Random(baseSeed);

        var open = openPrice;

        // Simulate price ticks for each second up to current time
        var currentPrice = open;
        var high = open;
        var low = open;
        long totalVolume = 0;

        // Volume per second (distributed across candle)
        var baseVolumePerSecond = CalculateTimeBasedVolume(candleTime, timeframe, baseRandom) / totalSecondsInCandle;

        for (int sec = 0; sec <= secondsIntoCandle && sec < totalSecondsInCandle; sec++)
        {
            // Deterministic seed for this specific second
            var tickSeed = baseSeed ^ (sec * 7919); // Prime number for better distribution
            var tickRandom = new Random(tickSeed);

            // Small price movement each second (scaled volatility)
            var tickVolatility = Volatility / (decimal)Math.Sqrt(totalSecondsInCandle);
            var priceChange = currentPrice * tickVolatility * (decimal)(tickRandom.NextDouble() - 0.5) * 2m;
            currentPrice += priceChange;

            // Keep price within ±5% of open for this candle
            currentPrice = Math.Max(open * 0.95m, Math.Min(open * 1.05m, currentPrice));

            // Update high/low
            high = Math.Max(high, currentPrice);
            low = Math.Min(low, currentPrice);

            // Accumulate volume (with some randomness per tick)
            totalVolume += (long)(baseVolumePerSecond * (0.5 + tickRandom.NextDouble()));
        }

        return new Candle
        {
            Time = candleTime,
            Open = Math.Round(open, 2),
            High = Math.Round(high, 2),
            Low = Math.Round(low, 2),
            Close = Math.Round(currentPrice, 2),
            Volume = totalVolume
        };
    }

    /// <summary>
    /// Rounds a DateTime down to the nearest timeframe interval.
    /// </summary>
    private static DateTime RoundDownToTimeframe(DateTime time, int timeframeMinutes)
    {
        var totalMinutes = (long)(time.TimeOfDay.TotalMinutes);
        var roundedMinutes = (totalMinutes / timeframeMinutes) * timeframeMinutes;
        return time.Date.AddMinutes(roundedMinutes);
    }

    /// <summary>
    /// Generates a single candle with realistic OHLCV data.
    /// Volume is time-dependent: higher at market open/close.
    /// </summary>
    private Candle GenerateCandle(DateTime time, decimal basePrice, int timeframe, Random random)
    {
        // Calculate price movement
        var priceMove = basePrice * Volatility * (decimal)random.NextDouble();

        var open = basePrice;
        var close = basePrice + (decimal)(random.NextDouble() - 0.5) * priceMove;

        // High/Low based on open/close with additional range
        var high = Math.Max(open, close) + Math.Abs(priceMove) * (decimal)random.NextDouble() * 0.5m;
        var low = Math.Min(open, close) - Math.Abs(priceMove) * (decimal)random.NextDouble() * 0.5m;

        // Ensure high >= open, close and low <= open, close
        high = Math.Max(high, Math.Max(open, close));
        low = Math.Min(low, Math.Min(open, close));

        // Volume calculation with time-dependent realism
        var volume = CalculateTimeBasedVolume(time, timeframe, random);

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
    /// Calculates volume based on time of day (higher at market open/close).
    /// US Market hours: 9:30 AM - 4:00 PM ET (14:30 - 21:00 UTC)
    /// </summary>
    private static long CalculateTimeBasedVolume(DateTime time, int timeframe, Random random)
    {
        // Base volume depends on timeframe
        var baseVolume = timeframe switch
        {
            1 => 50_000L,
            5 => 200_000L,
            15 => 500_000L,
            _ => 100_000L
        };

        // Get hour in market time (approximate - assume UTC for simplicity)
        var hour = time.Hour;

        // Volume multiplier based on trading session
        // Higher at open (14:30-16:00 UTC) and close (20:00-21:00 UTC)
        double volumeMultiplier;
        if (hour >= 14 && hour < 16)
        {
            // Market open - high volume
            volumeMultiplier = 1.8 + random.NextDouble() * 0.4; // 1.8x - 2.2x
        }
        else if (hour >= 20 && hour < 21)
        {
            // Market close - high volume
            volumeMultiplier = 1.6 + random.NextDouble() * 0.4; // 1.6x - 2.0x
        }
        else if (hour >= 16 && hour < 20)
        {
            // Mid-day - normal volume
            volumeMultiplier = 0.8 + random.NextDouble() * 0.4; // 0.8x - 1.2x
        }
        else
        {
            // Outside market hours - low volume (pre/post market)
            volumeMultiplier = 0.2 + random.NextDouble() * 0.3; // 0.2x - 0.5x
        }

        return (long)(baseVolume * volumeMultiplier);
    }

    /// <summary>
    /// Calculates candle count based on period and timeframe.
    /// </summary>
    private static int CalculateCandleCount(string period, int timeframe)
    {
        // Minutes in each period (approximate trading hours)
        var periodMinutes = period.ToLower() switch
        {
            "1d" => 390,        // 1 trading day ~6.5 hours
            "5d" => 1950,       // 5 trading days
            "7d" => 2730,       // 7 trading days
            "1mo" => 8190,      // ~21 trading days
            "60d" => 23400,     // ~60 trading days
            _ => 390            // Default to 1 day
        };

        return Math.Max(1, periodMinutes / timeframe);
    }

    /// <summary>
    /// Gets the base price for a symbol.
    /// </summary>
    private static decimal GetBasePrice(string symbol)
    {
        if (BasePrices.TryGetValue(symbol, out var price))
        {
            return price;
        }

        // Generate a consistent price for unknown symbols based on symbol hash
        var hash = Math.Abs(symbol.ToUpperInvariant().GetHashCode());
        return 50m + (hash % 450); // Price range: $50 - $500
    }
}
