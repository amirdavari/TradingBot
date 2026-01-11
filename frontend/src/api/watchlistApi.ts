import { fetchWithConfig, API_CONFIG } from './config';

export interface WatchlistSymbol {
    id: number;
    symbol: string;
    createdAt: string;
}

export async function getWatchlist(): Promise<WatchlistSymbol[]> {
    return fetchWithConfig<WatchlistSymbol[]>('/api/watchlist');
}

export async function addSymbol(symbol: string): Promise<WatchlistSymbol> {
    return fetchWithConfig<WatchlistSymbol>('/api/watchlist', {
        method: 'POST',
        body: JSON.stringify({ symbol }),
    });
}

export async function deleteSymbol(symbol: string): Promise<void> {
    await fetch(`${API_CONFIG.baseURL}/api/watchlist/${symbol}`, {
        method: 'DELETE',
    });
}
