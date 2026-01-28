## Plan: Realistic Market Simulation mit Regime + Pattern Overlays

Erweitere den `MockRealtimeCandleProvider` um ein dreischichtiges Simulationssystem (Basisprozess, Regimes, Pattern-Overlays) mit Frontend-Konfiguration für reproduzierbare Szenario-Tests.

### Steps

1. **Domain Models erstellen**: Erstelle [API/Models/ScenarioModels.cs](API/Models/ScenarioModels.cs) mit `MarketRegime` (enum: TREND_UP, TREND_DOWN, RANGE, HIGH_VOL, LOW_VOL, CRASH), `PatternOverlay` (BREAKOUT, PULLBACK, DOUBLE_TOP, HEAD_SHOULDERS, TRIANGLE, FLAG, GAP_AND_GO, MEAN_REVERSION), `ScenarioConfig` (regimes[], overlays[], seed, basePrice) und `ScenarioConfigEntity` für DB-Persistenz

2. **Market Simulation Engine erstellen**: Erstelle [API/Services/MarketSimulationEngine.cs](API/Services/MarketSimulationEngine.cs) mit drei Schichten:
   - `BaseProcessGenerator`: Returns-basierte Generierung mit EWMA-Volatility-Clustering, Fat-Tails, Drift, Session-Effekte, Gap-Wahrscheinlichkeit
   - `RegimeStateMachine`: Steuert Parameter (Volatilität, Drift, Mean-Reversion) basierend auf aktuellem Regime
   - `PatternOverlayProcessor`: Formt Basisdaten zu erkennbaren Patterns (Breakout mit Volume-Boost, Pullback nach Trend, etc.)

3. **ScenarioService + Controller erstellen**: 
   - [API/Services/ScenarioService.cs](API/Services/ScenarioService.cs): Verwaltet aktive Szenario-Konfiguration, Preset-Library, DB-Persistenz
   - [API/Controllers/ScenarioController.cs](API/Controllers/ScenarioController.cs): `GET /presets`, `GET /current`, `POST /apply`, `POST /custom` mit JSON-Szenario-Definition

4. **MockRealtimeCandleProvider refactoren**: Modifiziere [MockRealtimeCandleProvider.cs](API/Data/MockRealtimeCandleProvider.cs) um `IScenarioService` zu injizieren, Returns-basierte OHLCV-Generierung (`close = prevClose * (1 + return)`), Open-Gaps, High/Low aus Intrabar-Range (ATR-proportional), Volume-Boosts bei Pattern-Events

5. **Preset-Szenarien definieren**: Erstelle vordefinierte Szenarien in `ScenarioService`:
   - "VWAP Long Test": 200 RANGE → 300 TREND_UP mit BREAKOUT bei Bar 180, PULLBACK 181-220
   - "High Volume Breakout": Volume-Spike 2.5x bei Ausbruch
   - "Low Confidence Range": Niedriges Volumen (<1.2x), hohe Volatilität (>3%)
   - "ATR Test": HIGH_VOL Phase für breite SL/TP-Berechnung

6. **Frontend Scenario-Konfiguration**: 
   - Entferne alle nicht verwendeten Einstellungen in [SimulationControlPanel.tsx](frontend/src/pages/SimulationControlPanel.tsx) 
   - Erweitere [SimulationControlPanel.tsx](frontend/src/pages/SimulationControlPanel.tsx) um "Market Scenarios" Sektion
   - Erstelle [ScenarioConfigDialog.tsx](frontend/src/components/ScenarioConfigDialog.tsx) mit Preset-Dropdown, Regime-Timeline-Editor (drag & drop), Pattern-Overlay-Liste, Seed-Input
   - Erstelle [scenarioApi.ts](frontend/src/api/scenarioApi.ts) für API-Calls

7. **DB-Migration + SignalR-Integration**: 
   - Aktualisiere [ApplicationDbContext.cs](API/Data/ApplicationDbContext.cs) mit `DbSet<ScenarioConfigEntity>`
   - Erstelle Migration, füge SignalR-Broadcast `ReceiveScenarioChange` hinzu für Live-Updates

### Further Considerations

1. **Szenario-Serialisierung**: Soll das JSON-Format wie im Beispiel (`{"regimes": [...], "overlays": [...]}`) direkt als String in DB gespeichert werden, oder als normalisierte Tabellen (ScenarioRegimes, ScenarioOverlays)? **Empfehlung: JSON-String** für Flexibilität und einfacheres Import/Export

2. **Pattern-Overlay Präzision**: Sollen Patterns "garantiert" bei exakter Bar-Nummer auftreten, oder mit Noise-Toleranz (±2-3 Bars)?  **Option B:** Mit Noise für Realismus

3. **Performance bei vielen Szenarien**: Soll die Engine Candles on-demand generieren (aktueller Ansatz) oder bei Szenario-Aktivierung vorab cachen? **Empfehlung: On-demand** mit optionalem Pre-Generation für lange Zeiträume
