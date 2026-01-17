const API_BASE_URL = import.meta.env.VITE_API_URL || 'https://localhost:7001';

export const API_CONFIG = {
    baseURL: API_BASE_URL,
    endpoints: {
        signals: (symbol: string, timeframe?: number) =>
            `/api/signals/${symbol}${timeframe ? `?timeframe=${timeframe}` : ''}`,
        scanner: (symbols?: string[], timeframe?: number) => {
            const params = new URLSearchParams();
            if (symbols?.length) {
                symbols.forEach(s => params.append('symbols', s));
            }
            if (timeframe) {
                params.append('timeframe', timeframe.toString());
            }
            return `/api/scanner${params.toString() ? `?${params}` : ''}`;
        },
        candles: (symbol: string, timeframe?: number, period?: string) => {
            const params = new URLSearchParams();
            if (timeframe) {
                params.append('timeframe', timeframe.toString());
            }
            if (period) {
                params.append('period', period);
            }
            return `/api/candles/${symbol}${params.toString() ? `?${params}` : ''}`;
        },
        news: (symbol: string, count?: number) => {
            const params = new URLSearchParams();
            if (count) {
                params.append('count', count.toString());
            }
            return `/api/news/${symbol}${params.toString() ? `?${params}` : ''}`;
        },
        replay: {
            state: '/api/replay/state',
            start: '/api/replay/start',
            pause: '/api/replay/pause',
            reset: '/api/replay/reset',
            speed: '/api/replay/speed',
            time: '/api/replay/time',
            mode: '/api/replay/mode',
        },
    },
};

async function handleResponse<T>(response: Response): Promise<T> {
    if (!response.ok) {
        const error = await response.text();
        throw new Error(error || `HTTP error! status: ${response.status}`);
    }
    return response.json();
}

export async function fetchWithConfig<T>(
    endpoint: string,
    options?: RequestInit
): Promise<T> {
    const response = await fetch(`${API_CONFIG.baseURL}${endpoint}`, {
        ...options,
        headers: {
            'Content-Type': 'application/json',
            ...options?.headers,
        },
    });
    return handleResponse<T>(response);
}
