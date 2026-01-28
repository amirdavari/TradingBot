import { Box, Card, CardContent, Typography, Grid } from '@mui/material';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import TrendingDownIcon from '@mui/icons-material/TrendingDown';
import TimelineIcon from '@mui/icons-material/Timeline';
import type { TradeStatistics } from '../models';

interface StatisticsOverviewProps {
    statistics: TradeStatistics;
}

export default function StatisticsOverview({ statistics }: StatisticsOverviewProps) {
    const formatCurrency = (value: number) => {
        return new Intl.NumberFormat('de-DE', {
            style: 'currency',
            currency: 'EUR'
        }).format(value);
    };

    const formatPercent = (value: number) => {
        return `${value.toFixed(2)}%`;
    };

    const getColorForValue = (value: number) => {
        if (value > 0) return 'success.main';
        if (value < 0) return 'error.main';
        return 'text.secondary';
    };

    const stats = [
        {
            label: 'Total Trades',
            value: statistics.totalTrades.toString(),
            icon: <TimelineIcon />,
            color: 'primary.main'
        },
        {
            label: 'Win Rate',
            value: formatPercent(statistics.winRate),
            icon: statistics.winRate >= 50 ? <TrendingUpIcon /> : <TrendingDownIcon />,
            color: statistics.winRate >= 50 ? 'success.main' : 'error.main'
        },
        {
            label: 'Total P&L',
            value: formatCurrency(statistics.totalPnL),
            icon: statistics.totalPnL >= 0 ? <TrendingUpIcon /> : <TrendingDownIcon />,
            color: getColorForValue(statistics.totalPnL)
        },
        {
            label: 'Average Win',
            value: formatCurrency(statistics.averageWin),
            icon: <TrendingUpIcon />,
            color: 'success.main'
        },
        {
            label: 'Average Loss',
            value: formatCurrency(statistics.averageLoss),
            icon: <TrendingDownIcon />,
            color: 'error.main'
        },
        {
            label: 'Average R',
            value: statistics.averageR.toFixed(2) + 'R',
            icon: <TimelineIcon />,
            color: getColorForValue(statistics.averageR)
        },
        {
            label: 'Max Drawdown',
            value: formatCurrency(statistics.maxDrawdown),
            icon: <TrendingDownIcon />,
            color: 'error.main'
        },
        {
            label: 'Profit Factor',
            value: statistics.profitFactor.toFixed(2),
            icon: statistics.profitFactor >= 1 ? <TrendingUpIcon /> : <TrendingDownIcon />,
            color: statistics.profitFactor >= 1 ? 'success.main' : 'error.main'
        }
    ];

    return (
        <Box>
            <Typography variant="h6" sx={{ mb: 2 }}>
                Trading Statistics
            </Typography>
            <Grid container spacing={2}>
                {stats.map((stat) => (
                    <Grid size={{ xs: 12, sm: 6, md: 3 }} key={stat.label}>
                        <Card>
                            <CardContent>
                                <Box display="flex" alignItems="center" justifyContent="space-between" mb={1}>
                                    <Typography variant="body2" color="text.secondary">
                                        {stat.label}
                                    </Typography>
                                    <Box sx={{ color: stat.color }}>
                                        {stat.icon}
                                    </Box>
                                </Box>
                                <Typography variant="h6" sx={{ color: stat.color }}>
                                    {stat.value}
                                </Typography>
                            </CardContent>
                        </Card>
                    </Grid>
                ))}
            </Grid>

            {(statistics.bestTrade || statistics.worstTrade) && (
                <Box sx={{ mt: 3 }}>
                    <Typography variant="h6" sx={{ mb: 2 }}>
                        Notable Trades
                    </Typography>
                    <Grid container spacing={2}>
                        {statistics.bestTrade && (
                            <Grid size={{ xs: 12, md: 6 }}>
                                <Card sx={{ borderLeft: '4px solid', borderLeftColor: 'success.main' }}>
                                    <CardContent>
                                        <Typography variant="subtitle2" color="success.main" gutterBottom>
                                            Best Trade
                                        </Typography>
                                        <Typography variant="body2">
                                            Symbol: {statistics.bestTrade.symbol}
                                        </Typography>
                                        <Typography variant="body2">
                                            Direction: {statistics.bestTrade.direction}
                                        </Typography>
                                        <Typography variant="body2">
                                            P&L: {formatCurrency(statistics.bestTrade.pnL || 0)} ({formatPercent(statistics.bestTrade.pnLPercent || 0)})
                                        </Typography>
                                        <Typography variant="caption" color="text.secondary">
                                            {new Date(statistics.bestTrade.closedAt || '').toLocaleString('de-DE')}
                                        </Typography>
                                    </CardContent>
                                </Card>
                            </Grid>
                        )}
                        {statistics.worstTrade && (
                            <Grid size={{ xs: 12, md: 6 }}>
                                <Card sx={{ borderLeft: '4px solid', borderLeftColor: 'error.main' }}>
                                    <CardContent>
                                        <Typography variant="subtitle2" color="error.main" gutterBottom>
                                            Worst Trade
                                        </Typography>
                                        <Typography variant="body2">
                                            Symbol: {statistics.worstTrade.symbol}
                                        </Typography>
                                        <Typography variant="body2">
                                            Direction: {statistics.worstTrade.direction}
                                        </Typography>
                                        <Typography variant="body2">
                                            P&L: {formatCurrency(statistics.worstTrade.pnL || 0)} ({formatPercent(statistics.worstTrade.pnLPercent || 0)})
                                        </Typography>
                                        <Typography variant="caption" color="text.secondary">
                                            {new Date(statistics.worstTrade.closedAt || '').toLocaleString('de-DE')}
                                        </Typography>
                                    </CardContent>
                                </Card>
                            </Grid>
                        )}
                    </Grid>
                </Box>
            )}
        </Box>
    );
}
