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

- [x] Enum `MarketMode` definieren
      - Werte: Live, Replay
- [x] Globale Markt-Zeitquelle festlegen
      - Live: DateTime.UtcNow
      - Replay: ReplayState.CurrentTime
- [x] Sicherstellen:
      - KEIN direkter Zugriff auf DateTime.Now in Services
      - Zeit wird immer über eine abstrahierte Zeitquelle bezogen

================================================================

### Backend – ReplayState (Single Source of Truth)

- [x] Modell `ReplayState` erstellen mit:
      - ReplayStartTime (DateTime)
      - CurrentTime (DateTime)  ← einzige gültige Zeit im Replay
      - Speed (double)          ← 1x, 5x, 10x
      - IsRunning (bool)
- [x] ReplayState zentral im Backend halten (Singleton/Scoped Service)

================================================================

### Backend – ReplayClockService (Zeitsteuerung)

- [x] Service `ReplayClockService` erstellen
- [x] Methoden implementieren:
      - Start()
      - Pause()
      - Reset()
      - SetSpeed(double speed)
- [x] Interner Tick (z. B. 1 Sekunde):
      - CurrentTime += Tick * Speed
- [x] Service darf NUR ReplayState verändern
- [x] Keine Marktdaten- oder Business-Logik im ClockService

================================================================

### Backend – Market Data Provider (Replay)

- [x] `IMarketDataProvider` bleibt unverändert
- [x] `YahooReplayMarketDataProvider` implementieren
      - Lädt historische Yahoo-Finance-Daten
      - Filtert Candles strikt:
            Candle.Time <= ReplayState.CurrentTime
      - Gibt identisches Datenformat wie Live-Provider zurück
- [x] Business-Services (Scanner, Signals) dürfen:
      - NICHT wissen, ob Live oder Replay aktiv ist

================================================================

### Backend – Replay API (nur Dev / Simulation)

- [x] GET  /api/replay/state
      - Liefert aktuellen ReplayState
- [x] POST /api/replay/start
- [x] POST /api/replay/pause
- [x] POST /api/replay/reset
- [x] POST /api/replay/speed
      - Body: { speed: number }

================================================================

### Frontend – Simulation State (Pflicht)

- [x] Frontend hält KEINE eigene Zeit
- [x] ReplayState ausschließlich vom Backend laden
- [x] Typ `ReplayState` im Frontend definieren:
      - mode: "LIVE" | "REPLAY"
      - currentTime: string
      - speed: number
      - isRunning: boolean

================================================================

### Frontend – Simulation Indicator (immer sichtbar)

- [x] Globaler Header-Indikator implementieren
      - LIVE → grün
      - SIMULATION → gelb
- [x] Anzeige:
      - Simulationsdatum
      - aktuelle Replay-Zeit
      - Speed (z. B. 5x)

================================================================

### Frontend – Simulation Control Panel (Dev-only)

- [x] Toggle: Live ↔ Simulation
- [x] Replay-Datum auswählen
- [x] Speed Buttons: 1x / 5x / 10x
- [x] Controls:
      - Start
      - Pause
      - Reset
- [x] Controls rufen ausschließlich Replay API auf

================================================================

### Frontend – Verhalten in Simulation

- [x] Scanner aktualisiert sich anhand Replay-Zeit
- [x] Charts wachsen Candle-für-Candle
- [x] Signale erscheinen zeitabhängig


================================================================

### Verbindliche Regeln (Copilot-relevant)

- [x] KEINE Zeitberechnung im Frontend
- [x] KEINE Business-Logik im Frontend
- [x] KEINE Realtime-Streams / WebSockets
- [x] Simulation darf Live-Code NICHT verändern
- [x] Simulation muss visuell klar erkennbar sein

================================================================

## PHASE 10.8 – News Simulation (Replay)

### Backend – News Simulation (Replay)

- [x] `INewsProvider` Interface unverändert lassen
- [x] `YahooReplayNewsProvider` implementieren
      - Lädt historische Yahoo-News
      - Filtert News strikt:
            News.PublishedAt <= ReplayState.CurrentTime
      - Gibt identisches NewsItem-Format wie Live-Provider zurück
- [x] News-Daten werden:
      - NICHT zufällig generiert
      - NICHT neu berechnet
      - NUR zeitlich gefiltert

================================================================
### Backend – News & Replay-Zeit (Pflicht)

- [x] Sicherstellen:
      - News-Zeit basiert ausschließlich auf PublishedAt
      - KEIN DateTime.Now / DateTime.UtcNow im Replay
- [x] Scanner & Signal-Logik erhalten News:
      - nur wenn sie zum Replay-Zeitpunkt bereits veröffentlicht sind

================================================================
### Backend – Replay API (News relevant)

- [x] News-Endpunkte berücksichtigen ReplayState automatisch
      - KEIN separater News-Replay-Endpunkt
      - Mode-Logik entscheidet Provider-Auswahl

================================================================
### Frontend – News Verhalten in Simulation

- [x] News erscheinen erst ab ihrem PublishedAt-Zeitpunkt
- [x] News verschwinden NICHT rückwirkend
- [x] News-Listen werden bei Replay-Ticks aktualisiert
- [x] UI zeigt klar:
      - News ist Teil der Simulation (kein Live-Feed)

================================================================
### Verbindliche Regeln – News Simulation

- [x] KEINE Mock-News im Simulation Mode
- [x] KEINE zufälligen Sentiments
- [x] KEINE künstlichen News-Events
- [x] News-Sentiment basiert auf gespeicherten News-Daten
- [x] News-Logik unterscheidet NICHT zwischen Live und Replay


## PHASE 11 – Paper-Trading & Depot (Pflicht)

### 11.1 Depot / Account (neu)
- [x] Account / Depot Entity erstellen
  - Balance (Cash)
  - Equity
  - Available Cash
  - Initial Balance (z. B. 10.000 €)
- [x] Account initialisieren beim Start der Simulation
- [x] Account-Daten persistent speichern
- [x] Separate Depot-Seite im Frontend:
  - Kontostand
  - Freies Kapital
  - Gebundenes Kapital
  - Equity Verlauf (optional)

---

### 11.2 Risk & Position Management (neu)
- [x] RiskManagementService erstellen
- [x] Risiko pro Trade definieren (z. B. 1 % vom Account)
- [x] Positionsgröße berechnen basierend auf:
  - Account Balance
  - Risiko %
  - Entry / Stop Loss Abstand
- [x] Invest Amount dynamisch berechnen
- [x] Invest Amount read-only an Frontend liefern
- [x] Trade nur erlauben, wenn:
  - ausreichend Available Cash vorhanden
  - Risiko-Regeln erfüllt sind

---

### 11.3 PaperTrade Lifecycle
- [x] PaperTrade Entity erweitern
  - Symbol
  - Direction
  - Entry / StopLoss / TakeProfit
  - PositionSize (decimal 18,4)
  - InvestAmount (decimal 18,2)
  - Status (OPEN / CLOSED_SL / CLOSED_TP / CLOSED_MANUAL)
  - OpenedAt / ClosedAt
  - PnL / PnLPercent
- [x] PaperTradeService erstellen:
  - OpenTradeAsync() - Öffnet Trade mit Risk-Validierung
  - CloseTradeAsync() - Schließt Trade und berechnet PnL
  - CheckOpenTradesAsync() - Überwacht alle offenen Trades
  - CalculateUnrealizedPnLAsync() - Real-Time PnL für offene Trades
- [x] PaperTradeMonitorService erstellen:
  - Background Service (alle 5 Sekunden)
  - Automatische Stop Loss Ausführung
  - Automatische Take Profit Ausführung
- [x] PaperTradesController erweitern:
  - GET /api/papertrades/open
  - GET /api/papertrades/history
  - POST /api/papertrades/auto-execute
  - POST /api/papertrades/{id}/close
  - GET /api/papertrades/{id}/unrealized-pnl
- [x] Trade automatisch eröffnen:
  - Signal-Validierung über SignalService
  - Risk-Check über RiskManagementService
  - Capital-Allocation über AccountService
- [x] Trade automatisch schließen:
  - Stop Loss Detection (LONG: price ≤ SL, SHORT: price ≥ SL)
  - Take Profit Detection (LONG: price ≥ TP, SHORT: price ≤ TP)
  - Status entsprechend setzen (CLOSED_SL / CLOSED_TP)
- [x] PnL berechnen (realisiert & unrealisiert)
- [x] Account Balance beim Schließen aktualisieren:
  - Capital freigeben
  - PnL auf Balance anwenden

---

### 11.4 Open Trades (Dashboard)
- [x] PaperTrade Interface im Frontend definieren
- [x] API Client für GET /api/papertrades/open erweitern
- [x] useOpenTrades Hook erstellen (Auto-Refresh alle 5 Sekunden)
- [x] OpenTradesPanel Komponente erstellen mit:
  - Tabelle mit allen offenen Trades
  - Symbol, Richtung (LONG/SHORT), Entry, Stop Loss, Take Profit
  - Position Size, Invest Amount
  - Unrealized PnL (Betrag + Prozent)
  - Eröffnungszeitpunkt
  - Click-Handler zum Fokussieren des Charts
- [x] Dashboard Integration (ersetzt alte OpenPaperTradesPanel)
- [x] Unrealized PnL Anzeige (basierend auf Backend-Berechnung)
- [x] Klick auf Trade wechselt zum entsprechenden Symbol im Dashboard

---

### 11.5 Trade Historie & Statistik
- [ ] Geschlossene Trades aus DB laden
- [ ] Historie global anzeigen (separate Seite)
- [ ] Statistik berechnen:
  - Winrate
  - Gesamt-PnL
  - Average R
  - Max Drawdown
- [ ] Statistik & Historie im Frontend darstellen

---

### 11.6 API & Frontend-Integration
- [ ] GET /api/account
- [ ] GET /api/papertrades/open
- [ ] GET /api/papertrades/history
- [ ] POST /api/papertrades/auto-execute
- [ ] Trade Setup im Frontend zeigt:
  - berechnetes Invest Amount
  - berechnetes Risiko
  - read-only Parameter

---

## PHASE 12 – Qualität & Abschluss

- [ ] Logging hinzufügen
- [ ] Basis Validierungen
- [ ] Code aufräumen & kommentieren

