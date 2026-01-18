import { useState, useEffect, useCallback } from 'react';
import { getOpenTrades } from '../api/tradingApi';
import type { PaperTrade } from '../models';

export function useOpenTrades(refreshInterval: number = 5000) {
    const [trades, setTrades] = useState<PaperTrade[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const fetchTrades = useCallback(async () => {
        try {
            setError(null);
            const data = await getOpenTrades();
            setTrades(data);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to fetch open trades');
            console.error('Error fetching open trades:', err);
        } finally {
            setIsLoading(false);
        }
    }, []);

    useEffect(() => {
        fetchTrades();
        
        // Auto-refresh every refreshInterval milliseconds
        const interval = setInterval(fetchTrades, refreshInterval);
        
        return () => clearInterval(interval);
    }, [fetchTrades, refreshInterval]);

    return {
        trades,
        isLoading,
        error,
        refresh: fetchTrades
    };
}
