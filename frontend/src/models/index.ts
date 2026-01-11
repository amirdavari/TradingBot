export interface Candle {
    time: string;
    open: number;
    high: number;
    low: number;
    close: number;
    volume: number;
}

export interface TradeSignal {
    symbol: string;
    direction: 'LONG' | 'SHORT' | 'NONE';
    entry: number;
    stopLoss: number;
    takeProfit: number;
    confidence: number;
    reasons: string[];
}

export interface NewsItem {
    title: string;
    summary: string;
    sentiment: 'positive' | 'neutral' | 'negative';
    publishedAt: string;
    source: string;
}

export interface ScanResult {
    symbol: string;
    score: number;
    reasons: string[];
}
