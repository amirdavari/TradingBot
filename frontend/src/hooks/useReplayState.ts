import { useState, useEffect, useCallback } from 'react';
import { type ReplayState } from '../models';
import * as replayApi from '../api/replayApi';
import { useSignalRReplayState } from './useSignalR';

/**
 * Hook for managing replay simulation state.
 * IMPORTANT: Frontend holds NO local time - all time comes from backend.
 * Uses SignalR for real-time updates, with initial fetch on mount.
 */
export function useReplayState() {
    const [state, setState] = useState<ReplayState | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    /**
     * Fetches the current replay state from backend.
     * This is the ONLY source of truth for time and mode.
     */
    const fetchState = useCallback(async () => {
        try {
            const replayState = await replayApi.getReplayState();
            setState(replayState);
            setError(null);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to fetch replay state');
            console.error('Error fetching replay state:', err);
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
     * Starts the replay simulation.
     */
    const start = useCallback(async () => {
        try {
            const response = await replayApi.startReplay();
            setState(response.state);
            setError(null);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to start replay');
            console.error('Error starting replay:', err);
        }
    }, []);

    /**
     * Pauses the replay simulation.
     */
    const pause = useCallback(async () => {
        try {
            const response = await replayApi.pauseReplay();
            setState(response.state);
            setError(null);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to pause replay');
            console.error('Error pausing replay:', err);
        }
    }, []);

    /**
     * Resets the replay to start time.
     */
    const reset = useCallback(async () => {
        try {
            const response = await replayApi.resetReplay();
            setState(response.state);
            setError(null);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to reset replay');
            console.error('Error resetting replay:', err);
        }
    }, []);

    /**
     * Sets the replay speed multiplier.
     */
    const setSpeed = useCallback(async (speed: number) => {
        try {
            const response = await replayApi.setReplaySpeed(speed);
            setState(response.state);
            setError(null);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to set speed');
            console.error('Error setting speed:', err);
        }
    }, []);

    /**
     * Sets the replay start time.
     */
    const setTime = useCallback(async (startTime: string) => {
        try {
            const response = await replayApi.setReplayTime(startTime);
            setState(response.state);
            setError(null);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to set time');
            console.error('Error setting time:', err);
        }
    }, []);

    /**
     * Sets the market mode (LIVE or REPLAY).
     */
    const setMode = useCallback(async (mode: 'LIVE' | 'REPLAY') => {
        try {
            const response = await replayApi.setMarketMode(mode);
            setState(response.state);
            setError(null);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to set mode');
            console.error('Error setting mode:', err);
        }
    }, []);

    // Initial fetch on mount (SignalR will handle subsequent updates)
    useEffect(() => {
        fetchState();
    }, [fetchState]);

    // NOTE: Polling removed - SignalR handles real-time updates

    return {
        state,
        loading,
        error,
        refresh: fetchState,
        start,
        pause,
        reset,
        setSpeed,
        setTime,
        setMode,
    };
}
