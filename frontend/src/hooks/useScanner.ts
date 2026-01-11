import { useState, useEffect, useCallback, useRef } from 'react';
import type { ScanResult } from '../models';
import { scanStocks } from '../api/tradingApi';

export function useScanner(symbols: string[], enabled: boolean = true) {
    const [scanResults, setScanResults] = useState<ScanResult[]>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    // Use ref to track if we've scanned before and avoid infinite loops
    const lastSymbolsKey = useRef<string>('');

    useEffect(() => {
        const symbolsKey = symbols.join(',');

        // Only scan if symbols changed or it's the first load
        if (!enabled || symbolsKey === lastSymbolsKey.current) {
            return;
        }

        lastSymbolsKey.current = symbolsKey;

        const scan = async () => {
            if (symbols.length === 0) {
                setScanResults([]);
                return;
            }

            setLoading(true);
            setError(null);
            try {
                console.log('Scanning symbols:', symbols);
                const results = await scanStocks(symbols, 1);
                console.log('Scan results:', results);
                setScanResults(results);
            } catch (err) {
                console.error('Scan error:', err);
                setError(err instanceof Error ? err.message : 'Failed to scan stocks');
                setScanResults([]);
            } finally {
                setLoading(false);
            }
        };

        scan();
    }, [symbols, enabled]);

    const manualReload = useCallback(async () => {
        if (!enabled || symbols.length === 0) return;

        setLoading(true);
        setError(null);
        try {
            const results = await scanStocks(symbols, 1);
            setScanResults(results);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to scan stocks');
        } finally {
            setLoading(false);
        }
    }, [symbols, enabled]);

    return {
        scanResults,
        loading,
        error,
        reload: manualReload,
    };
}
