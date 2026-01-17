import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import TextField from '@mui/material/TextField';
import ToggleButton from '@mui/material/ToggleButton';
import ToggleButtonGroup from '@mui/material/ToggleButtonGroup';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemText from '@mui/material/ListItemText';
import Chip from '@mui/material/Chip';
import Divider from '@mui/material/Divider';
import Grid from '@mui/material/Grid';
import Stack from '@mui/material/Stack';
import SentimentSatisfiedIcon from '@mui/icons-material/SentimentSatisfied';
import SentimentNeutralIcon from '@mui/icons-material/SentimentNeutral';
import SentimentDissatisfiedIcon from '@mui/icons-material/SentimentDissatisfied';
import CandlestickChart from '../charts/CandlestickChart';
import { getCandles, getSignal, getNews } from '../api/tradingApi';
import type { Candle, TradeSignal, NewsItem } from '../models';
import LoadingSpinner from '../components/LoadingSpinner';
import ErrorAlert from '../components/ErrorAlert';
import { useWatchlist } from '../hooks/useWatchlist';
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

    const { watchlist, loading: watchlistLoading } = useWatchlist();

    // Refresh data automatically when replay is running
    const refreshData = async () => {
        if (!loading) { // Don't refresh if already loading
            try {
                const [candlesData, signalData] = await Promise.all([
                    getCandles(selectedSymbol, timeframe, '1d'),
                    getSignal(selectedSymbol, timeframe)
                ]);
                setCandles(candlesData);
                setSignal(signalData);
            } catch (err) {
                console.error('Error refreshing data in replay:', err);
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
                const [candlesData, signalData] = await Promise.all([
                    getCandles(selectedSymbol, timeframe, '1d'),
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
            setNewsLoading(true);
            try {
                const newsData = await getNews(selectedSymbol, 5);
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

    const riskReward = signal && signal.entry > 0 && signal.stopLoss > 0
        ? ((signal.takeProfit - signal.entry) / (signal.entry - signal.stopLoss)).toFixed(1)
        : 'N/A';

    return (
        <Box sx={{ width: '100%', height: '100%', display: 'flex', flexDirection: 'column', gap: 0, p: 0 }}>
            {/* Header */}
            <Paper sx={{ p: 1.5, flexShrink: 0 }}>
                <Stack direction="row" spacing={2} alignItems="center">
                    <TextField
                        placeholder="Search symbol..."
                        size="small"
                        value={selectedSymbol}
                        onChange={(e) => setSelectedSymbol(e.target.value.toUpperCase())}
                        sx={{ flexGrow: 1, maxWidth: 300 }}
                    />
                    <ToggleButtonGroup
                        value={timeframe}
                        exclusive
                        onChange={handleTimeframeChange}
                        size="small"
                    >
                        <ToggleButton value={1}>1m</ToggleButton>
                        <ToggleButton value={5}>5m</ToggleButton>
                        <ToggleButton value={15}>15m</ToggleButton>
                    </ToggleButtonGroup>
                </Stack>
            </Paper>

            {/* Main Content */}
            <Grid container spacing={0} sx={{ flexGrow: 1, minHeight: 0 }}>
                {/* Watchlist */}
                <Grid size={2}>
                    <Paper sx={{ height: '100%', overflow: 'auto' }}>
                        <Box sx={{ p: 2, pb: 1 }}>
                            <Typography variant="h6">Watchlist</Typography>
                        </Box>
                        <Divider />
                        {watchlistLoading ? (
                            <Box sx={{ p: 2, display: 'flex', justifyContent: 'center' }}>
                                <LoadingSpinner />
                            </Box>
                        ) : (
                            <List dense>
                                {watchlist.map((item) => (
                                    <ListItem key={item.symbol} disablePadding>
                                        <ListItemButton
                                            selected={selectedSymbol === item.symbol}
                                            onClick={() => handleSymbolChange(item.symbol)}
                                        >
                                            <ListItemText primary={item.symbol} />
                                        </ListItemButton>
                                    </ListItem>
                                ))}
                            </List>
                        )}
                    </Paper>
                </Grid>

                {/* Chart Area */}
                <Grid size={7} sx={{ display: 'flex', flexDirection: 'column', minHeight: 0 }}>
                    <Paper sx={{ height: '100%', display: 'flex', flexDirection: 'column', overflow: 'hidden' }}>
                        <Box sx={{ flexGrow: 1, display: 'flex', alignItems: 'center', justifyContent: 'center', bgcolor: 'background.default', position: 'relative', minHeight: 0 }}>
                            {loading ? (
                                <LoadingSpinner />
                            ) : error ? (
                                <ErrorAlert error={error} />
                            ) : candles.length > 0 ? (
                                <CandlestickChart candles={candles} signal={signal} />
                            ) : (
                                <Typography variant="h6" color="text.secondary">
                                    No data available
                                </Typography>
                            )}
                            {signal && signal.direction !== 'NONE' && (
                                <Chip
                                    label={signal.direction}
                                    color={signal.direction === 'LONG' ? 'success' : 'error'}
                                    sx={{ position: 'absolute', top: 16, right: 16 }}
                                />
                            )}
                        </Box>

                        {/* News Section */}
                        <Box sx={{ p: 2, borderTop: 1, borderColor: 'divider', flexShrink: 0, maxHeight: '30%', overflow: 'auto' }}>
                            <Typography variant="h6" gutterBottom>News</Typography>
                            {newsLoading ? (
                                <Box sx={{ display: 'flex', justifyContent: 'center', py: 2 }}>
                                    <LoadingSpinner />
                                </Box>
                            ) : news.length > 0 ? (
                                <Stack spacing={1}>
                                    {news.map((item, index) => (
                                        <Stack key={index} direction="row" spacing={1} alignItems="flex-start">
                                            {getSentimentIcon(item.sentiment)}
                                            <Box sx={{ flexGrow: 1 }}>
                                                <Typography variant="body2" sx={{ fontWeight: 'medium' }}>
                                                    {item.title}
                                                </Typography>
                                                {item.summary && (
                                                    <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 0.5 }}>
                                                        {item.summary.length > 100 ? `${item.summary.substring(0, 100)}...` : item.summary}
                                                    </Typography>
                                                )}
                                                <Typography variant="caption" color="text.secondary">
                                                    {new Date(item.publishedAt).toLocaleString()} • {item.source}
                                                </Typography>
                                            </Box>
                                        </Stack>
                                    ))}
                                </Stack>
                            ) : (
                                <Typography variant="body2" color="text.secondary">
                                    No news available for {selectedSymbol}
                                </Typography>
                            )}
                        </Box>
                    </Paper>
                </Grid>

                {/* Trade Setup */}
                <Grid size={3} sx={{ display: 'flex', flexDirection: 'column', minHeight: 0 }}>
                    <Paper sx={{ height: '100%', overflow: 'auto', p: 2, display: 'flex', flexDirection: 'column' }}>
                        <Typography variant="h6" gutterBottom>Trade Setup</Typography>
                        <Divider sx={{ mb: 2 }} />

                        {signal ? (
                            <Stack spacing={2}>
                                <Box>
                                    <Typography variant="caption" color="text.secondary">Direction:</Typography>
                                    <ToggleButtonGroup
                                        value={signal.direction}
                                        exclusive
                                        fullWidth
                                        size="small"
                                    >
                                        <ToggleButton value="LONG">LONG</ToggleButton>
                                        <ToggleButton value="SHORT">SHORT</ToggleButton>
                                    </ToggleButtonGroup>
                                </Box>

                                <Box>
                                    <Typography variant="caption" color="text.secondary">Entry Price:</Typography>
                                    <Typography variant="h6">{signal.entry.toFixed(2)}</Typography>
                                </Box>

                                <Box>
                                    <Typography variant="caption" color="text.secondary">Stop Loss:</Typography>
                                    <Typography variant="h6">{signal.stopLoss.toFixed(2)}</Typography>
                                </Box>

                                <Box>
                                    <Typography variant="caption" color="text.secondary">Take Profit:</Typography>
                                    <Typography variant="h6">{signal.takeProfit.toFixed(2)}</Typography>
                                </Box>

                                <Box>
                                    <Typography variant="caption" color="text.secondary">Risk/Reward:</Typography>
                                    <Typography variant="h6">{riskReward}</Typography>
                                </Box>

                                <Box>
                                    <Typography variant="caption" color="text.secondary">Confidence:</Typography>
                                    <Typography variant="h6">{signal.confidence}%</Typography>
                                </Box>

                                <Box>
                                    <Typography variant="caption" color="text.secondary">Reasons:</Typography>
                                    <List dense>
                                        {signal.reasons.map((reason, index) => (
                                            <ListItem key={index} sx={{ py: 0.5, px: 0 }}>
                                                <Typography variant="body2">• {reason}</Typography>
                                            </ListItem>
                                        ))}
                                    </List>
                                </Box>
                            </Stack>
                        ) : (
                            <Typography variant="body2" color="text.secondary">
                                No signal available
                            </Typography>
                        )}
                    </Paper>
                </Grid>
            </Grid>
        </Box>
    );
}
