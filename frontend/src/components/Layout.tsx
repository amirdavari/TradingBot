import type { ReactNode } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import Box from '@mui/material/Box';
import AppBar from '@mui/material/AppBar';
import Toolbar from '@mui/material/Toolbar';
import Typography from '@mui/material/Typography';
import Button from '@mui/material/Button';
import DashboardIcon from '@mui/icons-material/Dashboard';
import AccountBalanceWalletIcon from '@mui/icons-material/AccountBalanceWallet';
import HistoryIcon from '@mui/icons-material/History';
import SettingsIcon from '@mui/icons-material/Settings';
import SimulationIndicator from './SimulationIndicator';

interface LayoutProps {
    children: ReactNode;
}

export default function Layout({ children }: LayoutProps) {
    const navigate = useNavigate();
    const location = useLocation();

    const menuItems = [
        { text: 'Dashboard', icon: <DashboardIcon />, path: '/' },
        { text: 'Account', icon: <AccountBalanceWalletIcon />, path: '/account' },
        { text: 'History', icon: <HistoryIcon />, path: '/history' },
        { text: 'Simulation', icon: <SettingsIcon />, path: '/simulation' },
    ];

    return (
        <Box sx={{ display: 'flex', flexDirection: 'column', height: '100vh', width: '100vw', overflow: 'hidden' }}>
            <AppBar position="static" sx={{ flexShrink: 0 }}>
                <Toolbar sx={{ gap: 1 }}>
                    <Typography
                        variant="h6"
                        noWrap
                        component="div"
                        sx={{
                            fontWeight: 700,
                            mr: 3,
                            cursor: 'pointer',
                            '&:hover': { opacity: 0.8 }
                        }}
                        onClick={() => navigate('/')}
                    >
                        AI Broker
                    </Typography>

                    <Box sx={{ display: 'flex', gap: 0.5 }}>
                        {menuItems.map((item) => (
                            <Button
                                key={item.text}
                                color="inherit"
                                startIcon={item.icon}
                                onClick={() => navigate(item.path)}
                                sx={{
                                    textTransform: 'none',
                                    px: 2,
                                    borderRadius: 1,
                                    bgcolor: location.pathname === item.path ? 'rgba(255,255,255,0.15)' : 'transparent',
                                    '&:hover': {
                                        bgcolor: location.pathname === item.path ? 'rgba(255,255,255,0.2)' : 'rgba(255,255,255,0.1)',
                                    },
                                }}
                            >
                                {item.text}
                            </Button>
                        ))}
                    </Box>

                    <Box sx={{ flexGrow: 1 }} />

                    <SimulationIndicator />
                </Toolbar>
            </AppBar>

            <Box
                component="main"
                sx={{
                    flexGrow: 1,
                    overflow: 'hidden',
                    height: '100%',
                }}
            >
                {children}
            </Box>
        </Box>
    );
}
