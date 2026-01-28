using API.Data;
using API.Hubs;
using API.Services;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add SignalR for real-time updates
builder.Services.AddSignalR();

// Configure CORS (AllowCredentials required for SignalR)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

// Configure Database (SQLite for MVP)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=aibroker.db"));

// Register Mock News Provider for Replay mode
builder.Services.AddSingleton<MockNewsProvider>();

// Register Market Time Provider (Singleton for replay state)
builder.Services.AddSingleton<IMarketTimeProvider, MarketTimeProvider>();

// Register News Provider based on mode
// In Replay mode, use ReplayMockNewsProvider (generates mock news)
// In Live mode, use MockNewsProvider (no external API dependency)
builder.Services.AddScoped<INewsProvider>(sp =>
{
    var timeProvider = sp.GetRequiredService<IMarketTimeProvider>();
    var mode = timeProvider.GetMode();

    if (mode == API.Models.MarketMode.Replay)
    {
        var mockProvider = sp.GetRequiredService<MockNewsProvider>();
        var logger = sp.GetRequiredService<ILogger<ReplayMockNewsProvider>>();
        return new ReplayMockNewsProvider(mockProvider, timeProvider, logger);
    }
    else
    {
        // Live mode: use MockNewsProvider directly
        return sp.GetRequiredService<MockNewsProvider>();
    }
});

// Register Market Data Provider - switches between Yahoo (Live) and Mock (Replay)
// Live mode: YahooFinanceMarketDataProvider (delayed market data)
// Replay/Mock mode: MockRealtimeCandleProvider (generated scenario-based data)
builder.Services.AddSingleton<MarketSimulationEngine>();
builder.Services.AddSingleton<IScenarioService, ScenarioService>();
builder.Services.AddHttpClient<YahooFinanceMarketDataProvider>();
builder.Services.AddScoped<IMarketDataProvider>(sp =>
{
    var timeProvider = sp.GetRequiredService<IMarketTimeProvider>();
    var mode = timeProvider.GetMode();

    if (mode == API.Models.MarketMode.Live)
    {
        // Live mode: use Yahoo Finance (delayed data)
        return sp.GetRequiredService<YahooFinanceMarketDataProvider>();
    }
    else
    {
        // Replay/Mock mode: use generated candle data with scenarios
        var scenarioService = sp.GetRequiredService<IScenarioService>();
        var simulationEngine = sp.GetRequiredService<MarketSimulationEngine>();
        var logger = sp.GetRequiredService<ILogger<MockRealtimeCandleProvider>>();
        return new MockRealtimeCandleProvider(timeProvider, scenarioService, simulationEngine, logger);
    }
});

builder.Services.AddSingleton<MarketTimeProvider>(sp =>
    (MarketTimeProvider)sp.GetRequiredService<IMarketTimeProvider>());

// Register Business Services
builder.Services.AddScoped<SignalService>();
builder.Services.AddScoped<ScannerService>();
builder.Services.AddScoped<SymbolValidationService>();
builder.Services.AddScoped<AccountService>();
builder.Services.AddScoped<RiskManagementService>();
builder.Services.AddScoped<PaperTradeService>();

// Register PaperTrade Monitor Background Service
builder.Services.AddHostedService<PaperTradeMonitorService>();

// Register Live Chart Refresh Service (pushes SignalR updates in Live mode)
builder.Services.AddHostedService<LiveChartRefreshService>();

// Register Auto-Trade Service (monitors watchlist and opens trades automatically)
builder.Services.AddHostedService<AutoTradeService>();

var app = builder.Build();

// Ensure database is created (MVP: auto-migration)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    db.Database.EnsureCreated();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Use CORS
app.UseCors("AllowFrontend");

// Only use HTTPS redirection in production
// In development, we want to support both HTTP and HTTPS for easier debugging
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();

app.MapControllers();

// Map SignalR Hub
app.MapHub<TradingHub>("/hubs/trading");

app.Run();
