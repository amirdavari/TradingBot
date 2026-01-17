import { type ReplayState } from '../models';
import { API_CONFIG, fetchWithConfig } from './config';

/**
 * Gets the current replay state from the backend.
 * Frontend MUST NOT calculate time locally - always fetch from backend.
 */
export async function getReplayState(): Promise<ReplayState> {
    return fetchWithConfig<ReplayState>(API_CONFIG.endpoints.replay.state);
}

/**
 * Starts the replay simulation.
 */
export async function startReplay(): Promise<{ message: string; state: ReplayState }> {
    return fetchWithConfig(API_CONFIG.endpoints.replay.start, {
        method: 'POST',
    });
}

/**
 * Pauses the replay simulation.
 */
export async function pauseReplay(): Promise<{ message: string; state: ReplayState }> {
    return fetchWithConfig(API_CONFIG.endpoints.replay.pause, {
        method: 'POST',
    });
}

/**
 * Resets the replay to the start time.
 */
export async function resetReplay(): Promise<{ message: string; state: ReplayState }> {
    return fetchWithConfig(API_CONFIG.endpoints.replay.reset, {
        method: 'POST',
    });
}

/**
 * Sets the replay speed multiplier.
 */
export async function setReplaySpeed(speed: number): Promise<{ message: string; state: ReplayState }> {
    return fetchWithConfig(API_CONFIG.endpoints.replay.speed, {
        method: 'POST',
        body: JSON.stringify({ speed }),
    });
}

/**
 * Sets the replay start time.
 */
export async function setReplayTime(startTime: string): Promise<{ message: string; state: ReplayState }> {
    return fetchWithConfig(API_CONFIG.endpoints.replay.time, {
        method: 'POST',
        body: JSON.stringify({ startTime }),
    });
}

/**
 * Sets the market mode (LIVE or REPLAY).
 */
export async function setMarketMode(mode: 'LIVE' | 'REPLAY'): Promise<{ message: string; state: ReplayState }> {
    return fetchWithConfig(API_CONFIG.endpoints.replay.mode, {
        method: 'POST',
        body: JSON.stringify({ mode }),
    });
}
