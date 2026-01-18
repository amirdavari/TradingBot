import { useState, useEffect } from 'react';
import { getTradeStatistics } from '../api/tradingApi';
import type { TradeStatistics } from '../models';

export function useTradeStatistics() {
    const [statistics, setStatistics] = useState<TradeStatistics | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const fetchStatistics = async () => {
        try {
            setLoading(true);
            setError(null);
            const data = await getTradeStatistics();
            setStatistics(data);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to fetch trade statistics');
            console.error('Error fetching trade statistics:', err);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchStatistics();
    }, []);

    return {
        statistics,
        loading,
        error,
        refetch: fetchStatistics
    };
}
