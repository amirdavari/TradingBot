using API.Data;
using API.Hubs;
using API.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

/// <summary>
/// Background service that automatically opens trades when conditions are met.
/// Monitors scanner results and opens trades when:
/// - Confidence >= AutoTradeMinConfidence (default: 100)
/// - Direction is LONG or SHORT (not NONE)
/// - No existing open trade for the same symbol
/// - AutoTrading is enabled in settings
/// </summary>
public class AutoTradeService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<TradingHub> _hubContext;
    private readonly ILogger<AutoTradeService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromSeconds(10);

    public AutoTradeService(
        IServiceProvider serviceProvider,
        IHubContext<TradingHub> hubContext,
        ILogger<AutoTradeService> logger)
    {
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("AutoTradeService started");

        // Wait a bit before starting to ensure other services are initialized
        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndExecuteAutoTradesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AutoTradeService");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("AutoTradeService stopped");
    }

    private async Task CheckAndExecuteAutoTradesAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var riskService = scope.ServiceProvider.GetRequiredService<RiskManagementService>();
        var tradeService = scope.ServiceProvider.GetRequiredService<PaperTradeService>();
        var signalService = scope.ServiceProvider.GetRequiredService<SignalService>();
        var accountService = scope.ServiceProvider.GetRequiredService<AccountService>();

        // Get auto-trade settings
        var settings = await riskService.GetSettingsAsync();
        
        if (!settings.AutoTradeEnabled)
        {
            return; // Auto-trading is disabled
        }

        _logger.LogDebug("AutoTrade check: Enabled, MinConfidence={MinConf}", settings.AutoTradeMinConfidence);

        // Get watchlist symbols
        var watchlistSymbols = await dbContext.WatchlistSymbols
            .Select(w => w.Symbol)
            .ToListAsync(stoppingToken);

        if (watchlistSymbols.Count == 0)
        {
            return; // No symbols to monitor
        }

        // Get currently open trades to avoid duplicates
        var openTrades = await tradeService.GetOpenTradesAsync();
        var openTradeSymbols = openTrades.Select(t => t.Symbol).ToHashSet();

        // Check if we've reached max concurrent auto-trades
        // AUTO-TRADE marker is stored in Reasons list
        var autoTradeCount = openTrades.Count(t => t.Reasons != null && t.Reasons.Contains("AUTO-TRADE"));
        if (autoTradeCount >= settings.AutoTradeMaxConcurrent)
        {
            _logger.LogDebug("AutoTrade: Max concurrent trades reached ({Count}/{Max})", 
                autoTradeCount, settings.AutoTradeMaxConcurrent);
            return;
        }

        // Check each watchlist symbol for auto-trade opportunities
        foreach (var symbol in watchlistSymbols)
        {
            if (stoppingToken.IsCancellationRequested) break;

            // Skip if already have an open trade for this symbol
            if (openTradeSymbols.Contains(symbol))
            {
                continue;
            }

            try
            {
                // Get signal for this symbol
                var signal = await signalService.GenerateSignalAsync(symbol, timeframe: 5);

                // Check if conditions are met
                if (signal.Direction == "NONE")
                {
                    continue;
                }

                if (signal.Confidence < settings.AutoTradeMinConfidence)
                {
                    _logger.LogDebug("AutoTrade: {Symbol} confidence {Confidence} < {MinConf}, skipping",
                        symbol, signal.Confidence, settings.AutoTradeMinConfidence);
                    continue;
                }

                // Conditions met! Calculate risk and open trade
                _logger.LogInformation("AutoTrade: Opening {Direction} trade for {Symbol} (Confidence: {Confidence})",
                    signal.Direction, symbol, signal.Confidence);

                var riskCalc = await riskService.CalculateTradeRiskAsync(
                    symbol,
                    signal.Entry,
                    signal.StopLoss,
                    signal.TakeProfit,
                    settings.AutoTradeRiskPercent);

                if (!riskCalc.IsAllowed)
                {
                    _logger.LogWarning("AutoTrade: Risk calculation not allowed for {Symbol}: {Messages}",
                        symbol, string.Join(", ", riskCalc.Messages));
                    continue;
                }

                // Add AUTO-TRADE marker to reasons for tracking
                signal.Reasons.Add("AUTO-TRADE");

                var (success, message, trade) = await tradeService.OpenTradeAsync(signal, riskCalc);

                if (success && trade != null)
                {
                    _logger.LogInformation("AutoTrade: Successfully opened trade #{Id} for {Symbol}",
                        trade.Id, symbol);

                    // Notify clients via SignalR
                    await _hubContext.Clients.All.SendAsync(
                        TradingHubMethods.ReceiveTradeUpdate,
                        trade,
                        stoppingToken);

                    // Also send account update
                    var account = await accountService.GetOrCreateAccountAsync();
                    await _hubContext.Clients.All.SendAsync(
                        TradingHubMethods.ReceiveAccountUpdate,
                        account,
                        stoppingToken);

                    // Check if we've now reached the max concurrent trades
                    autoTradeCount++;
                    if (autoTradeCount >= settings.AutoTradeMaxConcurrent)
                    {
                        _logger.LogInformation("AutoTrade: Reached max concurrent trades ({Max}), stopping scan",
                            settings.AutoTradeMaxConcurrent);
                        break;
                    }
                }
                else
                {
                    _logger.LogWarning("AutoTrade: Failed to open trade for {Symbol}: {Message}",
                        symbol, message);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AutoTrade: Error processing symbol {Symbol}", symbol);
            }
        }
    }
}
