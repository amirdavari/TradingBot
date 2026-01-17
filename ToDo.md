# AI Broker – MVP TODO Liste

Diese TODO-Liste beschreibt alle Schritte zur Umsetzung des MVP.
Sie ist bewusst linear aufgebaut und dient als Arbeitsgrundlage
für Entwickler und GitHub Copilot.

Status:
- [ ] offen
- [x] erledigt

---

## PHASE 1 – Backend Grundgerüst (C# / .NET)

### 1.1 Projekt Setup
- [x] ASP.NET Core Web API (.NET 10) erstellen
- [x] HTTPS & Launch Settings prüfen

### 1.2 Ordnerstruktur
- [x] Controllers Ordner anlegen
- [x] Services Ordner anlegen
- [x] Models Ordner anlegen
- [x] Data Ordner anlegen

---

## PHASE 2 – Domain Modelle

- [x] Candle Modell erstellen
  - Time
  - Open / High / Low / Close
  - Volume
- [x] NewsItem Modell erstellen
  - Title
  - Summary
  - Sentiment
  - PublishedAt
- [x] TradeSignal Modell erstellen
  - Symbol
  - Direction (LONG / SHORT / NONE)
  - Entry
  - StopLoss
  - TakeProfit
  - Confidence
  - Reasons (List<string>)

---

## PHASE 3 – Marktdaten (Mock / MVP) und Marktdaten (Yahoo Finance)

### 3.1 Data Provider Schnittstelle
- [x] IMarketDataProvider Interface definieren
      - Methode: Laden von OHLCV-Candles
      - Parameter: Symbol, Timeframe, Zeitraum
      - Rückgabe: normalisierte Candle-Modelle

### 3.2 Yahoo Finance Integration
- [x] YahooFinanceMarketDataProvider erstellen
- [x] Historische OHLCV-Daten von Yahoo Finance laden
- [x] Unterstützung folgender Timeframes:
      - 1m
      - 5m
      - 15m
- [x] Yahoo-Rohdaten in Candle-Modell normalisieren
- [x] Candles chronologisch sortiert zurückgeben
- [x] Keine Realtime-Annahmen

### 3.3 Optionale Mock-Daten (nur für Tests)
- [x] MockMarketDataProvider erstellen
- [x] Nutzung ausschließlich für:
      - lokale Entwicklung
      - Tests

---

## PHASE 4 – Signal Logik (Kern des MVP)

- [x] SignalService erstellen
- [x] VWAP Berechnung implementieren
- [x] Volumen-Durchschnitt berechnen
- [x] Trend-Logik (über / unter VWAP)
- [x] Risk/Reward Logik definieren
- [x] Confidence Score (0–100) berechnen
- [x] Gründe (Reasons) für jedes Signal liefern
- [x] LONG / SHORT / NONE korrekt setzen

---

## PHASE 5 – Signal API

- [x] SignalsController erstellen
- [x] GET /api/signals/{symbol} implementieren
- [x] TradeSignal als JSON zurückgeben
- [x] Fehlerbehandlung (keine Daten, ungültiges Symbol)

---

## PHASE 6 – Aktien Scanner (Daytrading Kandidaten)

- [x] ScannerService erstellen
- [x] Regeln definieren:
  - Volumen über Durchschnitt
  - Hohe Volatilität
  - Preis nahe VWAP
  - News vorhanden
- [x] Score pro Aktie berechnen
- [x] ScannerController erstellen
- [x] GET /api/scanner Endpoint

---

## PHASE 7 – News (MVP einfach)/(Yahoo News)

### 7.1 News Provider Schnittstelle
- [x] INewsProvider Interface definieren
      - Methode: Laden aktienbezogener News
      - Rückgabe: normalisierte NewsItem-Modelle

### 7.2 Yahoo News Integration
- [x] YahooNewsProvider erstellen
- [x] News für ein Aktiensymbol von Yahoo News laden
- [x] Folgende Felder extrahieren:
      - Title
      - Summary
      - PublishedAt
      - Source / URL
- [x] Normalisierung in NewsItem-Modell
- [x] Zeitlich sortierte Rückgabe (neueste zuerst)

### 7.3 Einfaches Sentiment (MVP)
- [x] Einfaches Sentiment bestimmen:
      - positiv
      - neutral
      - negativ
- [x] Sentiment im NewsItem setzen

---

## PHASE 8 – Frontend Grundgerüst

- [x] Verwende Frontend als React App
- [x] Basis Layout (Header / Sidebar / Content)
- [x] API-Konfiguration (Backend URL)
- [x] Fehlerhandling & Loading States

---

## PHASE 9 – Chart & Visualisierung

- [x] TradingView Lightweight Charts in Dashboard in dem Bereich Chart Placeholder integrieren
- [x] Der Link findest du hier: https://github.com/tradingview/lightweight-charts
- [x] Candlestick Chart anzeigen
- [x] Timeframe Wechsel (1m / 5m / 15m)
- [x] Entry Linie darstellen
- [x] Stop-Loss Linie (rot)
- [x] Take-Profit Linie (grün)

---

## PHASE 10 – Scanner UI

- [x] Backend: Watchlist Entity + DbContext (SQLite)
- [x] Backend: Watchlist Controller (GET/POST/DELETE)
- [x] Backend: Scanner DTO erweitert (Trend, VolumeStatus, Confidence, Reasons)
- [x] Frontend: Watchlist API Client
- [x] Frontend: Scanner API Client erweitern
- [x] Frontend: useWatchlist Hook
- [x] Frontend: useScanner Hook
- [x] Frontend: Badge Components (Score, Trend, Volume, etc.)
- [x] Frontend: WatchlistAddForm Component
- [x] Frontend: ScannerTable Component  
- [x] Frontend: ScannerPage erstellen
- [x] Frontend: Navigation/Routing zur ScannerPage
- [x] Frontend: Dashboard unterstützt URL-Parameter (/symbol/:symbol)

---

## PHASE 10.5 – Datenpersistenz (MVP)
 
- [x] Datenbank entscheiden (SQLite für MVP)
- [x] Entity Framework Core einrichten
- [x] ApplicationDbContext erstellen
- [x] DbSets definieren:
  - PaperTrades
  - TradeHistory
- [x] Initiale Migration erstellen (EnsureCreated für MVP)
- [x] Datenbank automatisch beim Start anlegen
- [x] Im Dashboard sollen die Daten zur Watchlist aus der Datenbank geholt werden (WatchlistSymbols)

## PHASE 10.6 – Funktionalität (MVP)
- [x] Vor dem Hinzufügen von einem Symbol muss im backend überprüft werden ob das Symbol gültig ist. Ist sie nicht gültig, soll ein Hinweis angezeigt werden. 
- [x] Implementiere ein News Service im Backend, welches die News von Yahoo News holt.
- [x] Zeige die News im Dashboard im vorgegebenen Bereich

## PHASE 10.7 – Simulation / Replay Mode (Dev only)

Ziel:
Die Anwendung muss unabhängig von Börsenzeiten vollständig nutzbar sein.
Simulation ist ein gleichwertiger Betriebsmodus neben Live.
Business-Logik darf nicht zwischen Live und Replay unterscheiden.


================================================================

### Backend – Modus & Zeitquelle (Pflicht)

- [ ] Enum `MarketMode` definieren
      - Werte: Live, Replay
- [ ] Globale Markt-Zeitquelle festlegen
      - Live: DateTime.UtcNow
      - Replay: ReplayState.CurrentTime
- [ ] Sicherstellen:
      - KEIN direkter Zugriff auf DateTime.Now in Services
      - Zeit wird immer über eine abstrahierte Zeitquelle bezogen

================================================================

### Backend – ReplayState (Single Source of Truth)

- [ ] Modell `ReplayState` erstellen mit:
      - ReplayStartTime (DateTime)
      - CurrentTime (DateTime)  ← einzige gültige Zeit im Replay
      - Speed (double)          ← 1x, 5x, 10x
      - IsRunning (bool)
- [ ] ReplayState zentral im Backend halten (Singleton/Scoped Service)

================================================================

### Backend – ReplayClockService (Zeitsteuerung)

- [ ] Service `ReplayClockService` erstellen
- [ ] Methoden implementieren:
      - Start()
      - Pause()
      - Reset()
      - SetSpeed(double speed)
- [ ] Interner Tick (z. B. 1 Sekunde):
      - CurrentTime += Tick * Speed
- [ ] Service darf NUR ReplayState verändern
- [ ] Keine Marktdaten- oder Business-Logik im ClockService

================================================================

### Backend – Market Data Provider (Replay)

- [ ] `IMarketDataProvider` bleibt unverändert
- [ ] `YahooReplayMarketDataProvider` implementieren
      - Lädt historische Yahoo-Finance-Daten
      - Filtert Candles strikt:
            Candle.Time <= ReplayState.CurrentTime
      - Gibt identisches Datenformat wie Live-Provider zurück
- [ ] Business-Services (Scanner, Signals) dürfen:
      - NICHT wissen, ob Live oder Replay aktiv ist

================================================================

### Backend – Replay API (nur Dev / Simulation)

- [ ] GET  /api/replay/state
      - Liefert aktuellen ReplayState
- [ ] POST /api/replay/start
- [ ] POST /api/replay/pause
- [ ] POST /api/replay/reset
- [ ] POST /api/replay/speed
      - Body: { speed: number }

================================================================

### Frontend – Simulation State (Pflicht)

- [ ] Frontend hält KEINE eigene Zeit
- [ ] ReplayState ausschließlich vom Backend laden
- [ ] Typ `ReplayState` im Frontend definieren:
      - mode: "LIVE" | "REPLAY"
      - currentTime: string
      - speed: number
      - isRunning: boolean

================================================================

### Frontend – Simulation Indicator (immer sichtbar)

- [ ] Globaler Header-Indikator implementieren
      - LIVE → grün
      - SIMULATION → gelb
- [ ] Anzeige:
      - Simulationsdatum
      - aktuelle Replay-Zeit
      - Speed (z. B. 5x)

================================================================

### Frontend – Simulation Control Panel (Dev-only)

- [ ] Toggle: Live ↔ Simulation
- [ ] Replay-Datum auswählen
- [ ] Speed Buttons: 1x / 5x / 10x
- [ ] Controls:
      - Start
      - Pause
      - Reset
- [ ] Controls rufen ausschließlich Replay API auf

================================================================

### Frontend – Verhalten in Simulation

- [ ] Scanner aktualisiert sich anhand Replay-Zeit
- [ ] Charts wachsen Candle-für-Candle
- [ ] Signale erscheinen zeitabhängig
- [ ] Paper-Trading öffnet/schließt Trades anhand Replay-Zeit

================================================================

### Verbindliche Regeln (Copilot-relevant)

- [ ] KEINE Zeitberechnung im Frontend
- [ ] KEINE Business-Logik im Frontend
- [ ] KEINE Realtime-Streams / WebSockets
- [ ] Simulation darf Live-Code NICHT verändern
- [ ] Simulation muss visuell klar erkennbar sein


## PHASE 11 – Paper-Trading (Pflicht)
 
- [ ] PaperTrade Entity erstellen
- [ ] Trades in Datenbank speichern
- [ ] Trade schließen (SL / TP)
- [ ] PnL berechnen
- [ ] Trade Historie aus DB laden
- [ ] Statistik berechnen:
  - Winrate
  - Gesamt-PnL
  - Max Drawdown

---

## PHASE 12 – Qualität & Abschluss

- [ ] Logging hinzufügen
- [ ] Basis Validierungen
- [ ] Code aufräumen & kommentieren
- [ ] MVP Scope überprüfen (keine Extras!)
- [ ] README aktualisieren
