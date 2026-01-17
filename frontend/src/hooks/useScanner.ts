import { useState, useEffect, useCallback, useRef } from 'react';
import type { ScanResult } from '../models';
import { scanStocks } from '../api/tradingApi';

export function useScanner(symbols: string[], enabled: boolean = true) {
    const [scanResults, setScanResults] = useState<ScanResult[]>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    // Use ref to track if we've scanned before and avoid infinite loops
    const lastSymbolsKey = useRef<string>('');
    const isScanning = useRef<boolean>(false);
    const hasInitialScan = useRef<boolean>(false);

    useEffect(() => {
        const symbolsKey = symbols.join(',');

        // Don't scan if disabled or no symbols
        if (!enabled || symbols.length === 0) {
            console.log('useScanner: Skipping scan - enabled:', enabled, 'symbols:', symbols.length);
            return;
        }

        // Only scan if symbols changed
        if (symbolsKey === lastSymbolsKey.current && hasInitialScan.current) {
            console.log('useScanner: Skipping scan - symbols unchanged:', symbolsKey);
            return;
        }

        const scan = async () => {
            // Skip if already scanning
            if (isScanning.current) {
                console.log('Scan already in progress, skipping');
                return;
            }

            lastSymbolsKey.current = symbolsKey;
            hasInitialScan.current = true;
            isScanning.current = true;
            setLoading(true);
            setError(null);
            
            // Create a timeout promise
            const timeoutPromise = new Promise((_, reject) => {
                setTimeout(() => reject(new Error('Scan timeout after 30 seconds')), 30000);
            });
            
            try {
                console.log('Scanning symbols:', symbols);
                const results = await Promise.race([
                    scanStocks(symbols, 1),
                    timeoutPromise
                ]) as ScanResult[];
                console.log('Scan results:', results);
                setScanResults(results);
            } catch (err) {
                console.error('Scan error:', err);
                setError(err instanceof Error ? err.message : 'Failed to scan stocks');
                // Don't clear results on error, keep old results
            } finally {
                console.log('Scan completed, resetting isScanning flag');
                setLoading(false);
                isScanning.current = false;
            }
        };

        scan();
    }, [symbols, enabled]);

    const manualReload = useCallback(async () => {
        console.log('Manual reload requested - enabled:', enabled, 'symbols:', symbols.length);
        
        if (!enabled || symbols.length === 0) {
            console.log('Manual reload: Skipping - not enabled or no symbols');
            return;
        }

        // Skip if already scanning
        if (isScanning.current) {
            console.log('Manual reload: Scan already in progress, skipping');
            return;
        }

        isScanning.current = true;
        setLoading(true);
        setError(null);
        
        // Create a timeout promise
        const timeoutPromise = new Promise((_, reject) => {
            setTimeout(() => reject(new Error('Scan timeout after 30 seconds')), 30000);
        });
        
        try {
            console.log('Manual reload: Scanning symbols:', symbols);
            const results = await Promise.race([
                scanStocks(symbols, 1),
                timeoutPromise
            ]) as ScanResult[];
            console.log('Manual reload: Scan results:', results);
            setScanResults(results);
        } catch (err) {
            console.error('Manual reload: Scan error:', err);
            setError(err instanceof Error ? err.message : 'Failed to scan stocks');
            // Don't clear results on error
        } finally {
            console.log('Manual reload: Scan completed, resetting isScanning flag');
            setLoading(false);
            isScanning.current = false;
        }
    }, [symbols, enabled]);

    return {
        scanResults,
        loading,
        error,
        reload: manualReload,
    };
}
