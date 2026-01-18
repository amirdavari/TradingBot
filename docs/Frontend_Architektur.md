# Frontend Architecture – MVP

Dieses Dokument beschreibt die grundlegende Architektur
des Frontends für den AI Broker MVP.

Ziel ist ein klares, wartbares und MVP-fokussiertes Frontend
ohne Business- oder Trading-Logik. Verwender mui-mcp um die Best Practices für Material UI zu finden.

---

## 1. Technologie

- React
- TypeScript
- REST API Kommunikation mit dem Backend
- TradingView Lightweight Charts für Visualisierung
- Material UI

---

## 2. Verantwortung des Frontends

Das Frontend ist zuständig für:
- Darstellung von Marktdaten (Charts)
- Anzeige von Trading-Signalen
- Anzeige von News und Sentiment
- Benutzerinteraktionen (Klicks, Auswahl, Navigation)

Das Frontend ist **nicht zuständig für**:
- Signal-Berechnung
- Trading-Logik
- Datenaggregation
- Risiko- oder Strategieentscheidungen

---

## 3. Architektur-Prinzipien

- Trennung von UI, State und API-Zugriff
- Keine Business-Logik in React Components
- API-Kommunikation ausschließlich über Service-Layer
- Backend ist die einzige Quelle der Wahrheit

---

## 4. Ordnerstruktur (MVP)


src/
├── api/            # API Clients (Signals, Scanner, News)
├── components/     # Wiederverwendbare UI-Komponenten
├── pages/          # Seiten (Dashboard, Scanner, Detail)
├── charts/         # Chart-spezifische Komponenten
├── models/         # DTOs (Candle, TradeSignal, NewsItem)
├── hooks/          # Custom Hooks (z. B. useSignals)
├── styles/         # Styling

---

## 5. Datenfluss
Backend API
   ↓
API Client
   ↓
Hook / State
   ↓
Page
   ↓
UI Components

## 6. Charts

- Candlestick-Daten kommen ausschließlich vom Backend
- Charts zeigen:
  -- Candles
  -- Entry
  -- Stop-Loss
  -- Take-Profit
- Keine Berechnung von Signalen im Frontend
- TradingView Lightweight Charts
- Native JS Integration in React (useRef + useEffect)
- Keine React-Wrapper
- Open Source (Apache 2.0)
- Keine Full TradingView Library

## 7. Pfade und Ordner
 Frontend Pfad: frontend/