# AI Broker – Project Overview (MVP)

## 1. Ziel des Projekts
AI Broker ist eine Web-basierte Anwendung, die für Daytrading geeignete Aktien
identifiziert und Trading-Signale (Entry, Stop-Loss, Take-Profit) auf Basis von
Chartanalyse und Nachrichten vorschlägt.

Das System dient ausschließlich zur Entscheidungsunterstützung und zum
Paper-Trading.

---

## 2. Ziel des MVP
Das MVP soll beweisen, dass:
- Daytrading-Kandidaten sinnvoll gefiltert werden können
- Trading-Signale logisch, nachvollziehbar und visuell darstellbar sind
- die Signal-Performance im Paper-Trading messbar ist

---

## 3. MVP Scope (Enthalten)
✔ Web-Frontend mit Candlestick-Charts  
✔ Aktien-Scanner für Daytrading  
✔ Signal-Berechnung (LONG / SHORT / NONE)  
✔ Anzeige von Entry, Stop-Loss, Take-Profit  
✔ Einfache News-Integration mit Sentiment  
✔ Paper-Trading inkl. Statistik  

---

## 4. Nicht im MVP (Explizit ausgeschlossen)
✘ Echtgeld-Trading  
✘ Broker-Anbindungen  
✘ Automatische Order-Ausführung  
✘ Machine Learning / KI-Modelle  
✘ Mobile Apps (nur Web)  

---

## 5. Technologiestack
Backend:
- C#
- ASP.NET Core Web API (.NET 10)

Frontend:
- Web-App (React)
- TradingView Lightweight Charts

Datenbank:
- SQLite (MVP)

---

## 6. Architekturprinzipien
- Controller enthalten keine Business-Logik
- Alle Trading-Logiken liegen in Services
- Klare Trennung von Daten, Logik und API
- Backend ist vollständig API-basiert
- Frontend ist zustandslos gegenüber Trading-Logik

---

## 7. Grundidee der Signal-Logik (konzeptionell)
Trading-Signale basieren auf:
- Trendanalyse (z. B. VWAP, gleitende Durchschnitte)
- Volumen- und Volatilitätsbewertung
- Unterstützenden oder widersprechenden News
- Risk/Reward-Bewertung

Die konkrete Implementierung ist in separaten Dokumenten definiert.

---

## 8. Zielgruppe
- Entwickler
- technisch versierte Trader
- internes Analyse- und Lernprojekt

---

## 9. Wichtige Leitlinien
- Jede Entscheidung muss erklärbar sein
- Keine „Blackbox“-Signale
- Sicherheit und Nachvollziehbarkeit vor Performance

## 10. Datenquellen (MVP)

Marktdaten (Aktienkurse):
- Quelle: Yahoo Finance
- Art: historische und verzögerte OHLCV-Daten
- Nutzung: Anzeige, Scanner, Signal-Berechnung
- Keine Echtzeit-Daten

Nachrichten:
- Quelle: Yahoo News (aktienbezogen)
- Nutzung: Unterstützung der Signal-Logik
- Vereinfachtes Sentiment (positiv / neutral / negativ)

Die Daten werden ausschließlich zu Analyse- und Paper-Trading-Zwecken genutzt.

## 11. Backend und Frontend Architektur
- Verwende Backend_architektur.md für Backend
- Verwende Frontend_Architektur.md für Frontend

