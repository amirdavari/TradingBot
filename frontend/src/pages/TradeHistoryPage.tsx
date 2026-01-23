import { Box, Container, Paper } from '@mui/material';
import { useTradeHistory } from '../hooks/useTradeHistory';
import { useTradeStatistics } from '../hooks/useTradeStatistics';
import StatisticsOverview from '../components/StatisticsOverview';
import TradeHistoryTable from '../components/TradeHistoryTable';
import LoadingSpinner from '../components/LoadingSpinner';
import ErrorAlert from '../components/ErrorAlert';
import type { PaperTrade } from '../models';

export default function TradeHistoryPage() {
    const { trades, loading: tradesLoading, error: tradesError } = useTradeHistory(100);
    const { statistics, loading: statsLoading, error: statsError } = useTradeStatistics();

    const handleTradeClick = (trade: PaperTrade) => {
        // TODO: Open trade details dialog or navigate to chart
        console.log('Trade clicked:', trade);
    };

    if (tradesLoading || statsLoading) {
        return <LoadingSpinner />;
    }

    if (tradesError) {
        return <ErrorAlert error={tradesError} />;
    }

    if (statsError) {
        return <ErrorAlert error={statsError} />;
    }

    return (
        <Box sx={{ height: '100vh', display: 'flex', flexDirection: 'column', p: 3 }}>
            <Box sx={{ mb: 2, flexShrink: 0 }}>
                {statistics && <StatisticsOverview statistics={statistics} />}
            </Box>

            <Paper sx={{ p: 2, flexGrow: 1, display: 'flex', flexDirection: 'column', minHeight: 0, overflow: 'hidden' }}>
                <TradeHistoryTable trades={trades} onTradeClick={handleTradeClick} />
            </Paper>
        </Box>
    );
}
