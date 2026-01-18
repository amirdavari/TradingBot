import { BrowserRouter, Routes, Route } from 'react-router-dom';
import { ThemeProvider, createTheme } from '@mui/material/styles';
import CssBaseline from '@mui/material/CssBaseline';
import Layout from './components/Layout';
import Dashboard from './pages/Dashboard';
import ScannerPage from './pages/ScannerPage';
import AccountPage from './pages/AccountPage';
import SimulationControlPanel from './pages/SimulationControlPanel';

const theme = createTheme({
  palette: {
    mode: 'dark',
    primary: {
      main: '#1976d2',
    },
    secondary: {
      main: '#dc004e',
    },
  },
});

function App() {
  return (
    <BrowserRouter>
      <ThemeProvider theme={theme}>
        <CssBaseline />
        <Layout>
          <Routes>
            <Route path="/" element={<Dashboard />} />
            <Route path="/scanner" element={<ScannerPage />} />
            <Route path="/account" element={<AccountPage />} />
            <Route path="/simulation" element={<SimulationControlPanel />} />
            <Route path="/symbol/:symbol" element={<Dashboard />} />
          </Routes>
        </Layout>
      </ThemeProvider>
    </BrowserRouter>
  );
}

export default App;
