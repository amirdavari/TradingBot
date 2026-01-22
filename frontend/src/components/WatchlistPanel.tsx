import { useState } from 'react';
import Box from '@mui/material/Box';
import Paper from '@mui/material/Paper';
import Typography from '@mui/material/Typography';
import Divider from '@mui/material/Divider';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemText from '@mui/material/ListItemText';
import IconButton from '@mui/material/IconButton';
import Tooltip from '@mui/material/Tooltip';
import DeleteIcon from '@mui/icons-material/Delete';
import LoadingSpinner from './LoadingSpinner';
import { ScoreBadge, TrendBadge } from './Badge';
import { useWatchlist } from '../hooks/useWatchlist';
import { useScanner } from '../hooks/useScanner';
import { useOpenTrades } from '../hooks/useOpenTrades';

interface WatchlistPanelProps {
    selectedSymbol: string;
    onSymbolChange: (symbol: string) => void;
}

export default function WatchlistPanel({ selectedSymbol, onSymbolChange }: WatchlistPanelProps) {
    const { watchlist, loading, deleteSymbol } = useWatchlist();
    const symbols = watchlist.map((item) => item.symbol);
    // Refresh scanner results every 10 seconds (10000ms)
    const { scanResults } = useScanner(symbols, symbols.length > 0, 10000);
    const { trades } = useOpenTrades(5000);
    const [deletingSymbol, setDeletingSymbol] = useState<string | null>(null);

    // Create a lookup map for quick result access
    const resultMap = new Map(scanResults.map((result) => [result.symbol, result]));

    // Create a set of symbols with open trades
    const symbolsWithOpenTrades = new Set(trades.map(trade => trade.symbol));

    const handleDelete = async (e: React.MouseEvent, symbol: string) => {
        e.stopPropagation();

        // Check if symbol has open trades
        if (symbolsWithOpenTrades.has(symbol)) {
            alert(`Cannot remove ${symbol} from watchlist: There are open trades for this symbol. Please close all trades first.`);
            return;
        }

        if (!confirm(`Remove ${symbol} from watchlist?`)) {
            return;
        }

        setDeletingSymbol(symbol);
        try {
            await deleteSymbol(symbol);
        } catch (error) {
            console.error('Failed to delete symbol:', error);
            alert(`Failed to remove ${symbol}: ${error instanceof Error ? error.message : 'Unknown error'}`);
        } finally {
            setDeletingSymbol(null);
        }
    };

    return (
        <Paper sx={{ width: '100%', display: 'flex', flexDirection: 'column', overflow: 'hidden', height: '100%' }}>
            <Box sx={{ p: 2, pb: 1, flexShrink: 0 }}>
                <Typography variant="h6">Watchlist</Typography>
            </Box>
            <Divider />
            <Box sx={{ flexGrow: 1, overflow: 'auto' }}>
                {loading ? (
                    <Box sx={{ p: 2, display: 'flex', justifyContent: 'center' }}>
                        <LoadingSpinner />
                    </Box>
                ) : (
                    <List dense>
                        {watchlist.map((item) => {
                            const result = resultMap.get(item.symbol);
                            const hasOpenTrades = symbolsWithOpenTrades.has(item.symbol);
                            const isDeleting = deletingSymbol === item.symbol;

                            return (
                                <ListItem
                                    key={item.symbol}
                                    disablePadding
                                    secondaryAction={
                                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                                            {result && !result.hasError && (
                                                <>
                                                    <TrendBadge trend={result.trend} />
                                                    <ScoreBadge score={result.confidence} />
                                                </>
                                            )}
                                            <Tooltip
                                                title={
                                                    hasOpenTrades
                                                        ? 'Cannot remove: Symbol has open trades'
                                                        : 'Remove from watchlist'
                                                }
                                            >
                                                <span>
                                                    <IconButton
                                                        edge="end"
                                                        aria-label="delete"
                                                        size="small"
                                                        onClick={(e) => handleDelete(e, item.symbol)}
                                                        disabled={hasOpenTrades || isDeleting || loading}
                                                        sx={{
                                                            opacity: hasOpenTrades ? 0.3 : 1,
                                                            '&:hover': {
                                                                color: hasOpenTrades ? 'inherit' : 'error.main'
                                                            }
                                                        }}
                                                    >
                                                        <DeleteIcon fontSize="small" />
                                                    </IconButton>
                                                </span>
                                            </Tooltip>
                                        </Box>
                                    }
                                >
                                    <ListItemButton
                                        selected={selectedSymbol === item.symbol}
                                        onClick={() => onSymbolChange(item.symbol)}
                                    >
                                        <ListItemText primary={item.symbol} />
                                    </ListItemButton>
                                </ListItem>
                            );
                        })}
                    </List>
                )}
            </Box>
        </Paper>
    );
}
