import { useState } from 'react';
import {
    Box,
    Paper,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    TableSortLabel,
    Chip,
    Typography,
    IconButton
} from '@mui/material';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import TrendingDownIcon from '@mui/icons-material/TrendingDown';
import InfoIcon from '@mui/icons-material/Info';
import type { PaperTrade } from '../models';

interface TradeHistoryTableProps {
    trades: PaperTrade[];
    onTradeClick?: (trade: PaperTrade) => void;
}

type SortField = 'closedAt' | 'symbol' | 'pnL' | 'pnLPercent';
type SortDirection = 'asc' | 'desc';

export default function TradeHistoryTable({ trades, onTradeClick }: TradeHistoryTableProps) {
    const [sortField, setSortField] = useState<SortField>('closedAt');
    const [sortDirection, setSortDirection] = useState<SortDirection>('desc');

    const handleSort = (field: SortField) => {
        if (sortField === field) {
            setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc');
        } else {
            setSortField(field);
            setSortDirection('desc');
        }
    };

    const sortedTrades = [...trades].sort((a, b) => {
        let aValue: any;
        let bValue: any;

        switch (sortField) {
            case 'closedAt':
                aValue = new Date(a.closedAt || 0).getTime();
                bValue = new Date(b.closedAt || 0).getTime();
                break;
            case 'symbol':
                aValue = a.symbol;
                bValue = b.symbol;
                break;
            case 'pnL':
                aValue = a.pnL || 0;
                bValue = b.pnL || 0;
                break;
            case 'pnLPercent':
                aValue = a.pnLPercent || 0;
                bValue = b.pnLPercent || 0;
                break;
            default:
                return 0;
        }

        if (aValue < bValue) return sortDirection === 'asc' ? -1 : 1;
        if (aValue > bValue) return sortDirection === 'asc' ? 1 : -1;
        return 0;
    });

    const formatCurrency = (value: number) => {
        return new Intl.NumberFormat('de-DE', {
            style: 'currency',
            currency: 'EUR'
        }).format(value);
    };

    const formatPercent = (value: number) => {
        return `${value >= 0 ? '+' : ''}${value.toFixed(2)}%`;
    };

    const getStatusLabel = (status: string) => {
        const labels: Record<string, { text: string; color: 'success' | 'error' | 'warning' }> = {
            'CLOSED_TP': { text: 'Take Profit', color: 'success' },
            'CLOSED_SL': { text: 'Stop Loss', color: 'error' },
            'CLOSED_MANUAL': { text: 'Manual', color: 'warning' }
        };
        return labels[status] || { text: status, color: 'warning' };
    };

    return (
        <Box>
            <Typography variant="h6" sx={{ mb: 2 }}>
                Trade History
            </Typography>
            <TableContainer component={Paper}>
                <Table size="small" >
                    <TableHead>
                        <TableRow>
                            <TableCell>
                                <TableSortLabel
                                    active={sortField === 'closedAt'}
                                    direction={sortField === 'closedAt' ? sortDirection : 'asc'}
                                    onClick={() => handleSort('closedAt')}
                                >
                                    Closed At
                                </TableSortLabel>
                            </TableCell>
                            <TableCell>
                                <TableSortLabel
                                    active={sortField === 'symbol'}
                                    direction={sortField === 'symbol' ? sortDirection : 'asc'}
                                    onClick={() => handleSort('symbol')}
                                >
                                    Symbol
                                </TableSortLabel>
                            </TableCell>
                            <TableCell>Direction</TableCell>
                            <TableCell align="right">Entry</TableCell>
                            <TableCell align="right">Exit</TableCell>
                            <TableCell align="right">Quantity</TableCell>
                            <TableCell>Status</TableCell>
                            <TableCell align="right">
                                <TableSortLabel
                                    active={sortField === 'pnL'}
                                    direction={sortField === 'pnL' ? sortDirection : 'asc'}
                                    onClick={() => handleSort('pnL')}
                                >
                                    P&L
                                </TableSortLabel>
                            </TableCell>
                            <TableCell align="right">
                                <TableSortLabel
                                    active={sortField === 'pnLPercent'}
                                    direction={sortField === 'pnLPercent' ? sortDirection : 'asc'}
                                    onClick={() => handleSort('pnLPercent')}
                                >
                                    P&L %
                                </TableSortLabel>
                            </TableCell>
                            <TableCell align="center">Details</TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {sortedTrades.length === 0 ? (
                            <TableRow>
                                <TableCell colSpan={10} align="center">
                                    <Typography variant="body2" color="text.secondary">
                                        No closed trades yet
                                    </Typography>
                                </TableCell>
                            </TableRow>
                        ) : (
                            sortedTrades.map((trade) => {
                                const statusInfo = getStatusLabel(trade.status);
                                const pnl = trade.pnL || 0;
                                const pnlPercent = trade.pnLPercent || 0;

                                return (
                                    <TableRow
                                        key={trade.id}
                                        hover
                                        sx={{
                                            cursor: onTradeClick ? 'pointer' : 'default',
                                            backgroundColor: pnl >= 0 ? 'success.lighter' : 'error.lighter'
                                        }}
                                        onClick={() => onTradeClick?.(trade)}
                                    >
                                        <TableCell>
                                            <Typography variant="caption">
                                                {new Date(trade.closedAt || '').toLocaleString('de-DE', {
                                                    dateStyle: 'short',
                                                    timeStyle: 'short'
                                                })}
                                            </Typography>
                                        </TableCell>
                                        <TableCell>
                                            <Typography variant="body2" fontWeight="bold">
                                                {trade.symbol}
                                            </Typography>
                                        </TableCell>
                                        <TableCell>
                                            <Box display="flex" alignItems="center" gap={0.5}>
                                                {trade.direction === 'LONG' ? (
                                                    <TrendingUpIcon fontSize="small" color="success" />
                                                ) : (
                                                    <TrendingDownIcon fontSize="small" color="error" />
                                                )}
                                                <Typography variant="body2">
                                                    {trade.direction}
                                                </Typography>
                                            </Box>
                                        </TableCell>
                                        <TableCell align="right">
                                            <Typography variant="body2">
                                                {formatCurrency(trade.entryPrice)}
                                            </Typography>
                                        </TableCell>
                                        <TableCell align="right">
                                            <Typography variant="body2">
                                                {formatCurrency(trade.exitPrice || 0)}
                                            </Typography>
                                        </TableCell>
                                        <TableCell align="right">
                                            <Typography variant="body2">
                                                {trade.quantity}
                                            </Typography>
                                        </TableCell>
                                        <TableCell>
                                            <Chip
                                                label={statusInfo.text}
                                                color={statusInfo.color}
                                                size="small"
                                            />
                                        </TableCell>
                                        <TableCell align="right">
                                            <Typography
                                                variant="body2"
                                                fontWeight="bold"
                                                sx={{
                                                    color: pnl >= 0 ? 'success.main' : 'error.main'
                                                }}
                                            >
                                                {formatCurrency(pnl)}
                                            </Typography>
                                        </TableCell>
                                        <TableCell align="right">
                                            <Typography
                                                variant="body2"
                                                fontWeight="bold"
                                                sx={{
                                                    color: pnlPercent >= 0 ? 'success.main' : 'error.main'
                                                }}
                                            >
                                                {formatPercent(pnlPercent)}
                                            </Typography>
                                        </TableCell>
                                        <TableCell align="center">
                                            <IconButton size="small" onClick={(e) => {
                                                e.stopPropagation();
                                                onTradeClick?.(trade);
                                            }}>
                                                <InfoIcon fontSize="small" />
                                            </IconButton>
                                        </TableCell>
                                    </TableRow>
                                );
                            })
                        )}
                    </TableBody>
                </Table>
            </TableContainer>
        </Box>
    );
}
