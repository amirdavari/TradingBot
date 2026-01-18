import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import Box from '@mui/material/Box';
import Grid from '@mui/material/Grid';
import WatchlistPanel from '../components/WatchlistPanel';
import ChartPanel from '../components/ChartPanel';
import TradeSetupPanel from '../components/TradeSetupPanel';
import OpenPaperTradesPanel from '../components/OpenPaperTradesPanel';
import { getCandles, getSignal, getNews } from '../api/tradingApi';
import type { Candle, TradeSignal, NewsItem } from '../models';
import { useReplayRefresh } from '../hooks/useReplayRefresh';

export default function Dashboard() {
    const { symbol: urlSymbol } = useParams<{ symbol: string }>();
    const [timeframe, setTimeframe] = useState<1 | 5 | 15>(5);
    const [selectedSymbol, setSelectedSymbol] = useState(urlSymbol || 'AAPL');
    const [candles, setCandles] = useState<Candle[]>([]);
    const [signal, setSignal] = useState<TradeSignal | null>(null);
    const [news, setNews] = useState<NewsItem[]>([]);
    const [loading, setLoading] = useState(false);
    const [newsLoading, setNewsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    // Mock data for open paper trades (TODO: fetch from API)
    const openTrades = [
        // { symbol: 'AAPL', direction: 'LONG' as const, profitLossPercent: 0.8 },
        // { symbol: 'TSLA', direction: 'SHORT' as const, profitLossPercent: -0.3 },
    ];

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

    // Refresh data automatically when replay is running
    const refreshData = async () => {
        if (!loading) { // Don't refresh if already loading
            try {
                const period = getPeriodForTimeframe(timeframe);
                const [candlesData, signalData] = await Promise.all([
                    getCandles(selectedSymbol, timeframe, period),
                    getSignal(selectedSymbol, timeframe)
                ]);
                
                // Always create a new array reference to trigger React updates
                // This ensures the chart updates even if candle count is the same
                setCandles([...candlesData]);
                setSignal({...signalData});
                setError(null); // Clear any previous errors on successful refresh
                
                console.log(`Refreshed data: ${candlesData.length} candles, last candle: ${candlesData[candlesData.length - 1]?.time}`);
            } catch (err) {
                const errorMessage = err instanceof Error ? err.message : 'Failed to refresh data';
                setError(errorMessage);
                console.error('Error refreshing data in replay:', err);
                // Clear candles and signal on error
                setCandles([]);
                setSignal(null);
            }
        }
    };

    // Auto-refresh during replay simulation (every 5 seconds)
    useReplayRefresh(refreshData, 5000);

    const getSentimentIcon = (sentiment: string) => {
        switch (sentiment) {
            case 'positive': return <SentimentSatisfiedIcon color="success" />;
            case 'neutral': return <SentimentNeutralIcon color="warning" />;
            case 'negative': return <SentimentDissatisfiedIcon color="error" />;
            default: return <SentimentNeutralIcon />;
        }
    };

    // Update selected symbol when URL param changes
    useEffect(() => {
        if (urlSymbol) {
            setSelectedSymbol(urlSymbol);
        }
    }, [urlSymbol]);

    // Fetch data when symbol or timeframe changes
    useEffect(() => {
        const fetchData = async () => {
            setLoading(true);
            setError(null);
            try {
                const period = getPeriodForTimeframe(timeframe);
                const [candlesData, signalData] = await Promise.all([
                    getCandles(selectedSymbol, timeframe, period),
                    getSignal(selectedSymbol, timeframe)
                ]);
                setCandles(candlesData);
                setSignal(signalData);
            } catch (err) {
                setError(err instanceof Error ? err.message : 'Failed to fetch data');
                console.error('Error fetching data:', err);
            } finally {
                setLoading(false);
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

    const handleTimeframeChange = (_: React.MouseEvent<HTMLElement>, newTimeframe: 1 | 5 | 15 | null) => {
        if (newTimeframe !== null) {
            setTimeframe(newTimeframe);
        }
    };

    const handleSymbolChange = (symbol: string) => {
        setSelectedSymbol(symbol);
    };

    const handleBuyTrade = (investAmount: number) => {
        if (!signal || signal.direction !== 'LONG') return;
        console.log('BUY Paper Trade:', { symbol: selectedSymbol, signal, investAmount });
        // TODO: API call to create paper trade
    };

    const handleSellTrade = (investAmount: number) => {
        if (!signal || signal.direction !== 'SHORT') return;
        console.log('SELL Paper Trade:', { symbol: selectedSymbol, signal, investAmount });
        // TODO: API call to create paper trade
    };

    const handleTradeClick = (symbol: string) => {
        setSelectedSymbol(symbol);
    };

    return (
        <Box sx={{ width: '100%', height: '100%', display: 'flex', flexDirection: 'column', gap: 1, p: 0 }}>
            {/* Main Content */}
            <Grid container spacing={1} sx={{ flexGrow: 1, minHeight: 0, overflow: 'hidden' }}>
                {/* Watchlist Panel */}
                <Grid size={2} sx={{ height: '100%', display: 'flex' }}>
                    <WatchlistPanel 
                        selectedSymbol={selectedSymbol}
                        onSymbolChange={handleSymbolChange}
                    />
                </Grid>

                {/* Chart + News Panel */}
                <Grid size={7} sx={{ height: '100%', display: 'flex' }}>
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
                <Grid size={3} sx={{ height: '100%', display: 'flex' }}>
                    <TradeSetupPanel
                        signal={signal}
                        symbol={selectedSymbol}
                        timeframe={timeframe}
                        onBuyTrade={handleBuyTrade}
                        onSellTrade={handleSellTrade}
                    />
                </Grid>
            </Grid>

            {/* Open Paper Trades Panel */}
            <OpenPaperTradesPanel 
                trades={openTrades}
                onTradeClick={handleTradeClick}
            />
        </Box>
    );
}
