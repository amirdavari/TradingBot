using API.Data;
using API.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register HttpClient for Yahoo Finance
builder.Services.AddHttpClient<IMarketDataProvider, YahooFinanceMarketDataProvider>();
builder.Services.AddHttpClient<INewsProvider, YahooNewsProvider>();

// Register Business Services
builder.Services.AddScoped<SignalService>();
builder.Services.AddScoped<ScannerService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
