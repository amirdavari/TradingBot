import { Box, Card, CardContent, Typography, Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper, Chip, IconButton } from '@mui/material';
import { Refresh as RefreshIcon, TrendingUp, TrendingDown } from '@mui/icons-material';
import { useOpenTrades } from '../hooks/useOpenTrades';
import LoadingSpinner from './LoadingSpinner';
import ErrorAlert from './ErrorAlert';

interface OpenTradesPanelProps {
    onTradeClick?: (symbol: string) => void;
}

export function OpenTradesPanel({ onTradeClick }: OpenTradesPanelProps) {
    const { trades, isLoading, error, refresh } = useOpenTrades(5000); // Auto-refresh every 5 seconds

    // Unrealized PnL is now calculated and provided by backend
    // const calculateUnrealizedPnL = (trade: PaperTrade, currentPrice: number): { pnl: number; pnlPercent: number } => {
    //     let pnl = 0;
    //     let pnlPercent = 0;
    // 
    //     if (trade.direction === 'LONG') {
    //         pnl = (currentPrice - trade.entryPrice) * trade.quantity;
    //         pnlPercent = ((currentPrice - trade.entryPrice) / trade.entryPrice) * 100;
    //     } else { // SHORT
    //         pnl = (trade.entryPrice - currentPrice) * trade.quantity;
    //         pnlPercent = ((trade.entryPrice - currentPrice) / trade.entryPrice) * 100;
    //     }
    // 
    //     return {
    //         pnl: Math.round(pnl * 100) / 100,
    //         pnlPercent: Math.round(pnlPercent * 100) / 100
    //     };
    // };

    const formatCurrency = (value: number): string => {
        return new Intl.NumberFormat('de-DE', {
            style: 'currency',
            currency: 'EUR',
            minimumFractionDigits: 2
        }).format(value);
    };

    const formatPercent = (value: number | null | undefined): string => {
        if (value === null || value === undefined) return '0.00%';
        return `${value >= 0 ? '+' : ''}${value.toFixed(2)}%`;
    };

    const formatDateTime = (dateString: string): string => {
        return new Date(dateString).toLocaleString('de-DE', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    };

    const getPnLColor = (pnl: number): string => {
        if (pnl > 0) return 'success.main';
        if (pnl < 0) return 'error.main';
        return 'text.secondary';
    };

    if (isLoading && trades.length === 0) {
        return (
            <Card>
                <CardContent>
                    <LoadingSpinner message="Lade offene Trades..." />
                </CardContent>
            </Card>
        );
    }

    return (
        <Card>
            <CardContent>
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                    <Typography variant="h6" component="h2">
                        Open Trades ({trades.length})
                    </Typography>
                    <IconButton onClick={refresh} size="small" title="Aktualisieren">
                        <RefreshIcon />
                    </IconButton>
                </Box>

                {error && <ErrorAlert error={error} />}

                {trades.length === 0 ? (
                    <Typography color="text.secondary" sx={{ textAlign: 'center', py: 3 }}>
                        No open trades
                    </Typography>
                ) : (
                    <TableContainer component={Paper} variant="outlined">
                        <Table size="small">
                            <TableHead>
                                <TableRow>
                                    <TableCell>Symbol</TableCell>
                                    <TableCell>Direction</TableCell>
                                    <TableCell align="right">Entry</TableCell>
                                    <TableCell align="right">Stop Loss</TableCell>
                                    <TableCell align="right">Take Profit</TableCell>
                                    <TableCell align="right">Position</TableCell>
                                    <TableCell align="right">Invest</TableCell>
                                    <TableCell align="right">PnL</TableCell>
                                    <TableCell>Opened</TableCell>
                                </TableRow>
                            </TableHead>
                            <TableBody>
                                {trades.map((trade) => {
                                    // For now, we'll use entryPrice as current price until we implement real-time price updates
                                    const { pnl, pnlPercent } = trade.pnL !== undefined && trade.pnLPercent !== undefined
                                        ? { pnl: trade.pnL, pnlPercent: trade.pnLPercent }
                                        : { pnl: 0, pnlPercent: 0 };

                                    return (
                                        <TableRow
                                            key={trade.id}
                                            hover
                                            onClick={() => onTradeClick?.(trade.symbol)}
                                            sx={{
                                                cursor: onTradeClick ? 'pointer' : 'default',
                                                '&:hover': {
                                                    backgroundColor: onTradeClick ? 'action.hover' : 'transparent'
                                                }
                                            }}
                                        >
                                            <TableCell>
                                                <Typography variant="body2" fontWeight="medium">
                                                    {trade.symbol}
                                                </Typography>
                                            </TableCell>
                                            <TableCell>
                                                <Chip
                                                    icon={trade.direction === 'LONG' ? <TrendingUp /> : <TrendingDown />}
                                                    label={trade.direction}
                                                    color={trade.direction === 'LONG' ? 'success' : 'error'}
                                                    size="small"
                                                />
                                            </TableCell>
                                            <TableCell align="right">
                                                <Typography variant="body2">
                                                    {formatCurrency(trade.entryPrice)}
                                                </Typography>
                                            </TableCell>
                                            <TableCell align="right">
                                                <Typography variant="body2" color="error.main">
                                                    {formatCurrency(trade.stopLoss)}
                                                </Typography>
                                            </TableCell>
                                            <TableCell align="right">
                                                <Typography variant="body2" color="success.main">
                                                    {formatCurrency(trade.takeProfit)}
                                                </Typography>
                                            </TableCell>
                                            <TableCell align="right">
                                                <Typography variant="body2">
                                                    {trade.positionSize.toFixed(4)}
                                                </Typography>
                                            </TableCell>
                                            <TableCell align="right">
                                                <Typography variant="body2">
                                                    {formatCurrency(trade.investAmount)}
                                                </Typography>
                                            </TableCell>
                                            <TableCell align="right">
                                                <Box>
                                                    <Typography
                                                        variant="body2"
                                                        fontWeight="medium"
                                                        color={getPnLColor(pnl)}
                                                    >
                                                        {formatCurrency(pnl)}
                                                    </Typography>
                                                    <Typography
                                                        variant="caption"
                                                        color={getPnLColor(pnl)}
                                                    >
                                                        {formatPercent(pnlPercent)}
                                                    </Typography>
                                                </Box>
                                            </TableCell>
                                            <TableCell>
                                                <Typography variant="caption" color="text.secondary">
                                                    {formatDateTime(trade.openedAt)}
                                                </Typography>
                                            </TableCell>
                                        </TableRow>
                                    );
                                })}
                            </TableBody>
                        </Table>
                    </TableContainer>
                )}
            </CardContent>
        </Card>
    );
}
