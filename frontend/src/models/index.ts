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
    trend: 'LONG' | 'SHORT' | 'NONE';
    volumeStatus: 'LOW' | 'MEDIUM' | 'HIGH';
    hasNews: boolean;
    confidence: number;
    reasons: string[];
}

export interface WatchlistSymbol {
    id: number;
    symbol: string;
    createdAt: string;
}

export interface ReplayState {
    mode: 'LIVE' | 'REPLAY';
    currentTime: string;
    replayStartTime: string;
    speed: number;
    isRunning: boolean;
}
