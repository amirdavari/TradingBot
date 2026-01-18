import { type TradeSignal, type ScanResult, type Candle, type NewsItem, type Account } from '../models';
import { API_CONFIG, fetchWithConfig } from './config';

export async function getSignal(
    symbol: string,
    timeframe?: number
): Promise<TradeSignal> {
    return fetchWithConfig<TradeSignal>(
        API_CONFIG.endpoints.signals(symbol, timeframe)
    );
}

export async function scanStocks(
    symbols?: string[],
    timeframe?: number
): Promise<ScanResult[]> {
    return fetchWithConfig<ScanResult[]>(
        API_CONFIG.endpoints.scanner(symbols, timeframe)
    );
}

export async function getCandles(
    symbol: string,
    timeframe?: number,
    period?: string
): Promise<Candle[]> {
    return fetchWithConfig<Candle[]>(
        API_CONFIG.endpoints.candles(symbol, timeframe, period)
    );
}

export async function getNews(
    symbol: string,
    count?: number
): Promise<NewsItem[]> {
    return fetchWithConfig<NewsItem[]>(
        API_CONFIG.endpoints.news(symbol, count)
    );
}

export async function getAccount(): Promise<Account> {
    return fetchWithConfig<Account>('/api/account');
}

export async function resetAccount(): Promise<Account> {
    return fetchWithConfig<Account>('/api/account/reset', {
        method: 'POST'
    });
}
