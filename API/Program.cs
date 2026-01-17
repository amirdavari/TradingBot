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

// Register HttpClient for Yahoo Finance
builder.Services.AddHttpClient<IMarketDataProvider, YahooFinanceMarketDataProvider>();
builder.Services.AddHttpClient<INewsProvider, YahooNewsProvider>();

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
