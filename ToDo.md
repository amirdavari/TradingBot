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

- [ ] Scanner Liste anzeigen
- [ ] Sortierung nach Score
- [ ] Klick auf Aktie → Chart + Signal laden
- [ ] Confidence visuell anzeigen

---

## PHASE 10.5 – Datenpersistenz (MVP)
 
- [ ] Datenbank entscheiden (SQLite für MVP)
- [ ] Entity Framework Core einrichten
- [ ] ApplicationDbContext erstellen
- [ ] DbSets definieren:
  - PaperTrades
  - TradeHistory
- [ ] Initiale Migration erstellen
- [ ] Datenbank automatisch beim Start anlegen

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
