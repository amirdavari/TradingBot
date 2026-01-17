import { useEffect, useRef } from 'react';
import { useReplayState } from './useReplayState';

/**
 * Hook that triggers a callback when replay time advances significantly.
 * This allows components to refresh their data based on the simulated time.
 * 
 * @param callback Function to call when replay time advances
 * @param interval Minimum time between callbacks in milliseconds (default: 5000)
 */
export function useReplayRefresh(callback: () => void, interval: number = 5000) {
    const { state } = useReplayState(2000); // Poll every 2 seconds
    const callbackRef = useRef(callback);

    // Keep callback ref up to date
    useEffect(() => {
        callbackRef.current = callback;
    }, [callback]);

    useEffect(() => {
        if (!state) return;

        // In replay mode and running, trigger refresh at regular intervals
        if (state.mode === 'REPLAY' && state.isRunning) {
            console.log('useReplayRefresh: Setting up interval for replay mode');
            const refreshInterval = setInterval(() => {
                console.log('useReplayRefresh: Triggering callback');
                callbackRef.current();
            }, interval);

            return () => {
                console.log('useReplayRefresh: Clearing interval');
                clearInterval(refreshInterval);
            };
        }
    }, [state?.mode, state?.isRunning, interval]); // Only depend on these values
}

/**
 * Hook that returns whether replay mode is active and running.
 * Useful for conditional rendering or behavior.
 */
export function useIsReplayRunning(): boolean {
    const { state } = useReplayState(3000);
    return state?.mode === 'REPLAY' && state?.isRunning === true;
}
