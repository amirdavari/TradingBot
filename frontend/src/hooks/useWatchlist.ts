import { useState, useEffect } from 'react';
import { getWatchlist, addSymbol as addSymbolApi, deleteSymbol as deleteSymbolApi, type WatchlistSymbol } from '../api/watchlistApi';

export function useWatchlist() {
    const [watchlist, setWatchlist] = useState<WatchlistSymbol[]>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const loadWatchlist = async () => {
        setLoading(true);
        setError(null);
        try {
            const data = await getWatchlist();
            setWatchlist(data);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to load watchlist');
        } finally {
            setLoading(false);
        }
    };

    const addSymbol = async (symbol: string) => {
        setError(null);
        try {
            const newSymbol = await addSymbolApi(symbol);
            setWatchlist(prev => [...prev, newSymbol].sort((a, b) => a.symbol.localeCompare(b.symbol)));
        } catch (err) {
            const errorMsg = err instanceof Error ? err.message : 'Failed to add symbol';
            setError(errorMsg);
            throw new Error(errorMsg);
        }
    };

    const deleteSymbol = async (symbol: string) => {
        setError(null);
        try {
            await deleteSymbolApi(symbol);
            setWatchlist(prev => prev.filter(s => s.symbol !== symbol));
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to delete symbol');
        }
    };

    useEffect(() => {
        loadWatchlist();
    }, []);

    return {
        watchlist,
        loading,
        error,
        addSymbol,
        deleteSymbol,
        reload: loadWatchlist,
    };
}
