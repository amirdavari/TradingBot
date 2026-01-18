# AI Broker – Data Sources (MVP)

Dieses Dokument beschreibt die im MVP verwendeten Datenquellen
sowie deren Einschränkungen.

---

## 1. Marktdaten (Aktienkurse)

Quelle:
- Yahoo Finance

Art der Daten:
- Historische OHLCV-Daten
- Verzögerte Kurse
- Keine Tick-Daten
- Keine garantierte Aktualität

Verwendung im MVP:
- Chart-Darstellung
- Daytrading-Scanner
- Signal-Berechnung
- Backtesting / Paper-Trading

Nicht verwendet:
- Realtime Trading
- Orderbuch-Daten
- Bid/Ask-Spreads

---

## 2. Nachrichten (News)

Quelle:
- Yahoo News (aktienbezogen)

Art der Daten:
- Titel
- Kurze Beschreibung
- Veröffentlichungszeit
- Quelle

Verwendung im MVP:
- Erkennen, ob relevante News vorhanden sind
- Unterstützung oder Abschwächung von Trading-Signalen
- Einfaches Sentiment (positiv / neutral / negativ)

Nicht verwendet:
- Social Media
- Echtzeit-News-Alerts
- KI-basierte Textanalyse

---

## 3. Wichtige Einschränkungen

- Alle Daten dienen ausschließlich Analysezwecken
- Keine Garantie auf Vollständigkeit oder Aktualität
- Keine automatische Handelsausführung
- Kein Einsatz für Echtgeld-Trading

---

## 4. Architektur-Hinweis

Alle Datenquellen sind über Provider abstrahiert.
Ein Austausch der Quelle darf keine Änderungen an
Signal- oder Scanner-Logik erfordern.
