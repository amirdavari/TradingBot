using API.Data;
using API.Models;

namespace API.Services;

/// <summary>
/// Service for calculating trading signals based on technical analysis.
/// Provides entry, stop-loss, and take-profit levels with confidence scoring.
/// </summary>
public class SignalService
{
    private readonly IMarketDataProvider _marketDataProvider;
    private readonly ILogger<SignalService> _logger;

    public SignalService(IMarketDataProvider marketDataProvider, ILogger<SignalService> logger)
    {
        _marketDataProvider = marketDataProvider;
        _logger = logger;
    }

    /// <summary>
    /// Generates a trading signal for a given symbol.
    /// </summary>
    public async Task<TradeSignal> GenerateSignalAsync(string symbol, int timeframe = 5, string period = "1d")
    {
        _logger.LogInformation("Generating signal for {Symbol} with timeframe {Timeframe}", symbol, timeframe);

        var candles = await _marketDataProvider.GetCandlesAsync(symbol, timeframe, period);

        if (candles.Count < 20)
        {
            _logger.LogWarning("Insufficient data for {Symbol}. Need at least 20 candles.", symbol);
            return CreateNoSignal(symbol);
        }

        // Calculate indicators
        var vwap = CalculateVWAP(candles);
        var avgVolume = CalculateAverageVolume(candles);
        var currentCandle = candles.Last();
        var currentPrice = currentCandle.Close;

        // Determine trend
        var isAboveVWAP = currentPrice > vwap;
        var volumeRatio = (double)currentCandle.Volume / avgVolume;

        // Calculate volatility (ATR-like)
        var atr = CalculateATR(candles, 14);

        // Determine direction and levels
        var signal = new TradeSignal
        {
            Symbol = symbol,
            Direction = "NONE",
            Entry = currentPrice,
            StopLoss = 0,
            TakeProfit = 0,
            Confidence = 0,
            Reasons = new List<string>()
        };

        // LONG Signal Logic
        if (isAboveVWAP && volumeRatio > 1.2)
        {
            signal.Direction = "LONG";
            signal.Entry = currentPrice;
            signal.StopLoss = currentPrice - (atr * 1.5m);
            signal.TakeProfit = currentPrice + (atr * 2.5m);
            signal.Reasons.Add($"Price ({currentPrice:F2}) above VWAP ({vwap:F2})");
            signal.Reasons.Add($"Volume {volumeRatio:F2}x above average");
            signal.Confidence = CalculateConfidence(isAboveVWAP, volumeRatio, atr, currentPrice);
        }
        // SHORT Signal Logic
        else if (!isAboveVWAP && volumeRatio > 1.2)
        {
            signal.Direction = "SHORT";
            signal.Entry = currentPrice;
            signal.StopLoss = currentPrice + (atr * 1.5m);
            signal.TakeProfit = currentPrice - (atr * 2.5m);
            signal.Reasons.Add($"Price ({currentPrice:F2}) below VWAP ({vwap:F2})");
            signal.Reasons.Add($"Volume {volumeRatio:F2}x above average");
            signal.Confidence = CalculateConfidence(!isAboveVWAP, volumeRatio, atr, currentPrice);
        }
        // No Signal
        else
        {
            signal.Direction = "NONE";
            signal.Reasons.Add("No clear trend or insufficient volume");
            if (volumeRatio <= 1.2)
            {
                signal.Reasons.Add($"Volume only {volumeRatio:F2}x average (need >1.2x)");
            }
            signal.Confidence = 0;
        }

        // Validate Risk/Reward ratio
        if (signal.Direction != "NONE")
        {
            var risk = Math.Abs(signal.Entry - signal.StopLoss);
            var reward = Math.Abs(signal.TakeProfit - signal.Entry);
            var riskRewardRatio = reward / risk;

            signal.Reasons.Add($"Risk/Reward: 1:{riskRewardRatio:F2}");

            if (riskRewardRatio < 1.5m)
            {
                signal.Confidence = Math.Max(0, signal.Confidence - 20);
                signal.Reasons.Add("Warning: Risk/Reward below 1.5");
            }
        }

        _logger.LogInformation("Signal generated for {Symbol}: {Direction} with confidence {Confidence}",
            symbol, signal.Direction, signal.Confidence);

        return signal;
    }

    /// <summary>
    /// Calculates Volume Weighted Average Price (VWAP).
    /// </summary>
    private decimal CalculateVWAP(List<Candle> candles)
    {
        decimal cumulativePriceVolume = 0;
        long cumulativeVolume = 0;

        foreach (var candle in candles)
        {
            var typicalPrice = (candle.High + candle.Low + candle.Close) / 3;
            cumulativePriceVolume += typicalPrice * candle.Volume;
            cumulativeVolume += candle.Volume;
        }

        return cumulativeVolume > 0 ? cumulativePriceVolume / cumulativeVolume : 0;
    }

    /// <summary>
    /// Calculates average volume over all candles.
    /// </summary>
    private double CalculateAverageVolume(List<Candle> candles)
    {
        if (candles.Count == 0) return 0;
        return candles.Average(c => (double)c.Volume);
    }

    /// <summary>
    /// Calculates Average True Range (ATR) for volatility measurement.
    /// </summary>
    private decimal CalculateATR(List<Candle> candles, int period)
    {
        if (candles.Count < period + 1) return 0;

        var trueRanges = new List<decimal>();

        for (int i = 1; i < candles.Count; i++)
        {
            var current = candles[i];
            var previous = candles[i - 1];

            var tr1 = current.High - current.Low;
            var tr2 = Math.Abs(current.High - previous.Close);
            var tr3 = Math.Abs(current.Low - previous.Close);

            var trueRange = Math.Max(tr1, Math.Max(tr2, tr3));
            trueRanges.Add(trueRange);
        }

        // Take last 'period' true ranges
        var recentTRs = trueRanges.TakeLast(period).ToList();
        return recentTRs.Average();
    }

    /// <summary>
    /// Calculates confidence score (0-100) based on signal strength.
    /// </summary>
    private int CalculateConfidence(bool trendConfirmed, double volumeRatio, decimal atr, decimal price)
    {
        int confidence = 50; // Base confidence

        // Trend confirmation
        if (trendConfirmed)
        {
            confidence += 20;
        }

        // Volume strength
        if (volumeRatio > 1.5)
        {
            confidence += 15;
        }
        else if (volumeRatio > 1.2)
        {
            confidence += 10;
        }

        // Volatility (lower is better for entries)
        var volatilityRatio = atr / price;
        if (volatilityRatio < 0.015m) // Less than 1.5% volatility
        {
            confidence += 10;
        }
        else if (volatilityRatio > 0.03m) // More than 3% volatility
        {
            confidence -= 10;
        }

        // Ensure confidence stays within 0-100
        return Math.Clamp(confidence, 0, 100);
    }

    /// <summary>
    /// Creates a "no signal" response.
    /// </summary>
    private TradeSignal CreateNoSignal(string symbol)
    {
        return new TradeSignal
        {
            Symbol = symbol,
            Direction = "NONE",
            Entry = 0,
            StopLoss = 0,
            TakeProfit = 0,
            Confidence = 0,
            Reasons = new List<string> { "Insufficient data or no clear signal" }
        };
    }
}
