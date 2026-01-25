import { useState, useEffect, useCallback, useRef } from 'react';
import type { ScanResult } from '../models';
import { scanStocks } from '../api/tradingApi';

/**
 * Hook to scan stocks for daytrading candidates.
 * Initial scan happens on mount. Updates via SignalR are handled externally via setScanResults.
 * Gracefully handles errors - dashboard will work without scanner data.
 */
export function useScanner(symbols: string[], enabled: boolean = true) {
    const [scanResults, setScanResults] = useState<ScanResult[]>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    // Use ref to track if we've scanned before and avoid infinite loops
    const lastSymbolsKey = useRef<string>('');
    const isScanning = useRef<boolean>(false);
    const hasInitialScan = useRef<boolean>(false);

    useEffect(() => {
        // Skip if disabled
        if (!enabled) {
            console.log('[useScanner] Skipping scan - not enabled');
            return;
        }

        // Skip if no symbols - but don't show error, just return empty
        if (symbols.length === 0) {
            console.log('[useScanner] Skipping scan - no symbols');
            setScanResults([]);
            return;
        }

        const symbolsKey = symbols.join(',');

        // Skip if symbols haven't changed (prevents infinite loops)
        if (symbolsKey === lastSymbolsKey.current && hasInitialScan.current) {
            console.log('[useScanner] Skipping scan - symbols unchanged');
            return;
        }

        const scan = async () => {
            // Skip if already scanning
            if (isScanning.current) {
                console.log('[useScanner] Scan already in progress, skipping');
                return;
            }

            lastSymbolsKey.current = symbolsKey;
            hasInitialScan.current = true;
            isScanning.current = true;
            setLoading(true);
            // Don't clear error on retry - keep old error visible until success

            // Create a timeout promise
            const timeoutPromise = new Promise((_, reject) => {
                setTimeout(() => reject(new Error('Scan timeout after 30 seconds')), 30000);
            });

            try {
                console.log('[useScanner] Scanning symbols:', symbols.length, 'symbols');
                const results = await Promise.race([
                    scanStocks(symbols, 1),
                    timeoutPromise
                ]) as ScanResult[];
                console.log('[useScanner] Scan results:', results.length);
                setScanResults(results);
                setError(null); // Clear error on success
            } catch (err) {
                console.error('[useScanner] Scan error:', err);
                setError(err instanceof Error ? err.message : 'Failed to scan stocks');
                // Keep old results on error - dashboard remains functional
            } finally {
                console.log('[useScanner] Scan completed');
                setLoading(false);
                isScanning.current = false;
            }
        };

        scan();
    }, [symbols, enabled]);

    const reload = useCallback(async () => {
        console.log('[useScanner] Manual reload requested - enabled:', enabled, 'symbols:', symbols.length);

        if (!enabled || symbols.length === 0) {
            console.log('[useScanner] Manual reload: Skipping - not enabled or no symbols');
            return;
        }

        // Skip if already scanning
        if (isScanning.current) {
            console.log('[useScanner] Manual reload: Scan already in progress, skipping');
            return;
        }

        isScanning.current = true;
        setLoading(true);
        // Keep old error until we know new result

        // Create a timeout promise
        const timeoutPromise = new Promise((_, reject) => {
            setTimeout(() => reject(new Error('Scan timeout after 30 seconds')), 30000);
        });

        try {
            console.log('[useScanner] Manual reload: Scanning', symbols.length, 'symbols');
            const results = await Promise.race([
                scanStocks(symbols, 1),
                timeoutPromise
            ]) as ScanResult[];
            console.log('[useScanner] Manual reload: Scan results:', results.length);
            setScanResults(results);
            setError(null); // Clear error on success
        } catch (err) {
            console.error('[useScanner] Manual reload: Scan error:', err);
            setError(err instanceof Error ? err.message : 'Failed to scan stocks');
            // Keep old results on error
        } finally {
            console.log('[useScanner] Manual reload: Scan completed');
            setLoading(false);
            isScanning.current = false;
        }
    }, [symbols, enabled]);

    return {
        scanResults,
        loading,
        error,
        reload,
        setScanResults, // Exposed for SignalR updates
    };
}
