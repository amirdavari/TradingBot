import Box from '@mui/material/Box';
import Paper from '@mui/material/Paper';
import Typography from '@mui/material/Typography';
import Divider from '@mui/material/Divider';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import ListItemButton from '@mui/material/ListItemButton';
import ListItemText from '@mui/material/ListItemText';
import LoadingSpinner from './LoadingSpinner';
import { ScoreBadge } from './Badge';
import { useWatchlist } from '../hooks/useWatchlist';
import { useScanner } from '../hooks/useScanner';

interface WatchlistPanelProps {
    selectedSymbol: string;
    onSymbolChange: (symbol: string) => void;
}

export default function WatchlistPanel({ selectedSymbol, onSymbolChange }: WatchlistPanelProps) {
    const { watchlist, loading } = useWatchlist();
    const symbols = watchlist.map((item) => item.symbol);
    const { scanResults } = useScanner(symbols, symbols.length > 0);

    // Create a lookup map for quick result access
    const resultMap = new Map(scanResults.map((result) => [result.symbol, result]));

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
                            return (
                                <ListItem 
                                    key={item.symbol} 
                                    disablePadding
                                    secondaryAction={
                                        result && !result.hasError ? (
                                            <ScoreBadge score={result.score} />
                                        ) : null
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
