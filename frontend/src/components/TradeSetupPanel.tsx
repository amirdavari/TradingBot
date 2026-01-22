import { useState, useEffect } from 'react';
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
import CircularProgress from '@mui/material/CircularProgress';
import type { TradeSignal, RiskCalculation } from '../models';
import { calculateRisk } from '../api/tradingApi';

interface TradeSetupPanelProps {
    signal: TradeSignal | null;
    symbol: string;
    timeframe: number;
    availableCash: number;
    onBuyTrade: (signal: TradeSignal, riskCalc: RiskCalculation, riskPercent: number) => void;
    onSellTrade: (signal: TradeSignal, riskCalc: RiskCalculation, riskPercent: number) => void;
}

export default function TradeSetupPanel({ 
    signal,
    onBuyTrade, 
    onSellTrade 
}: TradeSetupPanelProps) {
    const [riskCalc, setRiskCalc] = useState<RiskCalculation | null>(null);
    const [loadingRisk, setLoadingRisk] = useState(false);
    const [riskPercent, setRiskPercent] = useState<number>(1);
    const [lastSignalKey, setLastSignalKey] = useState<string>('');

    useEffect(() => {
        if (!signal || signal.entry <= 0 || signal.stopLoss <= 0 || signal.takeProfit <= 0) {
            setRiskCalc(null);
            return;
        }

        // Create a key from signal values to detect actual changes
        const signalKey = `${signal.symbol}-${signal.entry}-${signal.stopLoss}-${signal.takeProfit}-${riskPercent}`;
        
        // Only fetch if signal values actually changed
        if (signalKey === lastSignalKey) {
            return;
        }

        const fetchRiskCalculation = async () => {
            try {
                setLoadingRisk(true);
                const calc = await calculateRisk(
                    signal.symbol,
                    signal.entry,
                    signal.stopLoss,
                    signal.takeProfit,
                    riskPercent
                );
                setRiskCalc(calc);
                setLastSignalKey(signalKey);
            } catch (error) {
                console.error('Failed to calculate risk:', error);
                setRiskCalc(null);
            } finally {
                setLoadingRisk(false);
            }
        };

        fetchRiskCalculation();
    }, [signal, riskPercent, lastSignalKey]);

    const handleBuy = () => {
        if (!signal || !riskCalc || !riskCalc.isAllowed) return;
        onBuyTrade(signal, riskCalc, riskPercent);
    };

    const handleSell = () => {
        if (!signal || !riskCalc || !riskCalc.isAllowed) return;
        onSellTrade(signal, riskCalc, riskPercent);
    };

    const riskReward = riskCalc ? `1:${riskCalc.riskRewardRatio.toFixed(2)}` : 'N/A';

    return (
        <Paper sx={{ width: '100%', display: 'flex', flexDirection: 'column', overflow: 'hidden', height: '100%' }}>
            <Box sx={{ p: 1.5, pb: 0.5, flexShrink: 0 }}>
                <Typography variant="h6">Trade Setup</Typography>
                <Divider sx={{ mt: 0.5 }} />
            </Box>
            <Box sx={{ flexGrow: 1, overflow: 'auto', px: 1.5, pb: 1.5 }}>
                {signal ? (
                    <Stack spacing={1}>
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
                            <Stack direction="row" justifyContent="space-between" alignItems="center" spacing={1} mb={0.25}>
                                <Typography variant="caption" color="text.secondary">Confidence:</Typography>
                                <Typography variant="body2" fontWeight="bold">{signal.confidence}%</Typography>
                            </Stack>
                            <LinearProgress 
                                variant="determinate" 
                                value={signal.confidence} 
                                sx={{ 
                                    height: 3, 
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
                                    <ListItem key={index} sx={{ py: 0, px: 0 }}>
                                        <Typography variant="body2">• {reason}</Typography>
                                    </ListItem>
                                ))}
                            </List>
                        </Box>

                        <Divider />

                        {/* Risk Percent Input */}
                        <Box>
                            <Stack direction="row" justifyContent="space-between" alignItems="center">
                                <Typography variant="caption" color="text.secondary">
                                    Risk per Trade:
                                </Typography>
                                <Stack direction="row" spacing={0.5} alignItems="center">
                                    <TextField
                                        type="number"
                                        value={riskPercent}
                                        onChange={(e) => setRiskPercent(Number(e.target.value))}
                                        size="small"
                                        sx={{ 
                                            width: '60px',
                                            '& input': {
                                                textAlign: 'left',
                                                fontWeight: 'bold',
                                                fontSize: '0.9rem',
                                                padding: '6px 8px'
                                            }
                                        }}
                                        inputProps={{ min: 0.1, max: 5, step: 0.1 }}
                                    />
                                    <Typography variant="body2" fontWeight="bold" sx={{ minWidth: '15px' }}>%</Typography>
                                </Stack>
                            </Stack>
                        </Box>

                        {loadingRisk ? (
                            <Box display="flex" justifyContent="center" py={1}>
                                <CircularProgress size={20} />
                            </Box>
                        ) : riskCalc ? (
                            <>
                                {/* Position Size (Read-Only) */}
                                <Box>
                                    <Stack direction="row" justifyContent="space-between" alignItems="baseline">
                                        <Typography variant="caption" color="text.secondary">Position Size:</Typography>
                                        <Typography variant="body1" fontWeight="bold">{riskCalc.positionSize.toFixed(4)} shares</Typography>
                                    </Stack>
                                </Box>

                                {/* Invest Amount (Read-Only) */}
                                <Box>
                                    <Stack direction="row" justifyContent="space-between" alignItems="baseline">
                                        <Typography variant="caption" color="text.secondary">Invest Amount:</Typography>
                                        <Typography variant="body1" fontWeight="bold">€{riskCalc.investAmount.toFixed(2)}</Typography>
                                    </Stack>
                                </Box>

                                {/* Risk/Reward in EUR */}
                                <Box>
                                    <Stack direction="row" justifyContent="space-between">
                                        <Box>
                                            <Typography variant="caption" color="text.secondary">Risk Amount:</Typography>
                                            <Typography variant="body1" color="error.main" fontWeight="bold">
                                                -€{riskCalc.riskAmount.toFixed(2)}
                                            </Typography>
                                        </Box>
                                        <Box>
                                            <Typography variant="caption" color="text.secondary">Reward Amount:</Typography>
                                            <Typography variant="body1" color="success.main" fontWeight="bold">
                                                +€{riskCalc.rewardAmount.toFixed(2)}
                                            </Typography>
                                        </Box>
                                    </Stack>
                                </Box>


                                {/* Cash-Limited Position Info */}
                                {riskCalc.isAllowed && riskCalc.limitingFactor === 'CASH' && (
                                    <Box sx={{ p: 0.75, bgcolor: 'warning.dark', borderRadius: 1 }}>
                                        <Typography variant="caption" color="warning.light" display="block" fontWeight="bold">
                                            💡 Cash-begrenzte Position
                                        </Typography>
                                        <Typography variant="caption" color="warning.light" display="block">
                                            Risiko-Budget: {(riskCalc.riskUtilization * 100).toFixed(0)}% genutzt
                                        </Typography>
                                    </Box>
                                )}

                                {/* Capital-Limited Position Info */}
                                {riskCalc.isAllowed && riskCalc.limitingFactor === 'CAPITAL' && (
                                    <Box sx={{ p: 0.75, bgcolor: 'info.dark', borderRadius: 1 }}>
                                        <Typography variant="caption" color="info.light" display="block" fontWeight="bold">
                                            📊 Kapital-Limit aktiv
                                        </Typography>
                                        <Typography variant="caption" color="info.light" display="block">
                                            Max. {riskCalc.maxCapitalPercent}% des Kapitals pro Trade
                                        </Typography>
                                    </Box>
                                )}
                            </>
                        ) : null}

                        {/* Action Button */}
                        <Button 
                            variant="contained" 
                            color={signal.direction === 'LONG' ? 'success' : 'error'}
                            fullWidth
                            onClick={signal.direction === 'LONG' ? handleBuy : handleSell}
                            size="large"
                            disabled={
                                signal.direction === 'NONE' || 
                                !riskCalc || 
                                !riskCalc.isAllowed || 
                                loadingRisk
                            }
                        >
                            Open {signal.direction} Trade {riskCalc && `(€${riskCalc.investAmount.toFixed(0)})`}
                        </Button>
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
