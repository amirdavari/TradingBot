# DB_MIGRATION_STRATEGY.md
AI Broker – Database Migration Strategy

## Ziel
Diese Strategie stellt sicher, dass sich das Datenbankschema
inkrementell weiterentwickeln kann, **ohne bestehende Daten zu löschen**.

Die Datenbank wird **erweitert, nicht ersetzt**.
Alle historischen Trades, Kontostände und Statistiken bleiben erhalten.

---

## Grundprinzipien (verbindlich)

- ❌ Keine DROP TABLE in produktiven Migrationen
- ❌ Keine DB-Resets nach MVP
- ✅ Schema-Evolution statt Schema-Replacement
- ✅ Versionierte, einmalige Migrationen
- ✅ Abwärtskompatible Änderungen
- ✅ Daten sind ein Vermögenswert

---

## 1. Migrationsmodell

### 1.1 Versionierte Migrationen
Jede Schema-Änderung wird als **separate Migration** abgelegt.

Beispiel:
/db/migrations/
001_create_accounts.sql
002_create_paper_trades.sql
003_add_risk_fields.sql
004_add_capital_allocation.sql
005_add_trade_statistics.sql


**Regeln:**
- Migrationen sind **append-only**
- Bereits ausgeführte Migrationen werden **nie geändert**
- Neue Änderungen = neue Migration

---

### 1.2 Migration Tracking
Die DB enthält eine eigene Tabelle zur Nachverfolgung:

```sql
CREATE TABLE schema_migrations (
  version VARCHAR(50) PRIMARY KEY,
  applied_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);
```

Eine Migration wird nur ausgeführt, wenn sie noch nicht in dieser Tabelle steht.

## 2. Erweiterungsstrategie (wichtig)

### 2.1 Tabellen erweitern, nicht löschen
    ❌ Falsch:
        DROP TABLE paper_trades;
        CREATE TABLE paper_trades (...);
    ✅ Richtig:
        ALTER TABLE paper_trades
        ADD COLUMN capital_limit_percent DECIMAL(5,2) NULL;

### 2.2 Neue Features = neue Spalten oder Tabellen
Feature	                    Strategie
Depot / Account	            Neue accounts Tabelle
Risk Management	            Neue Spalten
Capital Allocation	        Neue Spalten
Trade Historie	            Separate Tabelle
Statistiken	                Views oder Aggregationstabellen

### 3. Abwärtskompatibilität (sehr wichtig)
### 3.1 Defaults setzen
    Neue Spalten erhalten immer sinnvolle Defaults:

    ALTER TABLE accounts
    ADD COLUMN max_capital_percent DECIMAL(5,2) DEFAULT 100;
    ➡ Alte Accounts verhalten sich wie vorher
    ➡ Neue Accounts nutzen die neue Logik

    ### 3.2 Nullable zuerst
    Neue Pflichtfelder werden zuerst nullable eingeführt:

    ALTER TABLE paper_trades
    ADD COLUMN risk_model_version INT NULL;
    Später:

    UPDATE paper_trades
    SET risk_model_version = 2
    WHERE risk_model_version IS NULL;
    
### 4. Feature-Versionierung auf Datenebene
#### 4.1 Versionierte Logik
Neue Logik wird versioniert, nicht erzwungen.

Beispiel:

ALTER TABLE accounts
ADD COLUMN risk_model_version INT DEFAULT 1;
Backend:

switch (account.riskModelVersion) {
  case 1:
    applyLegacyRiskModel();
    break;
  case 2:
    applyRiskAndCapitalModel();
    break;
}
➡ Alte Daten bleiben gültig
➡ Neue Features können schrittweise aktiviert werden

### 5. Daten-Migration vs. Schema-Migration
#### 5.1 Schema-Migration
-Tabellen
-Spalten
-Indizes
-Constraints

### 5.2 Daten-Migration (optional)
- Backfills
- Default-Werte
- Re-Kalkulationen

Beispiel:
    UPDATE accounts
    SET max_capital_percent = 20
    WHERE max_capital_percent IS NULL;
    
    Regel:
    Schema-Migration ≠ Daten-Migration
    (beides trennen!)

##6. Sicherheit & Stabilität
### 6.1 Transaktionen
Jede Migration läuft in einer Transaktion:

BEGIN;

ALTER TABLE accounts
ADD COLUMN equity DECIMAL(12,2) DEFAULT 0;

COMMIT;

Bei Fehlern:

ROLLBACK;

## 6.2 Backups
Vor jeder Migration:
- DB-Backup erstellen
- Migration lokal testen

## 7. Empfohlener Workflow
    1. Neue fachliche Anforderung
    2. Neue Migration erstellen
    3. Migration lokal testen
    4. Backend-Logik anpassen
    5. Frontend erweitern
    6. Migration deployen