import { type TradeSignal, type ScanResult, type Candle, type NewsItem, type Account, type RiskCalculation, type RiskSettings, type AutoTradeSettings, type PaperTrade, type TradeStatistics } from '../models';
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

export async function calculateRisk(
    symbol: string,
    entryPrice: number,
    stopLoss: number,
    takeProfit: number,
    riskPercent?: number
): Promise<RiskCalculation> {
    const params = new URLSearchParams({
        symbol,
        entryPrice: entryPrice.toString(),
        stopLoss: stopLoss.toString(),
        takeProfit: takeProfit.toString()
    });
    
    if (riskPercent !== undefined) {
        params.append('riskPercent', riskPercent.toString());
    }
    
    return fetchWithConfig<RiskCalculation>(`/api/risk/calculate?${params.toString()}`);
}

export async function getRiskSettings(): Promise<RiskSettings> {
    return fetchWithConfig<RiskSettings>('/api/risk/settings');
}

export async function updateRiskSettings(settings: RiskSettings): Promise<RiskSettings> {
    return fetchWithConfig<RiskSettings>('/api/risk/settings', {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(settings)
    });
}

// Auto-Trade Settings
export async function getAutoTradeSettings(): Promise<AutoTradeSettings> {
    return fetchWithConfig<AutoTradeSettings>('/api/risk/autotrade');
}

export async function updateAutoTradeSettings(settings: AutoTradeSettings): Promise<AutoTradeSettings> {
    return fetchWithConfig<AutoTradeSettings>('/api/risk/autotrade', {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify(settings)
    });
}

// Paper Trading
export async function getOpenTrades(): Promise<PaperTrade[]> {
    return fetchWithConfig<PaperTrade[]>('/api/papertrades/open');
}

export async function getTradeHistory(limit: number = 50): Promise<PaperTrade[]> {
    return fetchWithConfig<PaperTrade[]>(`/api/papertrades/history?limit=${limit}`);
}

export async function autoExecuteTrade(
    symbol: string,
    timeframe: number = 5,
    riskPercent?: number
): Promise<PaperTrade> {
    const params = new URLSearchParams({
        symbol,
        timeframe: timeframe.toString()
    });
    
    if (riskPercent !== undefined) {
        params.append('riskPercent', riskPercent.toString());
    }
    
    return fetchWithConfig<PaperTrade>(`/api/papertrades/auto-execute?${params.toString()}`, {
        method: 'POST'
    });
}

export async function createTrade(
    symbol: string,
    direction: 'LONG' | 'SHORT',
    entryPrice: number,
    stopLoss: number,
    takeProfit: number,
    positionSize: number,
    investAmount: number,
    confidence: number,
    reasons: string[],
    riskPercent: number
): Promise<PaperTrade> {
    return fetchWithConfig<PaperTrade>('/api/papertrades', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            symbol,
            direction,
            entryPrice,
            stopLoss,
            takeProfit,
            positionSize,
            investAmount,
            confidence,
            reasons,
            riskPercent
        })
    });
}

export async function closeTrade(tradeId: number): Promise<{ message: string }> {
    return fetchWithConfig<{ message: string }>(`/api/papertrades/${tradeId}/close`, {
        method: 'POST'
    });
}

export async function getUnrealizedPnL(tradeId: number): Promise<number> {
    return fetchWithConfig<number>(`/api/papertrades/${tradeId}/unrealized-pnl`);
}

export async function getTradeStatistics(): Promise<TradeStatistics> {
    return fetchWithConfig<TradeStatistics>('/api/papertrades/statistics');
}
