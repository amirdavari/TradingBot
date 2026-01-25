import { useState, useEffect, useCallback } from 'react';
import { getOpenTrades } from '../api/tradingApi';
import type { PaperTrade } from '../models';
import { useSignalRTradeClosed, useSignalRTradeUpdate } from './useSignalR';

/**
 * Hook for managing open trades with real-time SignalR updates.
 * Fetches initially and listens for trade updates/closes via SignalR.
 */
export function useOpenTrades() {
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

    // Listen for trade closed events - remove from list
    useSignalRTradeClosed((data) => {
        console.log('[useOpenTrades] Trade closed via SignalR:', data.trade.id);
        setTrades(prev => prev.filter(t => t.id !== data.trade.id));
    });

    // Listen for trade updates - add or update in list
    useSignalRTradeUpdate((trade) => {
        console.log('[useOpenTrades] Trade update via SignalR:', trade.id);
        setTrades(prev => {
            const existingIndex = prev.findIndex(t => t.id === trade.id);
            if (existingIndex >= 0) {
                // Update existing trade
                const updated = [...prev];
                updated[existingIndex] = trade;
                return updated;
            } else if (trade.status === 'OPEN') {
                // Add new trade
                return [...prev, trade];
            }
            return prev;
        });
    });

    // Initial fetch on mount
    useEffect(() => {
        fetchTrades();
    }, [fetchTrades]);

    // NOTE: Polling removed - SignalR handles real-time updates

    return {
        trades,
        isLoading,
        error,
        refresh: fetchTrades
    };
}
