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
    currentPrice: number;
    hasError?: boolean;
    errorMessage?: string;
}

export interface WatchlistSymbol {
    id: number;
    symbol: string;
    companyName?: string;
    createdAt: string;
}

export interface ReplayState {
    mode: 'LIVE' | 'MOCK' | 'REPLAY'; // REPLAY for backwards compatibility
    currentTime: string;
}

export interface Account {
    id: number;
    initialBalance: number;
    balance: number;
    equity: number;
    availableCash: number;
    createdAt: string;
    updatedAt: string;
}

export interface RiskCalculation {
    symbol: string;
    entryPrice: number;
    stopLoss: number;
    takeProfit: number;
    investAmount: number;
    positionSize: number;
    riskAmount: number;
    riskPercent: number;
    rewardAmount: number;
    riskRewardRatio: number;
    isAllowed: boolean;
    messages: string[];
    limitingFactor: string;
    riskUtilization: number;
    maxCapitalPercent: number;
}

export interface RiskSettings {
    defaultRiskPercent: number;
    maxRiskPercent: number;
    minRiskRewardRatio: number;
    maxCapitalPercent: number;
}

export interface AutoTradeSettings {
    enabled: boolean;
    minConfidence: number;
    riskPercent: number;
    maxConcurrentTrades: number;
}

export interface PaperTrade {
    id: number;
    symbol: string;
    direction: 'LONG' | 'SHORT';
    entryPrice: number;
    stopLoss: number;
    takeProfit: number;
    quantity: number;
    positionSize: number;
    investAmount: number;
    status: 'OPEN' | 'CLOSED_SL' | 'CLOSED_TP' | 'CLOSED_MANUAL';
    confidence: number;
    reasons: string[];
    openedAt: string;
    closedAt?: string;
    exitPrice?: number;
    pnL?: number;
    pnLPercent?: number;
    notes?: string;
}

export interface TradeStatistics {
    totalTrades: number;
    winningTrades: number;
    losingTrades: number;
    winRate: number;
    totalPnL: number;
    averageWin: number;
    averageLoss: number;
    averageR: number;
    maxDrawdown: number;
    profitFactor: number;
    bestTrade?: PaperTrade;
    worstTrade?: PaperTrade;
}
