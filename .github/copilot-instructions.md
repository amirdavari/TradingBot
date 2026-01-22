# AI Broker Trading Bot – Copilot Instructions

## Project Overview
AI Broker is a web-based paper trading platform for daytrading analysis with dual-mode architecture: **Live** (delayed market data) and **Replay** (historical backtesting with time simulation).

**Key Principle**: This is a decision support tool for paper trading only—no real money, no broker integration, no automated execution.

## Architecture

### Dual-Mode System (Critical)
The system operates in two distinct modes controlled by `IMarketTimeProvider`:
- **Live Mode**: Uses `DateTime.UtcNow` + Yahoo Finance delayed data
- **Replay Mode**: Uses simulated time (`ReplayState.CurrentTime`) + historical Yahoo data

**NEVER** use `DateTime.Now` or `DateTime.UtcNow` directly in business logic—always inject and use `IMarketTimeProvider.GetCurrentTime()`. This abstraction enables time-travel debugging and backtesting.

Mode switching happens in [Program.cs](API/Program.cs#L58-L80) where different provider implementations are registered based on `IMarketTimeProvider.GetMode()`.

### Backend Architecture (C# / .NET)
**Strict separation of concerns** (see [Backend_architektur.md](docs/Backend_architektur.md)):

1. **Controllers** ([API/Controllers/](API/Controllers/)): HTTP only—no business logic, no calculations
2. **Services** ([API/Services/](API/Services/)): All business logic lives here
   - `SignalService`: Technical analysis + news sentiment → trading signals
   - `PaperTradeService`: Trade lifecycle (open/close/monitor)
   - `RiskManagementService`: Position sizing and risk validation
   - `ScannerService`: Multi-symbol screening
3. **Data Providers** ([API/Data/](API/Data/)): Abstracted via interfaces
   - `IMarketDataProvider`: Candle data (Mock, Yahoo, YahooReplay implementations)
   - `INewsProvider`: News + sentiment (Mock, Yahoo, ReplayMock implementations)
   - Mode-specific implementations wrap base providers with time filtering

**Data Flow**: Controller → Service(s) → Data Provider(s) → External API/DB

### Frontend Architecture (React + TypeScript)
**Zero business logic in frontend** (see [Frontend_Architektur.md](docs/Frontend_Architektur.md)):

- **Backend is single source of truth** for all calculations
- Frontend only handles: UI state, API calls, chart rendering, user interactions
- API layer ([frontend/src/api/](frontend/src/api/)): `config.ts` for base URL, typed functions per domain
- Charts: TradingView Lightweight Charts (native JS, no React wrapper)
- Material UI for components (use `mui-mcp` for best practices)

**Data Flow**: User Action → API Call → Update State → Render

### Database (SQLite + EF Core)
- **Single SQLite file**: `aibroker.db` (MVP simplicity)
- **Auto-migration**: `EnsureCreated()` in [Program.cs](API/Program.cs#L109-L113)
- **Replay state persistence**: `ReplayStateEntity` uses singleton pattern (`Id = 1`) to survive restarts
- Migrations in [API/Migrations/](API/Migrations/) (see [DB_Migration_Strategy.md](docs/DB_Migration_Strategy.md))

## Key Workflows

### Running the Application
```powershell
# Backend (default: https://localhost:7050)
cd API
dotnet run

# Frontend (default: http://localhost:5173)
cd frontend
npm run dev
```

**CORS**: Frontend ports 5173 and 5174 whitelisted in [Program.cs](API/Program.cs#L15-L22)

### Signal Generation Logic
Signals are generated in [SignalService.cs](API/Services/SignalService.cs) using:
1. **Technical indicators**: VWAP, ATR, volume ratios (minimum 20 candles required)
2. **News sentiment**: Analyzed via `INewsProvider`, adds ±15 confidence points
3. **Risk/Reward**: Entry/SL/TP levels calculated from ATR multiples
4. **Confidence scoring**: 0-100 based on volume ratio, trend alignment, sentiment

**Direction Logic**:
- `LONG`: Price above VWAP + volume > 1.2x average
- `SHORT`: Price below VWAP + volume > 1.2x average
- `NONE`: Insufficient conviction

### Paper Trading Lifecycle
Managed by [PaperTradeService.cs](API/Services/PaperTradeService.cs):
1. **Open**: Validates signal → checks risk → allocates capital → creates `PaperTrade` entity
2. **Monitor**: `PaperTradeMonitorService` (background service) checks SL/TP every 10s in Live, faster in Replay
3. **Close**: Automatic on SL/TP hit, or manual via API

**Capital Management**: [AccountService.cs](API/Services/AccountService.cs) tracks allocated vs. available capital (default $100k)

### Replay/Simulation Mode
Time-traveling for backtesting (see [Phase_10.7_Persistence_Implementation.md](docs/Phase_10.7_Persistence_Implementation.md)):
- **ReplayClockService** (background service): Advances `CurrentTime` at configurable speed
- **State persistence**: Saved to DB on every change, loaded on startup
- **Data filtering**: Yahoo providers filter historical data by `CurrentTime`
- **Frontend refresh**: `useReplayRefresh` hook polls when replay is running

## Critical Patterns

### Provider Registration Pattern
[Program.cs](API/Program.cs#L44-L80) uses **factory delegates** to register mode-specific implementations:
```csharp
builder.Services.AddScoped<INewsProvider>(sp => {
    var mode = sp.GetRequiredService<IMarketTimeProvider>().GetMode();
    return mode == MarketMode.Replay 
        ? /* ReplayProvider */ 
        : /* LiveProvider */;
});
```

### Service Dependencies
Services are `Scoped` (per-request), time provider is `Singleton` (shared state). Complex dependency: `MarketTimeProvider` needs `IServiceProvider` to create scoped `DbContext` for persistence ([MarketTimeProvider.cs](API/Services/MarketTimeProvider.cs)).

### Frontend API Calls
Always use `fetchWithConfig` from [config.ts](frontend/src/api/config.ts) for centralized error handling and base URL configuration. Type definitions in [models/index.ts](frontend/src/models/).

## Data Sources
- **Market Data**: Yahoo Finance (delayed, no realtime)
- **News**: Yahoo News (simplified sentiment: positive/neutral/negative)
- **Limitations**: Historical data for MVP, no tick data, no order book

See [Data_Source.md](docs/Data_Source.md) for constraints and provider abstraction rationale.

## Common Gotchas
1. **Time provider**: Inject `IMarketTimeProvider`, never use `DateTime.UtcNow` in services
2. **Mode switching**: Restart backend required for provider registration changes
3. **Candle count**: Signals need ≥20 candles; adjust period for timeframe (1m=7d, 5m/15m=5d)
4. **Frontend state**: Chart refresh requires new array reference (`[...data]`) even if content unchanged
5. **Background services**: `ReplayClockService` and `PaperTradeMonitorService` run independently

## Testing
- Postman collection: [AI_Broker_MVP.postman_collection.json](AI_Broker_MVP.postman_collection.json)
- Manual testing workflow: Scanner → Signal → Risk Calculation → Open Trade → Monitor

## MCP Helpers
- Use `microsoft-mcp` for .NET/EF Core best practices
- Use `mui-mcp` for Material UI patterns in frontend
