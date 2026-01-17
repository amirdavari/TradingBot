using API.Data;
using API.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5174")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Configure Database (SQLite for MVP)
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=aibroker.db"));

// Register Yahoo Finance Provider with HttpClient
builder.Services.AddHttpClient<YahooFinanceMarketDataProvider>();
builder.Services.AddHttpClient<INewsProvider, YahooNewsProvider>();

// Register Market Time Provider (Singleton for replay state)
builder.Services.AddSingleton<IMarketTimeProvider, MarketTimeProvider>();

// Register Market Data Provider based on mode
// In Replay mode, YahooReplayMarketDataProvider wraps YahooFinanceMarketDataProvider
builder.Services.AddScoped<IMarketDataProvider>(sp =>
{
    var timeProvider = sp.GetRequiredService<IMarketTimeProvider>();
    var mode = timeProvider.GetMode();
    
    if (mode == API.Models.MarketMode.Replay)
    {
        var yahooProvider = sp.GetRequiredService<YahooFinanceMarketDataProvider>();
        var logger = sp.GetRequiredService<ILogger<YahooReplayMarketDataProvider>>();
        return new YahooReplayMarketDataProvider(yahooProvider, timeProvider, logger);
    }
    else
    {
        return sp.GetRequiredService<YahooFinanceMarketDataProvider>();
    }
});

builder.Services.AddSingleton<MarketTimeProvider>(sp => 
    (MarketTimeProvider)sp.GetRequiredService<IMarketTimeProvider>());

// Register Replay Clock Service (Background Service)
builder.Services.AddHostedService<ReplayClockService>();
builder.Services.AddSingleton<ReplayClockService>(sp =>
    sp.GetServices<IHostedService>()
        .OfType<ReplayClockService>()
        .First());

// Register Business Services
builder.Services.AddScoped<SignalService>();
builder.Services.AddScoped<ScannerService>();
builder.Services.AddScoped<SymbolValidationService>();

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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
