import { type TradeSignal, type ScanResult } from '../models';
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
