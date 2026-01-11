using API.Models;

namespace API.Data;

/// <summary>
/// Mock implementation of market data provider for testing and local development.
/// DO NOT use in production - use YahooFinanceMarketDataProvider instead.
/// </summary>
public class MockMarketDataProvider : IMarketDataProvider
{
    private readonly Random _random = new();

    public Task<List<Candle>> GetCandlesAsync(string symbol, int timeframe, string period = "1d")
    {
        // Parse period to determine candle count
        var candleCount = CalculateCandleCount(period, timeframe);

        // Base price depends on symbol
        var basePrice = GetBasePrice(symbol);
        var candles = new List<Candle>();

        // Start time - work backwards from now
        var currentTime = DateTime.UtcNow;

        // Generate candles backwards in time
        for (int i = candleCount - 1; i >= 0; i--)
        {
            var time = currentTime.AddMinutes(-i * timeframe);
            var candle = GenerateCandle(time, basePrice, timeframe);
            candles.Add(candle);

            // Let price drift slightly for next candle
            basePrice = candle.Close + (decimal)(_random.NextDouble() - 0.5) * 2;
        }

        // Sort by time ascending
        candles = candles.OrderBy(c => c.Time).ToList();

        return Task.FromResult(candles);
    }

    private int CalculateCandleCount(string period, int timeframe)
    {
        // Rough estimation of candle count based on period
        return period switch
        {
            "1d" => (int)(390 / timeframe), // Trading day ~6.5 hours
            "5d" => (int)(1950 / timeframe), // 5 trading days
            "1mo" => (int)(8190 / timeframe), // ~21 trading days
            _ => 100
        };
    }

    private Candle GenerateCandle(DateTime time, decimal basePrice, int timeframe)
    {
        // Generate realistic OHLC data
        var volatility = 0.02m; // 2% typical intraday volatility
        var priceMove = basePrice * volatility * (decimal)_random.NextDouble();

        var open = basePrice;
        var close = basePrice + (decimal)(_random.NextDouble() - 0.5) * priceMove;

        // High/Low based on open/close
        var high = Math.Max(open, close) + Math.Abs(priceMove) * (decimal)_random.NextDouble();
        var low = Math.Min(open, close) - Math.Abs(priceMove) * (decimal)_random.NextDouble();

        // Volume varies by timeframe
        var baseVolume = timeframe switch
        {
            1 => 50000,
            5 => 200000,
            15 => 500000,
            _ => 100000
        };

        var volume = (long)(baseVolume * (0.5 + _random.NextDouble()));

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

    private decimal GetBasePrice(string symbol)
    {
        // Mock base prices for common symbols
        return symbol.ToUpper() switch
        {
            "AAPL" => 180.00m,
            "MSFT" => 370.00m,
            "TSLA" => 250.00m,
            "GOOGL" => 140.00m,
            "AMZN" => 155.00m,
            "NVDA" => 480.00m,
            "META" => 340.00m,
            "AMD" => 140.00m,
            "NFLX" => 480.00m,
            "SPY" => 470.00m,
            _ => 100.00m
        };
    }
}
