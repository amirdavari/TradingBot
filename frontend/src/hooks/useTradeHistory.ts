import { useState, useEffect } from 'react';
import { getTradeHistory } from '../api/tradingApi';
import type { PaperTrade } from '../models';

export function useTradeHistory(limit: number = 50) {
    const [trades, setTrades] = useState<PaperTrade[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const fetchHistory = async () => {
        try {
            setLoading(true);
            setError(null);
            const data = await getTradeHistory(limit);
            setTrades(data);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to fetch trade history');
            console.error('Error fetching trade history:', err);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchHistory();
    }, [limit]);

    return {
        trades,
        loading,
        error,
        refetch: fetchHistory
    };
}
