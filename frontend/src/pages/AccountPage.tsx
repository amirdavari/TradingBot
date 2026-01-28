import { useState, useEffect, useCallback } from 'react';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import Grid from '@mui/material/Grid';
import Card from '@mui/material/Card';
import CardContent from '@mui/material/CardContent';
import Button from '@mui/material/Button';
import Divider from '@mui/material/Divider';
import AccountBalanceWalletIcon from '@mui/icons-material/AccountBalanceWallet';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import MoneyIcon from '@mui/icons-material/Money';
import RestartAltIcon from '@mui/icons-material/RestartAlt';
import LoadingSpinner from '../components/LoadingSpinner';
import ErrorAlert from '../components/ErrorAlert';
import RiskSettingsDialog from '../components/RiskSettingsDialog';
import { getAccount, resetAccount } from '../api/tradingApi';
import type { Account } from '../models';
import { useSignalRAccountUpdate } from '../hooks/useSignalR';

export default function AccountPage() {
    const [account, setAccount] = useState<Account | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [resetting, setResetting] = useState(false);

    const fetchAccount = async () => {
        try {
            setError(null);
            const data = await getAccount();
            setAccount(data);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to fetch account');
            console.error('Error fetching account:', err);
        } finally {
            setLoading(false);
        }
    };

    // Real-time account updates via SignalR (replaces polling)
    const handleAccountUpdate = useCallback((updatedAccount: Account) => {
        console.log('[AccountPage] Received account update via SignalR:', updatedAccount);
        setAccount(updatedAccount);
    }, []);

    useSignalRAccountUpdate(handleAccountUpdate);

    useEffect(() => {
        fetchAccount();
    }, []);

    const handleReset = async () => {
        if (!confirm('Are you sure you want to reset your account? This will close all positions and reset to initial balance.')) {
            return;
        }

        setResetting(true);
        try {
            const data = await resetAccount();
            setAccount(data);
            setError(null);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to reset account');
            console.error('Error resetting account:', err);
        } finally {
            setResetting(false);
        }
    };

    const formatCurrency = (value: number) => {
        return new Intl.NumberFormat('de-DE', {
            style: 'currency',
            currency: 'EUR'
        }).format(value);
    };

    const formatPercentage = (current: number, initial: number) => {
        const percentage = ((current - initial) / initial) * 100;
        return `${percentage >= 0 ? '+' : ''}${percentage.toFixed(2)}%`;
    };

    const getPerformanceColor = (current: number, initial: number) => {
        if (current > initial) return 'success.main';
        if (current < initial) return 'error.main';
        return 'text.secondary';
    };

    if (loading) {
        return (
            <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '60vh' }}>
                <LoadingSpinner />
            </Box>
        );
    }

    if (error) {
        return (
            <Box sx={{ p: 3 }}>
                <ErrorAlert error={error} />
            </Box>
        );
    }

    if (!account) {
        return (
            <Box sx={{ p: 3 }}>
                <Typography variant="h6" color="text.secondary">
                    No account data available
                </Typography>
            </Box>
        );
    }

    const allocatedCapital = account.balance - account.availableCash;
    const unrealizedPnL = account.equity - account.balance;
    const totalPnL = account.equity - account.initialBalance;

    return (
        <Box sx={{ p: 3 }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
                <Typography variant="h4">
                    Account / Depot
                </Typography>
                <Box sx={{ display: 'flex', gap: 2 }}>
                    <RiskSettingsDialog />
                    <Button
                        variant="outlined"
                        color="error"
                        startIcon={<RestartAltIcon />}
                        onClick={handleReset}
                        disabled={resetting}
                    >
                        Reset Account
                    </Button>
                </Box>
            </Box>

            {/* Main Account Cards */}
            <Grid container spacing={3} sx={{ mb: 3 }}>
                {/* Balance Card */}
                <Grid size={{ xs: 12, md: 4 }}>
                    <Card elevation={2}>
                        <CardContent>
                            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                                <AccountBalanceWalletIcon color="primary" sx={{ mr: 1 }} />
                                <Typography variant="h6">Balance</Typography>
                            </Box>
                            <Typography variant="h4" sx={{ mb: 1 }}>
                                {formatCurrency(account.balance)}
                            </Typography>
                            <Typography variant="body2" color="text.secondary">
                                Initial: {formatCurrency(account.initialBalance)}
                            </Typography>
                        </CardContent>
                    </Card>
                </Grid>

                {/* Equity Card */}
                <Grid size={{ xs: 12, md: 4 }}>
                    <Card elevation={2}>
                        <CardContent>
                            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                                <TrendingUpIcon color="success" sx={{ mr: 1 }} />
                                <Typography variant="h6">Equity</Typography>
                            </Box>
                            <Typography variant="h4" sx={{ mb: 1 }}>
                                {formatCurrency(account.equity)}
                            </Typography>
                            <Typography
                                variant="body2"
                                sx={{ color: getPerformanceColor(account.equity, account.initialBalance) }}
                            >
                                {formatPercentage(account.equity, account.initialBalance)}
                            </Typography>
                        </CardContent>
                    </Card>
                </Grid>

                {/* Available Cash Card */}
                <Grid size={{ xs: 12, md: 4 }}>
                    <Card elevation={2}>
                        <CardContent>
                            <Box sx={{ display: 'flex', alignItems: 'center', mb: 2 }}>
                                <MoneyIcon color="info" sx={{ mr: 1 }} />
                                <Typography variant="h6">Available Cash</Typography>
                            </Box>
                            <Typography variant="h4" sx={{ mb: 1 }}>
                                {formatCurrency(account.availableCash)}
                            </Typography>
                            <Typography variant="body2" color="text.secondary">
                                {((account.availableCash / account.balance) * 100).toFixed(1)}% of balance
                            </Typography>
                        </CardContent>
                    </Card>
                </Grid>
            </Grid>

            {/* Detailed Statistics */}
            <Paper sx={{ p: 3 }}>
                <Typography variant="h6" gutterBottom>
                    Account Details
                </Typography>
                <Divider sx={{ mb: 2 }} />

                <Grid container spacing={2}>
                    <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                        <Typography variant="body2" color="text.secondary">
                            Allocated Capital
                        </Typography>
                        <Typography variant="h6">
                            {formatCurrency(allocatedCapital)}
                        </Typography>
                    </Grid>

                    <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                        <Typography variant="body2" color="text.secondary">
                            Unrealized P&L
                        </Typography>
                        <Typography
                            variant="h6"
                            sx={{ color: unrealizedPnL >= 0 ? 'success.main' : 'error.main' }}
                        >
                            {formatCurrency(unrealizedPnL)}
                        </Typography>
                    </Grid>

                    <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                        <Typography variant="body2" color="text.secondary">
                            Total P&L
                        </Typography>
                        <Typography
                            variant="h6"
                            sx={{ color: getPerformanceColor(account.equity, account.initialBalance) }}
                        >
                            {formatCurrency(totalPnL)}
                        </Typography>
                    </Grid>

                    <Grid size={{ xs: 12, sm: 6, md: 3 }}>
                        <Typography variant="body2" color="text.secondary">
                            Last Updated
                        </Typography>
                        <Typography variant="h6" sx={{ fontSize: '1rem' }}>
                            {new Date(account.updatedAt).toLocaleString('de-DE')}
                        </Typography>
                    </Grid>
                </Grid>
            </Paper>
        </Box>
    );
}
