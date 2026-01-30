import { type ReplayState } from '../models';
import { API_CONFIG, fetchWithConfig } from './config';

/**
 * Gets the current market mode state from the backend.
 */
export async function getReplayState(): Promise<ReplayState> {
    return fetchWithConfig<ReplayState>(API_CONFIG.endpoints.replay.state);
}

/**
 * Sets the market mode (LIVE or MOCK).
 */
export async function setMarketMode(mode: 'LIVE' | 'MOCK' | 'REPLAY'): Promise<{ message: string; state: ReplayState }> {
    return fetchWithConfig(API_CONFIG.endpoints.replay.mode, {
        method: 'POST',
        body: JSON.stringify({ mode }),
    });
}
