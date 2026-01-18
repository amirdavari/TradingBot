import Paper from '@mui/material/Paper';
import Typography from '@mui/material/Typography';
import Divider from '@mui/material/Divider';
import Stack from '@mui/material/Stack';
import Chip from '@mui/material/Chip';

interface PaperTrade {
    symbol: string;
    direction: 'LONG' | 'SHORT';
    profitLossPercent: number;
}

interface OpenPaperTradesPanelProps {
    trades: PaperTrade[];
    onTradeClick: (symbol: string) => void;
}

export default function OpenPaperTradesPanel({ trades, onTradeClick }: OpenPaperTradesPanelProps) {
    return (
        <Paper sx={{ p: 2, flexShrink: 0 }}>
            <Typography variant="h6" gutterBottom>Open Paper Trades</Typography>
            <Divider sx={{ mb: 1 }} />
            <Stack direction="row" spacing={2} flexWrap="wrap" useFlexGap>
                {trades.length === 0 ? (
                    <Typography variant="body2" color="text.secondary">
                        No open trades
                    </Typography>
                ) : (
                    trades.map((trade, index) => (
                        <Chip 
                            key={index}
                            label={`${trade.symbol} ${trade.direction} ${trade.profitLossPercent > 0 ? '+' : ''}${trade.profitLossPercent.toFixed(1)}%`}
                            color={trade.profitLossPercent >= 0 ? 'success' : 'error'}
                            onClick={() => onTradeClick(trade.symbol)}
                            clickable
                        />
                    ))
                )}
            </Stack>
        </Paper>
    );
}
