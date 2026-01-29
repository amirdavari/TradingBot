# Trading Signals & Scanner - Dokumentation

## Übersicht

Diese Dokumentation erklärt die verschiedenen Komponenten des AI Broker Trading Bots, die für die Analyse und Bewertung von Trading-Möglichkeiten zuständig sind.

---

## 1. Trading Signals (Handelssignale)

### Was sind Trading Signals?

Trading Signals sind automatisch generierte Handelsempfehlungen basierend auf technischer Analyse und News-Sentiment. Sie geben konkrete Ein- und Ausstiegspunkte für einen Trade vor.

### Signal-Komponenten

| Feld | Beschreibung | Beispiel |
|------|--------------|----------|
| **Symbol** | Der Aktienticker | `AAPL`, `SAP.DE` |
| **Direction** | Handelsrichtung | `LONG` (kaufen), `SHORT` (verkaufen), `NONE` (kein Signal) |
| **Entry** | Empfohlener Einstiegspreis | `250.45` |
| **Stop Loss** | Ausstiegspreis bei Verlust (Risikobegrenzung) | `247.80` |
| **Take Profit** | Ausstiegspreis bei Gewinn (Gewinnmitnahme) | `255.15` |
| **Confidence** | Vertrauenswert des Signals (0-100%) | `75` |
| **Reasons** | Liste der Gründe für dieses Signal | siehe unten |

### Signal-Logik

#### LONG Signal (Kaufen)
Ein LONG-Signal wird generiert, wenn:
- **Preis über VWAP**: Der aktuelle Preis liegt über dem Volume Weighted Average Price
- **Hohes Volumen**: Das Handelsvolumen ist mindestens 1.2x höher als der Durchschnitt
- **Stop Loss**: Einstiegspreis - (ATR × 1.5)
- **Take Profit**: Einstiegspreis + (ATR × 2.5)

#### SHORT Signal (Verkaufen)
Ein SHORT-Signal wird generiert, wenn:
- **Preis unter VWAP**: Der aktuelle Preis liegt unter dem Volume Weighted Average Price
- **Hohes Volumen**: Das Handelsvolumen ist mindestens 1.2x höher als der Durchschnitt
- **Stop Loss**: Einstiegspreis + (ATR × 1.5)
- **Take Profit**: Einstiegspreis - (ATR × 2.5)

#### NONE (Kein Signal)
Kein Signal wird generiert, wenn:
- Volumen zu niedrig (< 1.2x Durchschnitt)
- Kein klarer Trend erkennbar
- Unzureichende Daten (< 20 Candles)

### Confidence Score (Vertrauenswert)

Der Confidence Score wird berechnet aus:

1. **Basis-Score**: 50 Punkte
2. **Volumen-Bonus**: 
   - +20 Punkte wenn Volumen > 1.5x Durchschnitt
   - +10 Punkte wenn Volumen > 1.2x Durchschnitt
3. **News-Sentiment**: 
   - ±15 Punkte basierend auf News-Stimmung
   - Positiv: +15 Punkte
   - Negativ: -15 Punkte
   - Neutral: 0 Punkte
4. **Risk/Reward Abzug**: 
   - -20 Punkte wenn Risk/Reward < 1.5

**Maximaler Score**: 100 Punkte

### Reasons (Begründungen)

Typische Reasons in einem Signal:

```
✓ Price (250.45) above VWAP (248.20)
✓ Volume 1.45x above average
✓ 📰 Positive news (3+ / 0-)
✓ 📊 News Confidence Impact: +15 points
✓ Risk/Reward: 1:1.67
```

---

## 2. Trade Setup Panel

### Anzeige-Elemente

| Element | Beschreibung | Bedeutung |
|---------|--------------|-----------|
| **Direction Chip** | Farbcodierte Badge | Grün (LONG), Rot (SHORT), Grau (NONE) |
| **Entry Price** | Einstiegspreis | Empfohlener Kaufpreis |
| **Stop Loss** | Stop-Loss-Preis | Automatischer Verkauf bei Erreichen (Verlustbegrenzung) |
| **Take Profit** | Take-Profit-Preis | Automatischer Verkauf bei Erreichen (Gewinnmitnahme) |
| **Risk/Reward** | Risiko-Gewinn-Verhältnis | z.B. 1:1.67 = Für 1€ Risiko, 1.67€ Gewinnchance |
| **Confidence Bar** | Visueller Confidence Score | Farbe: Grün (≥70%), Gelb (≥50%), Rot (<50%) |
| **Reasons List** | Begründungen | Warum dieses Signal generiert wurde |

### Risk Management

| Feld | Beschreibung | Standardwert |
|------|--------------|--------------|
| **Risk per Trade** | Prozent des Kapitals, das riskiert wird | 1% |
| **Position Size** | Anzahl der Aktien | Automatisch berechnet |
| **Invest Amount** | Gesamtinvestitionsbetrag | Automatisch berechnet |
| **Risk Amount** | Maximaler Verlust bei Stop Loss | Automatisch berechnet |
| **Reward Amount** | Potenzieller Gewinn bei Take Profit | Automatisch berechnet |

### Limiting Factors

#### Cash-begrenzte Position
```
💡 Cash-begrenzte Position
Risiko-Budget: 100% genutzt
```
- Das verfügbare Kapital reicht nicht aus für die volle Position
- Position wird auf verfügbares Kapital reduziert

#### Kapital-Limit aktiv
```
📊 Kapital-Limit aktiv
Max. 20% des Kapitals pro Trade
```
- Maximum 20% des Gesamtkapitals pro Trade
- Verhindert Überkonzentration in einem Symbol

---

## 3. Stock Scanner

### Scanner-Spalten

| Spalte | Beschreibung | Werte/Bedeutung |
|--------|--------------|-----------------|
| **Company** | Firmenname | z.B. "SAP", "Siemens" |
| **Ticker** | Börsensymbol | z.B. "SAP.DE", "SIE.DE" |
| **Score** | Daytrading-Score | 0-100 Punkte (höher = besser) |
| **Trend** | Trendrichtung | LONG 📈, SHORT 📉, NONE — |
| **Volume** | Volumen-Status | HIGH 🔥, MEDIUM ⚡, LOW 💧 |
| **News** | News verfügbar | ✓ (Ja) oder ✗ (Nein) |
| **Confidence** | Signal-Vertrauen | 0-100% |
| **Reasons** | Begründungen | Top 3 Gründe für den Score |
| **Watchlist** | Watchlist-Aktion | Button zum Hinzufügen |

### Score Berechnung

Der Scanner-Score (0-100) basiert auf:

#### 1. Volumen-Analyse (0-35 Punkte)
```
- Volumen > 1.5x Durchschnitt: 35 Punkte
- Volumen > 1.2x Durchschnitt: 25 Punkte
- Volumen > 0.8x Durchschnitt: 15 Punkte
- Volumen < 0.8x Durchschnitt: 5 Punkte
```

#### 2. Volatilität (0-25 Punkte)
```
- Volatilität 1-3%: 25 Punkte (ideal)
- Volatilität 0.5-1%: 20 Punkte
- Volatilität 3-5%: 15 Punkte
- Volatilität > 5% oder < 0.5%: 5 Punkte
```

#### 3. VWAP-Distanz (0-25 Punkte)
```
- Distanz 0-2%: 25 Punkte (nahe VWAP)
- Distanz 2-5%: 15 Punkte
- Distanz > 5%: 5 Punkte
```

#### 4. News-Bonus (0-15 Punkte)
```
- News vorhanden: +15 Punkte
- Keine News: 0 Punkte
```

**Gesamt-Score**: Summe aller Komponenten (max. 100)

### Score-Kategorien

| Score | Bewertung | Empfehlung |
|-------|-----------|------------|
| **70-100** | 🟢 Exzellent | Sehr gute Trading-Gelegenheit |
| **50-69** | 🟡 Gut | Trading möglich, höheres Risiko |
| **30-49** | 🟠 Durchschnitt | Vorsicht, nur für erfahrene Trader |
| **0-29** | 🔴 Schwach | Nicht empfohlen |

### Volume Status

| Status | Badge | Kriterium |
|--------|-------|-----------|
| **HIGH** | 🔥 Rot | Volumen > 1.5x Durchschnitt |
| **MEDIUM** | ⚡ Gelb | Volumen 0.8-1.5x Durchschnitt |
| **LOW** | 💧 Blau | Volumen < 0.8x Durchschnitt |

### Trend Badges

| Trend | Badge | Bedeutung |
|-------|-------|-----------|
| **LONG** | 📈 Grün | Aufwärtstrend, Kaufsignal |
| **SHORT** | 📉 Rot | Abwärtstrend, Verkaufssignal |
| **NONE** | — Grau | Kein klarer Trend |

### Confidence Badge

| Confidence | Farbe | Bedeutung |
|------------|-------|-----------|
| **70-100%** | 🟢 Grün | Hohes Vertrauen |
| **50-69%** | 🟡 Gelb | Mittleres Vertrauen |
| **0-49%** | 🔴 Rot | Niedriges Vertrauen |

---

## 4. Technische Indikatoren (Hintergrund)

### VWAP (Volume Weighted Average Price)
- **Berechnung**: Durchschnittspreis gewichtet nach Volumen
- **Bedeutung**: Zeigt den "fairen" Preis basierend auf Handelsaktivität
- **Trading**: 
  - Preis über VWAP = Bullish (Kaufdruck)
  - Preis unter VWAP = Bearish (Verkaufsdruck)

### ATR (Average True Range)
- **Berechnung**: Durchschnittliche Handelsspanne der letzten 14 Perioden
- **Bedeutung**: Misst Volatilität
- **Verwendung**: 
  - Stop Loss = Entry ± (ATR × 1.5)
  - Take Profit = Entry ± (ATR × 2.5)

### Volumen-Ratio
- **Berechnung**: Aktuelles Volumen / Durchschnittsvolumen
- **Bedeutung**: Zeigt Handelsaktivität relativ zum Durchschnitt
- **Schwellwerte**:
  - > 1.5x = Sehr hohes Interesse
  - > 1.2x = Ausreichend für Signal
  - < 0.8x = Geringes Interesse

### Volatilität
- **Berechnung**: (ATR / Aktueller Preis) × 100
- **Bedeutung**: Prozentuale Preisbewegung
- **Ideal**: 1-3% für Daytrading
- **Zu hoch**: > 5% = Hohes Risiko
- **Zu niedrig**: < 0.5% = Wenig Gewinnpotential

---

## 5. Workflow & Best Practices

### Schritt 1: Scanner nutzen
1. Öffne die **Scanner Page**
2. Prüfe die **Score-Spalte** (suche nach Scores ≥ 70)
3. Achte auf **HIGH Volume** 🔥
4. Prüfe **Trend** und **Confidence**
5. Klicke auf **"Add"** um interessante Symbole zur Watchlist hinzuzufügen

### Schritt 2: Dashboard analysieren
1. Öffne das **Dashboard**
2. Wähle ein Symbol aus der **Watchlist** (links)
3. Analysiere den **Chart** (Mitte)
4. Prüfe **News** (unter dem Chart)
5. Überprüfe das **Trade Setup** (rechts)

### Schritt 3: Trade eröffnen
1. Prüfe **Direction**: LONG oder SHORT?
2. Überprüfe **Confidence**: Mindestens 50%?
3. Prüfe **Risk/Reward**: Mindestens 1:1.5?
4. Stelle **Risk per Trade** ein (Standard: 1%)
5. Prüfe **Position Size** und **Invest Amount**
6. Klicke auf **"Open LONG/SHORT Trade"**

### Schritt 4: Trade überwachen
1. Offene Trades erscheinen im **Open Trades Panel** (unten)
2. Trades werden automatisch geschlossen bei:
   - Stop Loss erreicht (Verlustbegrenzung)
   - Take Profit erreicht (Gewinnmitnahme)
3. Manuelles Schließen möglich über **"Close Trade"** Button

---

## 6. Häufig gestellte Fragen (FAQ)

### Warum bekomme ich kein Signal?
- **Volumen zu niedrig**: < 1.2x Durchschnitt
- **Keine klare Trendrichtung**
- **Unzureichende Daten**: < 20 Candles verfügbar

### Was bedeutet "Risk/Reward 1:1.67"?
- Für jeden 1€ den du riskierst, kannst du 1.67€ gewinnen
- Minimum sollte 1:1.5 sein
- Je höher, desto besser das Chance-Risiko-Verhältnis

### Wann sollte ich einen Trade NICHT eröffnen?
- ❌ Confidence < 50%
- ❌ Risk/Reward < 1.5
- ❌ Score < 50 im Scanner
- ❌ Unklare Nachrichtenlage (prüfe News Panel)

### Wie viel Kapital sollte ich pro Trade einsetzen?
- **Konservativ**: 0.5-1% Risiko pro Trade
- **Moderat**: 1-2% Risiko pro Trade
- **Aggressiv**: 2-3% Risiko pro Trade (Nicht empfohlen)
- **Niemals**: > 5% Risiko pro Trade

### Warum kann ich ein Symbol nicht aus der Watchlist entfernen?
- Symbol hat **offene Trades**
- Schließe zuerst alle Trades für dieses Symbol
- Dann kannst du es entfernen

---

## 7. Glossar

| Begriff | Bedeutung |
|---------|-----------|
| **VWAP** | Volume Weighted Average Price - Volumengewichteter Durchschnittspreis |
| **ATR** | Average True Range - Durchschnittliche Handelsspanne |
| **Stop Loss** | Automatischer Verkauf bei Erreichen eines Verlustlimits |
| **Take Profit** | Automatischer Verkauf bei Erreichen eines Gewinnziels |
| **LONG** | Kaufposition (profitiert von steigenden Kursen) |
| **SHORT** | Verkaufsposition (profitiert von fallenden Kursen) |
| **Confidence** | Vertrauenswert des Systems in das Signal (0-100%) |
| **Risk/Reward** | Verhältnis von Risiko zu Gewinnchance |
| **Position Size** | Anzahl der zu handelnden Aktien |
| **Paper Trading** | Simuliertes Trading ohne echtes Geld |

---

## 8. Mock Data Providers (Entwicklungs- & Testmodus)

Die Anwendung verwendet verschiedene **Mock Data Providers** für lokale Entwicklung und Tests, wenn kein Zugriff auf Yahoo Finance besteht.

### 8.1 MockMarketDataProvider

Generiert synthetische Kursdaten für Tests.

**Konfiguration & Verhalten:**

| Parameter | Wert | Beschreibung |
|-----------|------|--------------|
| **Base Volatility** | 2% | Standard-Volatilität für Preisbewegungen |
| **Price Drift** | ±$1 | Zufällige Preisänderung zwischen Candles |

**Symbol-spezifische Basispreise:**

| Symbol | Basis-Preis | Beschreibung |
|--------|-------------|--------------|
| `AAPL` | $180.00 | Apple Inc. |
| `MSFT` | $370.00 | Microsoft Corp. |
| `TSLA` | $250.00 | Tesla Inc. |
| `GOOGL` | $140.00 | Alphabet Inc. |
| `AMZN` | $155.00 | Amazon.com Inc. |
| `NVDA` | $480.00 | NVIDIA Corp. |
| `META` | $340.00 | Meta Platforms |
| `AMD` | $140.00 | Advanced Micro Devices |
| `NFLX` | $480.00 | Netflix Inc. |
| `SPY` | $470.00 | S&P 500 ETF |
| *Andere* | $100.00 | Standard für unbekannte Symbole |

**Candle-Generierung nach Period:**

| Period | Candles berechnet | Logik |
|--------|-------------------|-------|
| `1d` | 390 / timeframe | ~6.5 Stunden Handelstag |
| `5d` | 1950 / timeframe | 5 Handelstage |
| `1mo` | 8190 / timeframe | ~21 Handelstage |
| *Default* | 100 | Fallback |

**Volume-Generierung nach Timeframe:**

| Timeframe | Basis-Volumen | Beschreibung |
|-----------|---------------|--------------|
| 1 Minute | 50,000 | Niedriges Volumen für kurze Intervalle |
| 5 Minuten | 200,000 | Standard-Volumen |
| 15 Minuten | 500,000 | Höheres Volumen |
| *Default* | 100,000 | Fallback |

**OHLC-Berechnung:**
```
Open = basePrice
Close = basePrice + (random(-0.5 bis +0.5) × priceMove)
High = max(Open, Close) + (random × priceMove)
Low = min(Open, Close) - (random × priceMove)
Volume = baseVolume × (0.5 + random)
```

---

### 8.2 MockNewsProvider

Generiert synthetische Nachrichten für Tests und Replay-Modus.

**News-Templates (10 Stück):**

| # | Template | Sentiment |
|---|----------|-----------|
| 1 | "{Symbol} Reports Strong Q4 Earnings" | **positive** |
| 2 | "{Symbol} Announces New Product Launch" | **positive** |
| 3 | "{Symbol} Shares Rise on Market Optimism" | **positive** |
| 4 | "Analysts Upgrade {Symbol} Price Target" | **positive** |
| 5 | "{Symbol} CEO Discusses Growth Strategy" | **neutral** |
| 6 | "{Symbol} Trading Volume Increases" | **neutral** |
| 7 | "Market Watch: {Symbol} in Focus" | **neutral** |
| 8 | "{Symbol} Faces Regulatory Scrutiny" | **negative** |
| 9 | "{Symbol} Reports Supply Chain Issues" | **negative** |
| 10 | "Concerns Grow Over {Symbol} Market Position" | **negative** |

**Sentiment-Verteilung:**
- **4 Positive** (40%)
- **3 Neutral** (30%)
- **3 Negative** (30%)

**Zeitliche Verteilung:**
- Start: 7 Tage vor aktuellem Zeitpunkt
- Intervall: Alle 12 Stunden eine News
- Sortierung: Neueste zuerst

**Auswirkung auf Confidence Score:**

Die News-Sentiment-Analyse berechnet einen Score von -1 bis +1:

```
sentimentScore = (positiveCount - negativeCount) / totalNewsCount
confidenceImpact = sentimentScore × 15 Punkte
```

| Sentiment-Score | Confidence Änderung |
|-----------------|---------------------|
| +1.0 (sehr positiv) | +15 Punkte |
| +0.5 (positiv) | +7-8 Punkte |
| 0 (neutral) | 0 Punkte |
| -0.5 (negativ) | -7-8 Punkte |
| -1.0 (sehr negativ) | -15 Punkte |

---

### 8.3 Wann werden Mock Provider verwendet?

Die Provider-Auswahl erfolgt in `Program.cs` basierend auf Umgebungsvariablen und Modus:

| Szenario | Market Data Provider | News Provider |
|----------|---------------------|---------------|
| **Live Mode + Yahoo** | `YahooFinanceMarketDataProvider` | `YahooNewsProvider` |
| **Live Mode + Mock** | `MockMarketDataProvider` | `MockNewsProvider` |
| **Replay Mode** | `YahooReplayMarketDataProvider` | `YahooReplayNewsProvider` oder `ReplayMockNewsProvider` |
| **Tests** | `MockMarketDataProvider` | `MockNewsProvider` |

**Umstellung auf Mock:**
- Setze Umgebungsvariable `USE_MOCK_DATA=true`
- Oder ändere Provider-Registrierung in `Program.cs`

---

## 9. Detaillierte Berechnungslogik

### 9.1 VWAP (Volume Weighted Average Price)

**Formel:**
```
VWAP = Σ(Typical Price × Volume) / Σ(Volume)

wobei: Typical Price = (High + Low + Close) / 3
```

**Interpretation:**
- Preis > VWAP → Bullish (Käufer dominieren)
- Preis < VWAP → Bearish (Verkäufer dominieren)
- Wird für alle Candles im Zeitraum berechnet

### 9.2 ATR (Average True Range)

**Berechnung der True Range (TR):**
```
TR = max(
    High - Low,
    |High - Previous Close|,
    |Low - Previous Close|
)
```

**ATR (14 Perioden):**
```
ATR = Durchschnitt der letzten 14 TR-Werte
```

**Verwendung für Levels:**
```
LONG:
  Stop Loss = Entry - (ATR × 1.5)
  Take Profit = Entry + (ATR × 2.5)

SHORT:
  Stop Loss = Entry + (ATR × 1.5)
  Take Profit = Entry - (ATR × 2.5)
```

### 9.3 Vollständige Confidence-Berechnung

```
Basis-Score: 50 Punkte

+ Trend-Bestätigung (Preis auf richtiger Seite von VWAP): +20 Punkte

+ Volumen-Bonus:
  - Volume Ratio > 1.5: +15 Punkte
  - Volume Ratio > 1.2: +10 Punkte
  - Volume Ratio ≤ 1.2: +0 Punkte

+ Volatilitäts-Anpassung:
  - Volatility < 1.5%: +10 Punkte (guter Entry)
  - Volatility > 3.0%: -10 Punkte (zu riskant)

+ News-Sentiment: -15 bis +15 Punkte

- Risk/Reward-Abzug:
  - R/R < 1.5: -20 Punkte

= Finaler Confidence Score (geclampt auf 0-100)
```

### 9.4 Scanner Score Berechnung (100 Punkte max)

**1. Volumen-Score (max. 35 Punkte):**
| Volume Ratio | Punkte |
|--------------|--------|
| > 2.0x | 35 |
| > 1.5x | 30 |
| > 1.2x | 25 |
| > 0.8x | 15 |
| ≤ 0.8x | 5 |

**2. Volatilitäts-Score (max. 30 Punkte):**
| Volatilität | Punkte | Bewertung |
|-------------|--------|-----------|
| 1.0% - 3.0% | 30 | Ideal |
| 0.5% - 1.0% | 25 | Gut |
| 0.3% - 5.0% | 20 | Akzeptabel |
| > 0.1% | 10 | Niedrig |
| ≤ 0.1% | 5 | Sehr niedrig |

**3. VWAP-Distanz-Score (max. 25 Punkte):**
| Distanz zum VWAP | Punkte |
|------------------|--------|
| ≤ 1.0% | 25 |
| ≤ 2.0% | 20 |
| ≤ 3.0% | 15 |
| ≤ 5.0% | 10 |
| > 5.0% | 5 |

**4. News-Bonus (max. 10 Punkte):**
| Hat News? | Punkte |
|-----------|--------|
| Ja | 10 |
| Nein | 0 |

---

## 10. Support & Weitere Informationen

Für weitere technische Details siehe:
- [Backend Architektur](Backend_architektur.md)
- [Frontend Architektur](Frontend_Architektur.md)
- [Projekt Übersicht](Project_Overview.md)

---

**Hinweis**: Dies ist eine Paper-Trading-Plattform. Alle Trades sind simuliert und nutzen kein echtes Geld.
