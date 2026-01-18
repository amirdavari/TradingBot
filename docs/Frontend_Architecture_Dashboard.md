# AI Broker – Dashboard Frontend Architecture

## Ziel
Umsetzung des Dashboards gemäß freigegebenem Mockup
in **Struktur, Logik und Interaktion**.

Farben, Typografie und visuelles Design sind frei gestaltbar,
solange die UX-Intention, Anordnung und Funktionalität erhalten bleiben.

Regeln:
- Struktur und Anordnung dürfen NICHT verändert werden
- Keine Modals
- Keine zusätzliche Features
- Trade Setup belegt die komplette rechte Spalte

Frontend wird **zuerst** implementiert.
Backend folgt exakt den Bedürfnissen des Frontends.

---

## Architektur-Prinzipien

- Mockup definiert:
  - Layout
  - Komponenten
  - Interaktionen
- Design (Farben, Fonts, Spacing) ist **nicht bindend**
- Trade-Signale sind **read-only**
- User bestätigt Trades, konfiguriert sie nur im sinne der höhe des Investments
- Keine Modals im Dashboard
- Keine Signal-Overrides

---

## Dashboard Layout (ASCII – funktionale Wahrheit)

```text
┌──────────────┬───────────────────────────┬───────────────────┐
│ Watchlist    │ Chart + News              │ Trade Setup       │
│ (links)      │ (mitte)                   │ (rechts, voll)    │
│              │ ┌─────────────────────┐   │                   │
│ AAPL         │ │Symbol | Timeframe   │   │ Signal (RO)       │
│ TSLA         │ └─────────────────────┘   │ Confidence %      │
│ NVDA         │ Candles + VWAP            │ Reasons           │
│              │ Entry / SL / TP Linien    │ Invest Amount €   │
│              │                           │ BUY / SELL        │
│              │ News unter dem Chart      │                   │
└──────────────┴───────────────────────────┴───────────────────┘

┌──────────────────────────────────────────────────────────────┐
│ Open Paper Trades (global, unten)                            │
│ AAPL LONG +0.8% | TSLA SHORT -0.3%                           │
└──────────────────────────────────────────────────────────────┘


RO = read-only

Komponenten
  WatchlistPanel
  - Teil des Dashboards (nicht Navigation)
  - Liste von Symbolen
  - Status-Icons (Trend / Signalindikator)
  - Klick setzt aktives Symbol

ChartPanel
  - Header mit Symbol-Eingabe und Timeframe-Auswahl
  - Candlestick Chart
  - VWAP (oder weitere Indikatoren)
  - Entry / Stop Loss / Take Profit Linien
  - Linien sind read-only
  - Reagiert auf Symbol, Timeframe, Replay-Time

NewsPanel
  - Unterhalb des Charts
  - Kontextuelle News zum Symbol
  - Snapshot pro Replay-Tag

TradeSetupPanel (rechte Spalte, volle Höhe)
  - Signal Direction (LONG / SHORT / NONE) – read-only
  - Entry / Stop Loss / Take Profit
  - Risk/Reward
  - Confidence (% + visuell)
  - Reasons
  - User Input:
    - Invest Amount (€)
  - Estimated Risk/Reward (€)  
  - Aktionen:
    -BUY (Paper) nur bei LONG
    -SELL (Paper) nur bei SHORT
Keine:
  - Direction-Toggles
  - manuellen SL/TP
  - Modals

OpenPaperTradesPanel
- Zeigt nur offene Trades
- Global (symbol-unabhängig)
- Kompakt
- Klick fokussiert Symbol im Chart