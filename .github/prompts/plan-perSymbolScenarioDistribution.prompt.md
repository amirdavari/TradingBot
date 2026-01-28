# Plan: Per-Symbol Scenario Distribution & Strategy Display

**TL;DR**: Szenarien werden über einen Hash-Algorithmus automatisch auf Symbole verteilt (kein DB-Mapping nötig), und das aktive Szenario wird pro Symbol im UI angezeigt. Strategie-Info wird ebenfalls sichtbar gemacht.

## Steps

1. **`ScenarioService` erweitern** ([API/Services/ScenarioService.cs](API/Services/ScenarioService.cs)) - Neue Methode `GetScenarioForSymbol(symbol)` die via deterministischem Hash aus dem Symbolnamen ein Preset wählt (verteilt Szenarien automatisch ohne DB).

2. **`MockRealtimeCandleProvider` anpassen** ([API/Data/MockRealtimeCandleProvider.cs](API/Data/MockRealtimeCandleProvider.cs)) - Statt `GetActiveScenario()` nun `GetScenarioForSymbol(symbol)` aufrufen, sodass jedes Symbol sein eigenes Szenario bekommt.

3. **Neuer API-Endpoint** ([API/Controllers/ScannerController.cs](API/Controllers/ScannerController.cs) oder neuer Controller) - `GET /api/scenario/symbol-assignments` gibt Liste aller Symbole mit zugewiesenem Szenario und Strategie zurück.

4. **Frontend: SimulationControlPanel erweitern** ([frontend/src/pages/SimulationControlPanel.tsx](frontend/src/pages/SimulationControlPanel.tsx)) - Neue Section "Symbol Assignments" mit Tabelle: Symbol | Szenario | Strategie. Die Zuweisung passiert automatisch, die tabelle dient nur der Info.

## Further Considerations

1. **Verteilungsmethode**: Zufällig beim Start (variiert). Zeige es aber im setting an, damit User es nachvollziehen können.

2. **Override-Möglichkeit**: Soll man für einzelne Symbole das Szenario manuell überschreiben können? → Später als Feature möglich

3. **Strategie-Benennung**: Aktuell nur "VWAP Momentum" - soll das für Zukunft erweiterbar sein mit StrategyType Enum?
