# AI Broker – Backend Architecture (MVP)

Dieses Dokument beschreibt die Backend-Architektur des AI Broker MVP.
Es definiert Verantwortlichkeiten, Struktur und Regeln für die Implementierung
in C# mit ASP.NET Core Web API. Benutze Microdoft MCP als Hilfsmittel falls vorhanden ist.

---

## 1. Ziel des Backends

Das Backend stellt:
- Marktdaten (Candles)
- News-Daten (MVP einfach)
- Daytrading-Scanner
- Trading-Signale (Entry / SL / TP)
- Paper-Trading-Funktionalität

bereit und liefert diese ausschließlich über eine REST API an das Frontend.

---

## 2. Technologie

- Sprache: C#
- Framework: ASP.NET Core Web API (.NET 8)
- Datenbank (MVP): SQLite
- ORM: Entity Framework Core
- Architektur: Service-orientiert (kein Monolith, keine Microservices)

---

## 3. Ordnerstruktur (verbindlich)

Backend/
│
├── Controllers/
│ └── API-Endpunkte (HTTP)
│
├── Services/
│ └── Business-Logik (Signale, Scanner, Trading)
│
├── Models/
│ └── Reine Datenmodelle (POCOs)
│
├── Data/
│ └── Datenzugriff, Mock-Daten, später DB
│
└── Program.cs


---

## 4. Verantwortlichkeiten

### 4.1 Controllers

Controllers sind ausschließlich für:
- HTTP Requests / Responses
- Routing
- Statuscodes
- Model Binding

❌ KEINE Business-Logik  
❌ KEINE Berechnungen  
❌ KEIN Datenzugriff  

Beispiel:
- `SignalsController`
- `ScannerController`
- `NewsController`
- `PaperTradingController`

---

### 4.2 Services

Services enthalten die **gesamte Business-Logik**.

Typische Aufgaben:
- Berechnung von Trading-Signalen
- Scanner-Regeln
- Risk/Reward-Logik
- Confidence-Berechnung
- Paper-Trading-Simulation

✔ Services dürfen andere Services nutzen  
✔ Services dürfen Daten aus `Data` beziehen  
❌ Services dürfen keine HTTP-Konzepte kennen  

Beispiele:
- `SignalService`
- `ScannerService`
- `NewsService`
- `PaperTradingService`

---

### 4.3 Models

Models sind:
- einfache Datenklassen (POCOs)
- ohne Logik
- ohne Abhängigkeiten

❌ Keine Methoden mit Business-Logik  
❌ Keine Services injizieren  

Beispiele:
- `Candle`
- `TradeSignal`
- `NewsItem`
- `PaperTrade`

---

### 4.4 Data

Der Data-Bereich ist verantwortlich für:
- Bereitstellung von Marktdaten
- Datenbankzugriffe (später EF Core)
❌ Keine Business-Logik  
❌ Keine Berechnungen  

#### 4.5 ## Data Provider (MVP)
Das Backend nutzt externe Datenquellen ausschließlich lesend.
Marktdaten:
- Yahoo Finance
- Zugriff über einen MarketDataProvider
- Keine Abhängigkeit von Echtzeit-Feeds
News:
- Yahoo News
- Zugriff über einen NewsProvider
- Fokus auf Titel, Kurzbeschreibung und Zeitstempel

Alle externen Datenquellen sind über Provider abstrahiert,
um sie später austauschen zu können.

---

## 5. Dependency Injection (DI)

- Alle Services werden über DI registriert
- Controller erhalten Services per Konstruktor
- Keine `new`-Instanzen in Controllern

Beispiel:
```csharp
builder.Services.AddScoped<SignalService>();

6. API-Prinzipien
- REST-konform
- JSON als Austauschformat
- Klare, sprechende Endpunkte
- Versionierung optional (v1)

Beispiele:
- GET /api/signals/{symbol}
- GET /api/scanner
- GET /api/news/{symbol}
- POST /api/papertrades

7. Fehlerbehandlung
- Keine Exceptions bis ins Frontend durchreichen
- Saubere HTTP Statuscodes
- Verständliche Fehlermeldungen

Beispiele:
- 400 → ungültige Anfrage
- 404 → Symbol nicht gefunden
- 500 → interner Fehler

8. Asynchronität
- Alle API-Endpunkte sind async
- Services arbeiten async, wo sinnvoll
- Keine blockierenden Aufrufe

9. Logging
- Basis-Logging im Backend
- Fokus auf:
  -- Fehler
  -- Signal-Berechnung
  -- Trade-Eröffnungen / Schließungen

- ❌ Kein Performance-Tuning im MVP

10. Architektur-Grenzen (WICHTIG)
Im MVP NICHT erlaubt:
- Machine Learning
- Echtzeit-Broker-Anbindungen
- Auto-Trading
- Microservices
- Event Streaming
- Externe Message Queues

11. Erweiterbarkeit (nach MVP)
- Nach dem MVP können ergänzt werden:
- echte Marktdaten-APIs
- persistente Datenbank
- Strategien als Module
- ML-Modelle (optional)
- Broker-Anbindung (manuell)
- Diese Erweiterungen dürfen die bestehende Architektur nicht brechen.

12. Leitlinien für GitHub Copilot
-Beim Implementieren soll Copilot:
  -- die Ordnerstruktur respektieren
  -- keine Logik in Controller schreiben
  -- keine Features außerhalb des MVP-Scope erzeugen
  -- sich an PROJECT_OVERVIEW.md halten

## 6. Pfade und Ordner
 Backend Pfad: API/
