import { useState, useEffect, useCallback, useRef } from 'react';
import { useParams } from 'react-router-dom';
import Box from '@mui/material/Box';
import Grid from '@mui/material/Grid';
import WatchlistPanel from '../components/WatchlistPanel';
import ChartPanel from '../components/ChartPanel';
import TradeSetupPanel from '../components/TradeSetupPanel';
import { OpenTradesPanel } from '../components/OpenTradesPanel';
import { getCandles, getSignal, getNews, getAccount, createTrade } from '../api/tradingApi';
import type { Candle, TradeSignal, NewsItem, Account, RiskCalculation } from '../models';
import { useSignalRChartRefresh, useSignalRAccountUpdate } from '../hooks/useSignalR';

export default function Dashboard() {
    const { symbol: urlSymbol } = useParams<{ symbol: string }>();
    const [timeframe, setTimeframe] = useState<1 | 5 | 15>(5);
    const [selectedSymbol, setSelectedSymbol] = useState(urlSymbol || 'AAPL');
    const [candles, setCandles] = useState<Candle[]>([]);
    const [signal, setSignal] = useState<TradeSignal | null>(null);
    const [news, setNews] = useState<NewsItem[]>([]);
    const [account, setAccount] = useState<Account | null>(null);
    const [loading, setLoading] = useState(false);
    const [newsLoading, setNewsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    console.log('[Dashboard] State:', {
        selectedSymbol,
        timeframe,
        candleCount: candles.length,
        loading,
        error,
        hasSignal: !!signal
    });

    // Get period based on timeframe (longer periods to ensure enough data)
    // Yahoo Finance limits: 1m=7days, 5m/15m=60days
    const getPeriodForTimeframe = (tf: number): string => {
        switch (tf) {
            case 1: return '7d';   // 1-minute: Use max available (Yahoo limit)
            case 5: return '5d';   // 5-minute: 5 days provides plenty of candles
            case 15: return '5d';  // 15-minute: 5 days provides plenty of candles
            default: return '5d';
        }
    };

    // Refresh data function (called by SignalR or initial load)
    // Uses ref to check loading state to avoid stale closure issues
    const loadingRef = useRef(loading);
    loadingRef.current = loading;

    // Track if initial data fetch has completed
    const hasInitialFetch = useRef(false);

    const refreshData = useCallback(async () => {
        // Only allow SignalR refresh after initial fetch is complete
        if (!hasInitialFetch.current) {
            console.log('[Dashboard] Skipping SignalR refresh - initial fetch not complete');
            return;
        }

        if (loadingRef.current) {
            console.log('[Dashboard] Skipping refresh - already loading');
            return;
        }

        console.log('[Dashboard] Starting SignalR data refresh for', selectedSymbol);

        try {
            const period = getPeriodForTimeframe(timeframe);
            const [candlesData, signalData] = await Promise.all([
                getCandles(selectedSymbol, timeframe, period),
                getSignal(selectedSymbol, timeframe)
            ]);

            // Always create a new array reference to trigger React updates
            // This ensures the chart updates even if candle count is the same
            setCandles([...candlesData]);
            setSignal({ ...signalData });
            setError(null); // Clear any previous errors on successful refresh

            console.log(`[Dashboard] Refreshed data: ${candlesData.length} candles, last candle: ${candlesData[candlesData.length - 1]?.time}`);
        } catch (err) {
            const errorMessage = err instanceof Error ? err.message : 'Failed to refresh data';
            setError(errorMessage);
            console.error('[Dashboard] Error refreshing data:', err);
            // Keep existing data on SignalR refresh error - don't clear
        }
    }, [selectedSymbol, timeframe]);

    // Real-time chart refresh via SignalR (replaces polling in replay mode)
    const handleChartRefresh = useCallback(() => {
        console.log('[Dashboard] Chart refresh via SignalR');
        refreshData();
    }, [refreshData]);

    useSignalRChartRefresh(handleChartRefresh);

    // Real-time account updates via SignalR
    const handleAccountUpdate = useCallback((updatedAccount: Account) => {
        console.log('[Dashboard] Account update via SignalR:', updatedAccount);
        setAccount(updatedAccount);
    }, []);

    useSignalRAccountUpdate(handleAccountUpdate);

    // Fetch account data
    const fetchAccount = async () => {
        try {
            const accountData = await getAccount();
            setAccount(accountData);
        } catch (err) {
            console.error('Failed to fetch account:', err);
        }
    };

    // Fetch account data on mount
    useEffect(() => {
        fetchAccount();
    }, []);

    // Update selected symbol when URL param changes
    useEffect(() => {
        if (urlSymbol) {
            setSelectedSymbol(urlSymbol);
        }
    }, [urlSymbol]);

    // Fetch data when symbol or timeframe changes
    useEffect(() => {
        const fetchData = async () => {
            console.log(`[Dashboard] Fetching data for ${selectedSymbol}, timeframe: ${timeframe}`);
            setLoading(true);
            setError(null);
            try {
                const period = getPeriodForTimeframe(timeframe);
                console.log(`[Dashboard] Using period: ${period}`);
                const [candlesData, signalData] = await Promise.all([
                    getCandles(selectedSymbol, timeframe, period),
                    getSignal(selectedSymbol, timeframe)
                ]);
                console.log(`[Dashboard] Received ${candlesData.length} candles for ${selectedSymbol}`);
                setCandles(candlesData);
                setSignal(signalData);
            } catch (err) {
                setError(err instanceof Error ? err.message : 'Failed to fetch data');
                console.error('[Dashboard] Error fetching data:', err);
            } finally {
                setLoading(false);
                // Mark initial fetch as complete after first successful or failed load
                hasInitialFetch.current = true;
            }
        };

        fetchData();
    }, [selectedSymbol, timeframe]);

    // Fetch news when symbol changes
    useEffect(() => {
        const fetchNewsData = async () => {
            console.log(`Fetching news for symbol: ${selectedSymbol}`);
            setNewsLoading(true);
            try {
                const newsData = await getNews(selectedSymbol, 5);
                console.log(`News fetched for ${selectedSymbol}:`, newsData);
                setNews(newsData);
            } catch (err) {
                console.error('Error fetching news:', err);
                // Don't set main error for news failures
                setNews([]);
            } finally {
                setNewsLoading(false);
            }
        };

        fetchNewsData();
    }, [selectedSymbol]);


    const handleSymbolChange = (symbol: string) => {
        setSelectedSymbol(symbol);
    };

    const handleBuyTrade = async (signal: TradeSignal, riskCalc: RiskCalculation, riskPercent: number) => {
        if (signal.direction !== 'LONG') return;

        try {
            const trade = await createTrade(
                signal.symbol,
                'LONG',
                signal.entry,
                signal.stopLoss,
                signal.takeProfit,
                riskCalc.positionSize,
                riskCalc.investAmount,
                signal.confidence,
                signal.reasons,
                riskPercent
            );

            console.log('Trade created:', trade);

            // Refresh account and open trades
            await fetchAccount();

        } catch (error) {
            console.error('Failed to create trade:', error);
            const errorMessage = error instanceof Error ? error.message : 'Unbekannter Fehler';
            alert(`Fehler beim Erstellen des Trades: ${errorMessage}`);
        }
    };

    const handleSellTrade = async (signal: TradeSignal, riskCalc: RiskCalculation, riskPercent: number) => {
        if (signal.direction !== 'SHORT') return;

        try {
            const trade = await createTrade(
                signal.symbol,
                'SHORT',
                signal.entry,
                signal.stopLoss,
                signal.takeProfit,
                riskCalc.positionSize,
                riskCalc.investAmount,
                signal.confidence,
                signal.reasons,
                riskPercent
            );

            console.log('Trade created:', trade);

            // Refresh account and open trades
            await fetchAccount();

        } catch (error) {
            console.error('Failed to create trade:', error);
            const errorMessage = error instanceof Error ? error.message : 'Unbekannter Fehler';
            alert(`Fehler beim Erstellen des Trades: ${errorMessage}`);
        }
    };

    const handleTradeClick = (symbol: string) => {
        setSelectedSymbol(symbol);
    };

    return (
        <Box sx={{ width: '100%', height: '100vh', display: 'flex', flexDirection: 'column', gap: 1, p: 1 }}>
            {/* Main Content */}
            <Grid container spacing={1} sx={{ flexShrink: 0, height: '65vh', minHeight: '500px' }}>
                {/* Watchlist Panel */}
                <Grid size={3.5} sx={{ height: '100%', display: 'flex' }}>
                    <WatchlistPanel
                        selectedSymbol={selectedSymbol}
                        onSymbolChange={handleSymbolChange}
                    />
                </Grid>

                {/* Chart + News Panel */}
                <Grid size={6} sx={{ height: '100%', display: 'flex' }}>
                    <ChartPanel
                        candles={candles}
                        signal={signal}
                        news={news}
                        loading={loading}
                        newsLoading={newsLoading}
                        error={error}
                        symbol={selectedSymbol}
                        timeframe={timeframe}
                        onSymbolChange={handleSymbolChange}
                        onTimeframeChange={(tf) => setTimeframe(tf)}
                    />
                </Grid>

                {/* Trade Setup Panel */}
                <Grid size={2.5} sx={{ height: '100%', display: 'flex' }}>
                    <TradeSetupPanel
                        signal={signal}
                        symbol={selectedSymbol}
                        timeframe={timeframe}
                        availableCash={account?.availableCash ?? 0}
                        onBuyTrade={handleBuyTrade}
                        onSellTrade={handleSellTrade}
                    />
                </Grid>
            </Grid>

            {/* Open Trades Panel - scrollbar with remaining space */}
            <Box sx={{ flexGrow: 1, minHeight: 0, overflow: 'hidden', display: 'flex' }}>
                <OpenTradesPanel onTradeClick={handleTradeClick} />
            </Box>
        </Box>
    );
}
