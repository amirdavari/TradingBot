import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { ThemeProvider, createTheme, alpha } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import Layout from './components/Layout';
import Dashboard from './pages/Dashboard';
import AccountPage from './pages/AccountPage';
import TradeHistoryPage from './pages/TradeHistoryPage';
import SimulationControlPanel from './pages/SimulationControlPanel';

// Modern Trading Theme - Dark with green/red accents
const tradingTheme = createTheme({
  palette: {
    mode: 'dark',
    primary: {
      main: '#2196F3',       // Trading blue
      light: '#64B5F6',
      dark: '#1565C0',
    },
    secondary: {
      main: '#9C27B0',       // Accent purple
      light: '#BA68C8',
      dark: '#7B1FA2',
    },
    success: {
      main: '#00C853',       // Bright green for profit/buy
      light: '#69F0AE',
      dark: '#00A040',
    },
    error: {
      main: '#FF1744',       // Bright red for loss/sell
      light: '#FF5252',
      dark: '#D50000',
    },
    warning: {
      main: '#FF9100',       // Orange for warnings
      light: '#FFB74D',
      dark: '#E65100',
    },
    info: {
      main: '#00BCD4',       // Cyan for info/neutral
      light: '#4DD0E1',
      dark: '#00838F',
    },
    background: {
      default: '#0D1117',    // Deep dark background (GitHub-like)
      paper: '#161B22',      // Slightly lighter for cards/panels
    },
    text: {
      primary: '#E6EDF3',    // Bright white-ish
      secondary: '#8B949E',  // Muted gray
    },
    divider: alpha('#30363D', 0.8),
  },
  typography: {
    fontFamily: '"Inter", "Roboto", "Helvetica", "Arial", sans-serif',
    h1: { fontWeight: 600 },
    h2: { fontWeight: 600 },
    h3: { fontWeight: 600 },
    h4: { fontWeight: 600 },
    h5: { fontWeight: 500 },
    h6: { fontWeight: 500 },
    button: {
      textTransform: 'none',
      fontWeight: 500,
    },
  },
  shape: {
    borderRadius: 8,
  },
  components: {
    MuiCssBaseline: {
      styleOverrides: {
        body: {
          scrollbarColor: '#30363D #0D1117',
          '&::-webkit-scrollbar': {
            width: 8,
            height: 8,
          },
          '&::-webkit-scrollbar-track': {
            background: '#0D1117',
          },
          '&::-webkit-scrollbar-thumb': {
            background: '#30363D',
            borderRadius: 4,
            '&:hover': {
              background: '#484F58',
            },
          },
          '& *::-webkit-scrollbar': {
            width: 8,
            height: 8,
          },
          '& *::-webkit-scrollbar-track': {
            background: 'transparent',
          },
          '& *::-webkit-scrollbar-thumb': {
            background: '#30363D',
            borderRadius: 4,
            '&:hover': {
              background: '#484F58',
            },
          },
        },
      },
    },
    MuiAppBar: {
      styleOverrides: {
        root: {
          background: 'linear-gradient(90deg, #161B22 0%, #1C2128 100%)',
          borderBottom: '1px solid rgba(48, 54, 61, 0.8)',
          boxShadow: 'none',
        },
      },
    },
    MuiPaper: {
      styleOverrides: {
        root: {
          backgroundImage: 'none',
          border: '1px solid rgba(48, 54, 61, 0.6)',
        },
      },
    },
    MuiCard: {
      styleOverrides: {
        root: {
          backgroundImage: 'none',
          border: '1px solid rgba(48, 54, 61, 0.6)',
        },
      },
    },
    MuiButton: {
      styleOverrides: {
        root: {
          borderRadius: 6,
        },
        containedPrimary: {
          background: 'linear-gradient(135deg, #2196F3 0%, #1976D2 100%)',
          '&:hover': {
            background: 'linear-gradient(135deg, #42A5F5 0%, #2196F3 100%)',
          },
        },
        containedSuccess: {
          background: 'linear-gradient(135deg, #00C853 0%, #00A040 100%)',
          '&:hover': {
            background: 'linear-gradient(135deg, #00E676 0%, #00C853 100%)',
          },
        },
        containedError: {
          background: 'linear-gradient(135deg, #FF1744 0%, #D50000 100%)',
          '&:hover': {
            background: 'linear-gradient(135deg, #FF5252 0%, #FF1744 100%)',
          },
        },
      },
    },
    MuiChip: {
      styleOverrides: {
        root: {
          borderRadius: 6,
        },
      },
    },
    MuiTableCell: {
      styleOverrides: {
        head: {
          fontWeight: 600,
          backgroundColor: '#1C2128',
        },
      },
    },
    MuiSlider: {
      styleOverrides: {
        root: {
          '& .MuiSlider-thumb': {
            '&:hover, &.Mui-focusVisible': {
              boxShadow: '0 0 0 8px rgba(33, 150, 243, 0.16)',
            },
          },
        },
      },
    },
  },
});

function App() {
  return (
    <BrowserRouter>
      <ThemeProvider theme={tradingTheme}>
        <CssBaseline />
        <Layout>
          <Routes>
            <Route path="/" element={<Dashboard />} />
            <Route path="/account" element={<AccountPage />} />
            <Route path="/history" element={<TradeHistoryPage />} />
            <Route path="/simulation" element={<SimulationControlPanel />} />
            <Route path="/symbol/:symbol" element={<Dashboard />} />
          </Routes>
        </Layout>
      </ThemeProvider>
    </BrowserRouter>
  );
}

export default App;
