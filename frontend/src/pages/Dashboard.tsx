import { useState } from 'react';
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

export default function Dashboard() {
    const [timeframe, setTimeframe] = useState('5m');
    const [selectedSymbol, setSelectedSymbol] = useState('ABC');

    const watchlist = [
        { symbol: 'ABC', status: 'bullish' },
        { symbol: 'XYZ', status: 'neutral' },
        { symbol: 'DEF', status: 'bearish' },
        { symbol: 'LMN', status: 'bullish' },
        { symbol: 'QRS', status: 'bearish' },
    ];

    const mockSignal = {
        direction: 'LONG',
        entry: 150.25,
        stopLoss: 148.50,
        takeProfit: 155.00,
        confidence: 80,
        reasons: [
            'Preis über VWAP',
            'Hohes Volumen',
            'Positive News',
            'Bullish Trend'
        ]
    };

    const news = [
        { headline: 'Stock hits new high on earnings beat', sentiment: 'positive' },
        { headline: 'Market consolidation continues', sentiment: 'neutral' },
        { headline: 'Concerns over supply chain issues', sentiment: 'negative' },
    ];

    const getStatusColor = (status: string) => {
        switch (status) {
            case 'bullish': return 'success';
            case 'neutral': return 'warning';
            case 'bearish': return 'error';
            default: return 'default';
        }
    };

    const getSentimentIcon = (sentiment: string) => {
        switch (sentiment) {
            case 'positive': return <SentimentSatisfiedIcon color="success" />;
            case 'neutral': return <SentimentNeutralIcon color="warning" />;
            case 'negative': return <SentimentDissatisfiedIcon color="error" />;
            default: return <SentimentNeutralIcon />;
        }
    };

    const riskReward = ((mockSignal.takeProfit - mockSignal.entry) / (mockSignal.entry - mockSignal.stopLoss)).toFixed(1);

    return (
        <Box sx={{ width: '100%', height: '100%', display: 'flex', flexDirection: 'column', gap: 0, p: 0 }}>
            {/* Header */}
            <Paper sx={{ p: 1.5, flexShrink: 0 }}>
                <Stack direction="row" spacing={2} alignItems="center">
                    <TextField
                        placeholder="Search symbol..."
                        size="small"
                        sx={{ flexGrow: 1, maxWidth: 300 }}
                    />
                    <ToggleButtonGroup
                        value={timeframe}
                        exclusive
                        onChange={(_, value) => value && setTimeframe(value)}
                        size="small"
                    >
                        <ToggleButton value="1m">1m</ToggleButton>
                        <ToggleButton value="5m">5m</ToggleButton>
                        <ToggleButton value="15m">15m</ToggleButton>
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
                        <List dense>
                            {watchlist.map((item) => (
                                <ListItem key={item.symbol} disablePadding>
                                    <ListItemButton
                                        selected={selectedSymbol === item.symbol}
                                        onClick={() => setSelectedSymbol(item.symbol)}
                                    >
                                        <ListItemText primary={item.symbol} />
                                        <Box
                                            sx={{
                                                width: 12,
                                                height: 12,
                                                borderRadius: '50%',
                                                bgcolor: getStatusColor(item.status) === 'success' ? 'success.main' :
                                                    getStatusColor(item.status) === 'warning' ? 'warning.main' : 'error.main'
                                            }}
                                        />
                                    </ListItemButton>
                                </ListItem>
                            ))}
                        </List>
                    </Paper>
                </Grid>

                {/* Chart Area */}
                <Grid size={7} sx={{ display: 'flex', flexDirection: 'column', minHeight: 0 }}>
                    <Paper sx={{ height: '100%', display: 'flex', flexDirection: 'column', overflow: 'hidden' }}>
                        <Box sx={{ flexGrow: 1, display: 'flex', alignItems: 'center', justifyContent: 'center', bgcolor: 'background.default', position: 'relative', minHeight: 0 }}>
                            <Typography variant="h6" color="text.secondary">
                                Chart Placeholder
                            </Typography>
                            <Chip
                                label={mockSignal.direction}
                                color={mockSignal.direction === 'LONG' ? 'success' : 'error'}
                                sx={{ position: 'absolute', top: 16, right: 16 }}
                            />
                        </Box>

                        {/* News Section */}
                        <Box sx={{ p: 2, borderTop: 1, borderColor: 'divider', flexShrink: 0, maxHeight: '30%', overflow: 'auto' }}>
                            <Typography variant="h6" gutterBottom>News</Typography>
                            <Stack spacing={1}>
                                {news.map((item, index) => (
                                    <Stack key={index} direction="row" spacing={1} alignItems="center">
                                        {getSentimentIcon(item.sentiment)}
                                        <Typography variant="body2">{item.headline}</Typography>
                                    </Stack>
                                ))}
                            </Stack>
                        </Box>
                    </Paper>
                </Grid>

                {/* Trade Setup */}
                <Grid size={3} sx={{ display: 'flex', flexDirection: 'column', minHeight: 0 }}>
                    <Paper sx={{ height: '100%', overflow: 'auto', p: 2, display: 'flex', flexDirection: 'column' }}>
                        <Typography variant="h6" gutterBottom>Trade Setup</Typography>
                        <Divider sx={{ mb: 2 }} />

                        <Stack spacing={2}>
                            <Box>
                                <Typography variant="caption" color="text.secondary">Direction:</Typography>
                                <ToggleButtonGroup
                                    value={mockSignal.direction}
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
                                <Typography variant="h6">{mockSignal.entry.toFixed(2)}</Typography>
                            </Box>

                            <Box>
                                <Typography variant="caption" color="text.secondary">Stop Loss:</Typography>
                                <Typography variant="h6">{mockSignal.stopLoss.toFixed(2)}</Typography>
                            </Box>

                            <Box>
                                <Typography variant="caption" color="text.secondary">Take Profit:</Typography>
                                <Typography variant="h6">{mockSignal.takeProfit.toFixed(2)}</Typography>
                            </Box>

                            <Box>
                                <Typography variant="caption" color="text.secondary">Risk/Reward:</Typography>
                                <Typography variant="h6">{riskReward}</Typography>
                            </Box>

                            <Box>
                                <Typography variant="caption" color="text.secondary">Confidence:</Typography>
                                <Typography variant="h6">{mockSignal.confidence}%</Typography>
                            </Box>

                            <Box>
                                <Typography variant="caption" color="text.secondary">Reasons:</Typography>
                                <List dense>
                                    {mockSignal.reasons.map((reason, index) => (
                                        <ListItem key={index} sx={{ py: 0.5, px: 0 }}>
                                            <Typography variant="body2">• {reason}</Typography>
                                        </ListItem>
                                    ))}
                                </List>
                            </Box>
                        </Stack>
                    </Paper>
                </Grid>
            </Grid>
        </Box>
    );
}
