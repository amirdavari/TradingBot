import { useState, useEffect, useCallback } from 'react';
import { type ReplayState } from '../models';
import * as replayApi from '../api/replayApi';
import { useSignalRReplayState } from './useSignalR';

/**
 * Hook for managing market mode state (LIVE vs MOCK).
 * Uses SignalR for real-time updates.
 */
export function useReplayState() {
    const [state, setState] = useState<ReplayState | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    /**
     * Fetches the current state from backend.
     */
    const fetchState = useCallback(async () => {
        try {
            const replayState = await replayApi.getReplayState();
            setState(replayState);
            setError(null);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to fetch state');
            console.error('Error fetching state:', err);
        } finally {
            setLoading(false);
        }
    }, []);

    // Listen for SignalR updates
    useSignalRReplayState((newState) => {
        setState(newState);
        setError(null);
    });

    /**
     * Sets the market mode (LIVE or MOCK).
     */
    const setMode = useCallback(async (mode: 'LIVE' | 'MOCK') => {
        try {
            const response = await replayApi.setMarketMode(mode);
            setState(response.state);
            setError(null);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to set mode');
            console.error('Error setting mode:', err);
        }
    }, []);

    // Initial fetch on mount
    useEffect(() => {
        fetchState();
    }, [fetchState]);

    return {
        state,
        loading,
        error,
        refresh: fetchState,
        setMode,
    };
}
