## Plan: Mock Real-Time Candle Generator

Erstelle einen neuen Mock-Datengenerator, der Yahoo Finance ersetzt und jede Sekunde neue Candles generiert. Der Generator erzeugt historische Daten und streamt kontinuierlich neue Candles über SignalR.

### Steps

1. **Neuen Provider erstellen**: Erstelle `MockRealtimeCandleProvider` in [API/Data/](API/Data/) der `IMarketDataProvider` implementiert und `IMarketTimeProvider` injiziert, um zeitabhängige Candle-Generierung zu ermöglichen (basierend auf Patterns aus [MockMarketDataProvider.cs](API/Data/MockMarketDataProvider.cs))

2. **Candle-Generierung mit Seed-basierter Konsistenz**: Implementiere deterministische Candle-Generierung mit Symbol+Timestamp als Seed, sodass historische Candles bei erneutem Abruf identisch bleiben (Random Walk mit fester Seed-Basis)

3. **Provider-Registrierung aktualisieren**: Ersetze Yahoo-Registrierung in [Program.cs](API/Program.cs#L58-L80) durch den neuen Mock-Provider für beide Modi (Live + Replay)

4. **Background-Service für Echtzeit-Updates**: Erweitere [ReplayClockService.cs](API/Services/ReplayClockService.cs) oder erstelle neuen Service, der jede Sekunde `ReceiveChartRefresh` via SignalR pusht, um neue Candles anzuzeigen

5. **Frontend-Anpassung prüfen**: Verifiziere, dass bestehende SignalR-Handler in der Frontend-Chart-Komponente auf `ReceiveChartRefresh` reagieren und Daten neu laden

### Further Considerations

1. **Seed-Strategie für Determinismus**: Sollen historische Candles bei jedem Aufruf identisch sein (deterministic seed = `symbol.GetHashCode() + timestamp.Ticks`)? **Empfehlung: Ja**, für konsistentes Backtesting
2. **Volatilität und Realismus**: Soll die Volatilität konfigurierbar sein (z.B. per Symbol oder global)? **Empfehlung: Global 2% wie in MockMarketDataProvider**
3. **Volume-Simulation**: Soll Volume realistisch mit Handelszeiten variieren (höher bei Market Open/Close)? **Option A:** Konstant / **Option B:** Zeitabhängig realistisch
