import Box from '@mui/material/Box';
import Paper from '@mui/material/Paper';
import Typography from '@mui/material/Typography';
import TextField from '@mui/material/TextField';
import ToggleButton from '@mui/material/ToggleButton';
import ToggleButtonGroup from '@mui/material/ToggleButtonGroup';
import Stack from '@mui/material/Stack';
import CandlestickChart from '../charts/CandlestickChart';
import LoadingSpinner from './LoadingSpinner';
import ErrorAlert from './ErrorAlert';
import NewsPanel from './NewsPanel';
import type { Candle, TradeSignal, NewsItem } from '../models';

interface ChartPanelProps {
    candles: Candle[];
    signal: TradeSignal | null;
    news: NewsItem[];
    loading: boolean;
    newsLoading: boolean;
    error: string | null;
    symbol: string;
    timeframe: 1 | 5 | 15;
    onSymbolChange: (symbol: string) => void;
    onTimeframeChange: (timeframe: 1 | 5 | 15) => void;
}

export default function ChartPanel({ 
    candles, 
    signal, 
    news, 
    loading, 
    newsLoading, 
    error, 
    symbol,
    timeframe,
    onSymbolChange,
    onTimeframeChange
}: ChartPanelProps) {
    const handleTimeframeChange = (_: React.MouseEvent<HTMLElement>, newTimeframe: 1 | 5 | 15 | null) => {
        if (newTimeframe !== null) {
            onTimeframeChange(newTimeframe);
        }
    };

    return (
        <Paper sx={{ width: '100%', display: 'flex', flexDirection: 'column', overflow: 'hidden', height: '100%' }}>
            {/* Chart Header */}
            <Box sx={{ p: 1.5, flexShrink: 0, borderBottom: 1, borderColor: 'divider' }}>
                <Stack direction="row" spacing={2} alignItems="center">
                    <TextField
                        label="Symbol"
                        size="small"
                        value={symbol}
                        onChange={(e) => onSymbolChange(e.target.value.toUpperCase())}
                        sx={{ width: 120 }}
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
            </Box>

            {/* Chart Area */}
            <Box sx={{ flexGrow: 1, display: 'flex', alignItems: 'center', justifyContent: 'center', minHeight: 0, overflow: 'hidden' }}>
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
            </Box>
            <NewsPanel news={news} loading={newsLoading} symbol={symbol} />
        </Paper>
    );
}
