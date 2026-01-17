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
    const intervalIdRef = useRef<NodeJS.Timeout | null>(null);

    // Keep callback ref up to date
    useEffect(() => {
        callbackRef.current = callback;
    }, [callback]);

    // Extract values we care about
    const isReplayRunning = state?.mode === 'REPLAY' && state?.isRunning === true;

    useEffect(() => {
        // Clear any existing interval
        if (intervalIdRef.current) {
            clearInterval(intervalIdRef.current);
            intervalIdRef.current = null;
        }

        // In replay mode and running, trigger refresh at regular intervals
        if (isReplayRunning) {
            console.log('useReplayRefresh: Setting up interval for replay mode');
            
            intervalIdRef.current = setInterval(() => {
                console.log('useReplayRefresh: Triggering callback');
                callbackRef.current();
            }, interval);
        } else {
            console.log('useReplayRefresh: Not in replay mode or not running, no interval');
        }

        return () => {
            if (intervalIdRef.current) {
                console.log('useReplayRefresh: Cleanup - clearing interval');
                clearInterval(intervalIdRef.current);
                intervalIdRef.current = null;
            }
        };
    }, [isReplayRunning, interval]); // Only depend on the boolean, not the whole state object
}

/**
 * Hook that returns whether replay mode is active and running.
 * Useful for conditional rendering or behavior.
 */
export function useIsReplayRunning(): boolean {
    const { state } = useReplayState(3000);
    return state?.mode === 'REPLAY' && state?.isRunning === true;
}
