import { useState } from 'react';
import Box from '@mui/material/Box';
import Paper from '@mui/material/Paper';
import Typography from '@mui/material/Typography';
import Divider from '@mui/material/Divider';
import Button from '@mui/material/Button';
import TextField from '@mui/material/TextField';
import Chip from '@mui/material/Chip';
import Stack from '@mui/material/Stack';
import List from '@mui/material/List';
import ListItem from '@mui/material/ListItem';
import LinearProgress from '@mui/material/LinearProgress';
import type { TradeSignal } from '../models';

interface TradeSetupPanelProps {
    signal: TradeSignal | null;
    symbol: string;
    timeframe: number;
    onBuyTrade: (investAmount: number) => void;
    onSellTrade: (investAmount: number) => void;
}

export default function TradeSetupPanel({ 
    signal, 
    symbol, 
    timeframe, 
    onBuyTrade, 
    onSellTrade 
}: TradeSetupPanelProps) {
    const [investAmount, setInvestAmount] = useState<number>(1000);

    const handleBuy = () => {
        onBuyTrade(investAmount);
    };

    const handleSell = () => {
        onSellTrade(investAmount);
    };

    // Calculate Risk/Reward Ratio: reward / risk (how much reward per unit of risk)
    const riskReward = signal && signal.entry > 0 && signal.stopLoss > 0 && signal.takeProfit > 0
        ? (() => {
            const risk = Math.abs(signal.entry - signal.stopLoss);
            const reward = Math.abs(signal.takeProfit - signal.entry);
            const ratio = reward / risk;
            return `1:${ratio.toFixed(2)}`;
          })()
        : 'N/A';

    // Calculate estimated risk/reward in EUR
    const calculateEstimatedRiskReward = () => {
        if (!signal || signal.entry <= 0 || signal.stopLoss <= 0) {
            return { risk: 0, reward: 0 };
        }

        const riskPercentage = Math.abs((signal.stopLoss - signal.entry) / signal.entry);
        const rewardPercentage = Math.abs((signal.takeProfit - signal.entry) / signal.entry);

        return {
            risk: investAmount * riskPercentage,
            reward: investAmount * rewardPercentage
        };
    };

    const { risk, reward } = calculateEstimatedRiskReward();

    return (
        <Paper sx={{ width: '100%', display: 'flex', flexDirection: 'column', overflow: 'hidden', height: '100%' }}>
            <Box sx={{ p: 2, pb: 1, flexShrink: 0 }}>
                <Typography variant="h6">Trade Setup</Typography>
                <Divider sx={{ mt: 1 }} />
            </Box>
            <Box sx={{ flexGrow: 1, overflow: 'auto', px: 2, pb: 2 }}>
                {signal ? (
                    <Stack spacing={1.5}>
                        {/* Direction (Read-Only) */}
                        <Box>
                            <Stack direction="row" justifyContent="space-between" alignItems="center">
                                <Typography variant="caption" color="text.secondary">Direction:</Typography>
                                <Chip 
                                    label={signal.direction}
                                    color={signal.direction === 'LONG' ? 'success' : signal.direction === 'SHORT' ? 'error' : 'default'}
                                    size="small"
                                    sx={{ fontWeight: 'bold' }}
                                />
                            </Stack>
                        </Box>

                        {/* Entry Price (Read-Only) */}
                        <Box>
                            <Stack direction="row" justifyContent="space-between" alignItems="baseline">
                                <Typography variant="caption" color="text.secondary">Entry:</Typography>
                                <Typography variant="body1" fontWeight="bold">${signal.entry.toFixed(2)}</Typography>
                            </Stack>
                        </Box>

                        {/* Stop Loss (Read-Only) */}
                        <Box>
                            <Stack direction="row" justifyContent="space-between" alignItems="baseline">
                                <Typography variant="caption" color="text.secondary">Stop Loss:</Typography>
                                <Typography variant="body1" fontWeight="bold" color="error.main">${signal.stopLoss.toFixed(2)}</Typography>
                            </Stack>
                        </Box>

                        {/* Take Profit (Read-Only) */}
                        <Box>
                            <Stack direction="row" justifyContent="space-between" alignItems="baseline">
                                <Typography variant="caption" color="text.secondary">Take Profit:</Typography>
                                <Typography variant="body1" fontWeight="bold" color="success.main">${signal.takeProfit.toFixed(2)}</Typography>
                            </Stack>
                        </Box>

                        {/* Risk/Reward Ratio */}
                        <Box>
                            <Stack direction="row" justifyContent="space-between" alignItems="baseline">
                                <Typography variant="caption" color="text.secondary">Risk/Reward:</Typography>
                                <Typography variant="body1" fontWeight="bold">{riskReward}</Typography>
                            </Stack>
                        </Box>

                        {/* Confidence */}
                        <Box>
                            <Stack direction="row" justifyContent="space-between" alignItems="center" spacing={1} mb={0.5}>
                                <Typography variant="caption" color="text.secondary">Confidence:</Typography>
                                <Typography variant="body2" fontWeight="bold">{signal.confidence}%</Typography>
                            </Stack>
                            <LinearProgress 
                                variant="determinate" 
                                value={signal.confidence} 
                                sx={{ 
                                    height: 6, 
                                    borderRadius: 1,
                                    '& .MuiLinearProgress-bar': {
                                        backgroundColor: signal.confidence >= 70 ? 'success.main' : signal.confidence >= 50 ? 'warning.main' : 'error.main'
                                    }
                                }}
                            />
                        </Box>

                        {/* Reasons */}
                        <Box>
                            <Typography variant="caption" color="text.secondary" gutterBottom display="block">
                                Reasons:
                            </Typography>
                            <List dense disablePadding>
                                {signal.reasons.map((reason, index) => (
                                    <ListItem key={index} sx={{ py: 0.25, px: 0 }}>
                                        <Typography variant="body2">• {reason}</Typography>
                                    </ListItem>
                                ))}
                            </List>
                        </Box>

                        <Divider />

                        {/* Invest Amount (User Input) */}
                        <Box>
                            <TextField
                                label="Invest Amount (€)"
                                type="number"
                                value={investAmount}
                                onChange={(e) => setInvestAmount(Number(e.target.value))}
                                size="small"
                                fullWidth
                                inputProps={{ min: 0, step: 100 }}
                            />
                        </Box>

                        {/* Estimated Risk/Reward in EUR */}
                        <Box>
                            <Stack direction="row" justifyContent="space-between">
                                <Box>
                                    <Typography variant="caption" color="text.secondary">Est. Risk:</Typography>
                                    <Typography variant="body1" color="error.main" fontWeight="bold">
                                        -€{risk.toFixed(2)}
                                    </Typography>
                                </Box>
                                <Box>
                                    <Typography variant="caption" color="text.secondary">Est. Reward:</Typography>
                                    <Typography variant="body1" color="success.main" fontWeight="bold">
                                        +€{reward.toFixed(2)}
                                    </Typography>
                                </Box>
                            </Stack>
                        </Box>

                        {/* Action Buttons */}
                        <Stack spacing={1}>
                            <Button 
                                variant="contained" 
                                color="success" 
                                fullWidth
                                onClick={handleBuy}
                                size="large"
                                disabled={signal.direction !== 'LONG'}
                            >
                                Kaufen
                            </Button>
                            <Button 
                                variant="contained" 
                                color="error" 
                                fullWidth
                                onClick={handleSell}
                                size="large"
                                disabled={signal.direction !== 'SHORT'}
                            >
                                Verkaufen
                            </Button>
                        </Stack>
                    </Stack>
                ) : (
                    <Typography variant="body2" color="text.secondary">
                        No signal available
                    </Typography>
                )}
            </Box>
        </Paper>
    );
}
