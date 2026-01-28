import { fetchWithConfig } from './config';

// =============================================================================
// Types
// =============================================================================

export const MarketRegime = {
    NORMAL: 'NORMAL',
    TREND_UP: 'TREND_UP',
    TREND_DOWN: 'TREND_DOWN',
    RANGE: 'RANGE',
    HIGH_VOL: 'HIGH_VOL',
    LOW_VOL: 'LOW_VOL',
    CRASH: 'CRASH',
} as const;
export type MarketRegime = typeof MarketRegime[keyof typeof MarketRegime];

export const PatternOverlayType = {
    NONE: 'NONE',
    BREAKOUT: 'BREAKOUT',
    BREAKDOWN: 'BREAKDOWN',
    PULLBACK: 'PULLBACK',
    BOUNCE: 'BOUNCE',
    DOUBLE_TOP: 'DOUBLE_TOP',
    DOUBLE_BOTTOM: 'DOUBLE_BOTTOM',
    HEAD_SHOULDERS: 'HEAD_SHOULDERS',
    VWAP_BOUNCE: 'VWAP_BOUNCE',
} as const;
export type PatternOverlayType = typeof PatternOverlayType[keyof typeof PatternOverlayType];

export interface RegimePhase {
    type: MarketRegime;
    bars: number;
    drift?: number;
    volatility?: number;
}

export interface PatternOverlayConfig {
    type: PatternOverlayType;
    startBar: number;
    durationBars: number;
    intensity: number; // 0.0 - 1.0
}

export interface ScenarioConfig {
    name: string;
    seed: number;
    regimes: RegimePhase[];
    overlays: PatternOverlayConfig[];
    gapProbability: number;
    maxGapPercent: number;
    volumeMultiplier: number;
}

export interface ScenarioState {
    isEnabled: boolean;
    activeConfig: ScenarioConfig | null;
    availablePresets: string[];
    symbolAssignments: SymbolScenarioAssignment[];
}

export interface SymbolScenarioAssignment {
    symbol: string;
    scenarioPreset: string;
    strategy: string;
}

/**
 * Simulation settings for configuring market data generation parameters
 */
export interface SimulationSettings {
    /** Volatility scaling factor (0.01 - 1.0), default: 0.15 */
    volatilityScale: number;
    /** Drift scaling factor (0.01 - 1.0), default: 0.1 */
    driftScale: number;
    /** Mean reversion strength (0 - 1.0), default: 0.3 */
    meanReversionStrength: number;
    /** Fat tail probability multiplier (0 - 2.0), default: 0.1 */
    fatTailMultiplier: number;
    /** Fat tail min size (1.0 - 3.0), default: 1.5 */
    fatTailMinSize: number;
    /** Fat tail max size (min - 5.0), default: 2.5 */
    fatTailMaxSize: number;
    /** Max return per bar (0.005 - 0.1), default: 0.02 */
    maxReturnPerBar: number;
    /** Live tick noise (0 - 0.1), default: 0.01 */
    liveTickNoise: number;
    /** High/low range multiplier (0.1 - 1.0), default: 0.3 */
    highLowRangeMultiplier: number;
    /** Pattern overlay strength (0 - 2.0), default: 1.0 */
    patternOverlayStrength: number;
}

// =============================================================================
// API Endpoints Extension
// =============================================================================

const SCENARIO_ENDPOINTS = {
    presets: '/api/scenario/presets',
    current: '/api/scenario/current',
    apply: (name: string) => `/api/scenario/apply/${encodeURIComponent(name)}`,
    custom: '/api/scenario/custom',
    reset: '/api/scenario/reset',
    redistribute: '/api/scenario/redistribute',
    enabled: (enabled: boolean) => `/api/scenario/enabled/${enabled}`,
    settings: '/api/scenario/settings',
    settingsReset: '/api/scenario/settings/reset',
};

// =============================================================================
// API Functions
// =============================================================================

/**
 * Get list of available preset scenario names
 */
export async function getScenarioPresets(): Promise<string[]> {
    return fetchWithConfig<string[]>(SCENARIO_ENDPOINTS.presets);
}

/**
 * Get current scenario state (enabled, active config, presets)
 */
export async function getScenarioState(): Promise<ScenarioState> {
    return fetchWithConfig<ScenarioState>(SCENARIO_ENDPOINTS.current);
}

/**
 * Apply a preset scenario by name
 */
export async function applyScenarioPreset(presetName: string): Promise<void> {
    await fetchWithConfig<void>(SCENARIO_ENDPOINTS.apply(presetName), {
        method: 'POST',
    });
}

/**
 * Apply a custom scenario configuration
 */
export async function applyCustomScenario(config: ScenarioConfig): Promise<void> {
    await fetchWithConfig<void>(SCENARIO_ENDPOINTS.custom, {
        method: 'POST',
        body: JSON.stringify(config),
    });
}

/**
 * Reset scenario to defaults
 */
export async function resetScenario(): Promise<void> {
    await fetchWithConfig<void>(SCENARIO_ENDPOINTS.reset, {
        method: 'POST',
    });
}

/**
 * Enable or disable scenario simulation
 */
export async function setScenarioEnabled(enabled: boolean): Promise<void> {
    await fetchWithConfig<void>(SCENARIO_ENDPOINTS.enabled(enabled), {
        method: 'POST',
    });
}

/**
 * Redistribute scenarios randomly to all symbols
 */
export async function redistributeScenarios(): Promise<ScenarioState> {
    return fetchWithConfig<ScenarioState>(SCENARIO_ENDPOINTS.redistribute, {
        method: 'POST',
    });
}

/**
 * Get current simulation settings
 */
export async function getSimulationSettings(): Promise<SimulationSettings> {
    return fetchWithConfig<SimulationSettings>(SCENARIO_ENDPOINTS.settings);
}

/**
 * Update simulation settings
 */
export async function updateSimulationSettings(settings: SimulationSettings): Promise<SimulationSettings> {
    return fetchWithConfig<SimulationSettings>(SCENARIO_ENDPOINTS.settings, {
        method: 'PUT',
        body: JSON.stringify(settings),
    });
}

/**
 * Reset simulation settings to defaults
 */
export async function resetSimulationSettings(): Promise<SimulationSettings> {
    return fetchWithConfig<SimulationSettings>(SCENARIO_ENDPOINTS.settingsReset, {
        method: 'POST',
    });
}

// =============================================================================
// Helper Functions
// =============================================================================

/**
 * Get display name for a market regime
 */
export function getRegimeDisplayName(regime: MarketRegime): string {
    const names: Record<MarketRegime, string> = {
        [MarketRegime.NORMAL]: 'Normal',
        [MarketRegime.TREND_UP]: 'Trend Up ↗',
        [MarketRegime.TREND_DOWN]: 'Trend Down ↘',
        [MarketRegime.RANGE]: 'Range Bound ↔',
        [MarketRegime.HIGH_VOL]: 'High Volatility ⚡',
        [MarketRegime.LOW_VOL]: 'Low Volatility 💤',
        [MarketRegime.CRASH]: 'Crash 📉',
    };
    return names[regime] || regime;
}

/**
 * Get display name for a pattern overlay type
 */
export function getPatternDisplayName(pattern: PatternOverlayType): string {
    const names: Record<PatternOverlayType, string> = {
        [PatternOverlayType.NONE]: 'None',
        [PatternOverlayType.BREAKOUT]: 'Breakout',
        [PatternOverlayType.BREAKDOWN]: 'Breakdown',
        [PatternOverlayType.PULLBACK]: 'Pullback',
        [PatternOverlayType.BOUNCE]: 'Bounce',
        [PatternOverlayType.DOUBLE_TOP]: 'Double Top',
        [PatternOverlayType.DOUBLE_BOTTOM]: 'Double Bottom',
        [PatternOverlayType.HEAD_SHOULDERS]: 'Head & Shoulders',
        [PatternOverlayType.VWAP_BOUNCE]: 'VWAP Bounce',
    };
    return names[pattern] || pattern;
}

/**
 * Get color for a market regime (for UI visualization)
 */
export function getRegimeColor(regime: MarketRegime): string {
    const colors: Record<MarketRegime, string> = {
        [MarketRegime.NORMAL]: '#9e9e9e',
        [MarketRegime.TREND_UP]: '#4caf50',
        [MarketRegime.TREND_DOWN]: '#f44336',
        [MarketRegime.RANGE]: '#2196f3',
        [MarketRegime.HIGH_VOL]: '#ff9800',
        [MarketRegime.LOW_VOL]: '#00bcd4',
        [MarketRegime.CRASH]: '#9c27b0',
    };
    return colors[regime] || '#757575';
}

/**
 * Create a default scenario config
 */
export function createDefaultScenarioConfig(): ScenarioConfig {
    return {
        name: 'Custom',
        seed: Math.floor(Math.random() * 1000000),
        regimes: [
            { type: MarketRegime.NORMAL, bars: 100 },
        ],
        overlays: [],
        gapProbability: 0.02,
        maxGapPercent: 0.03,
        volumeMultiplier: 1.0,
    };
}
