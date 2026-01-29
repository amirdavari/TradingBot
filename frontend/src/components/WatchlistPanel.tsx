import { useState, useEffect, useRef } from 'react';
import Box from '@mui/material/Box';
import Paper from '@mui/material/Paper';
import Typography from '@mui/material/Typography';
import Divider from '@mui/material/Divider';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import TableSortLabel from '@mui/material/TableSortLabel';
import IconButton from '@mui/material/IconButton';
import Tooltip from '@mui/material/Tooltip';
import TextField from '@mui/material/TextField';
import Chip from '@mui/material/Chip';
import InputAdornment from '@mui/material/InputAdornment';
import CircularProgress from '@mui/material/CircularProgress';
import DeleteIcon from '@mui/icons-material/Delete';
import AddIcon from '@mui/icons-material/Add';
import RefreshIcon from '@mui/icons-material/Refresh';
import TrendingUpIcon from '@mui/icons-material/TrendingUp';
import TrendingDownIcon from '@mui/icons-material/TrendingDown';
import TrendingFlatIcon from '@mui/icons-material/TrendingFlat';
import NewspaperIcon from '@mui/icons-material/Newspaper';
import AutoGraphIcon from '@mui/icons-material/AutoGraph';
import LoadingSpinner from './LoadingSpinner';
import { useWatchlist } from '../hooks/useWatchlist';
import { useScanner } from '../hooks/useScanner';
import { useOpenTrades } from '../hooks/useOpenTrades';
import { useSignalRScanResults } from '../hooks/useSignalR';
import { getScenarioState, type ScenarioState } from '../api/scenarioApi';

// Static company name lookup for known symbols
const COMPANY_NAMES: Record<string, string> = {
    // US Stocks
    'AAPL': 'Apple', 'MSFT': 'Microsoft', 'GOOGL': 'Alphabet', 'AMZN': 'Amazon',
    'META': 'Meta', 'NVDA': 'NVIDIA', 'TSLA': 'Tesla', 'AMD': 'AMD',
    'NFLX': 'Netflix', 'SPY': 'S&P 500 ETF', 'QQQ': 'Nasdaq ETF',
    // German DAX Stocks
    'SAP.DE': 'SAP', 'SIE.DE': 'Siemens', 'ALV.DE': 'Allianz', 'BAS.DE': 'BASF',
    'IFX.DE': 'Infineon', 'BMW.DE': 'BMW', 'MBG.DE': 'Mercedes-Benz', 'VOW3.DE': 'Volkswagen',
    'DTE.DE': 'Dt. Telekom', 'RWE.DE': 'RWE', 'EOAN.DE': 'E.ON', 'MUV2.DE': 'Munich Re',
    'CBK.DE': 'Commerzbank', 'DBK.DE': 'Deutsche Bank', 'ENR.DE': 'Siemens Energy',
    'ADS.DE': 'Adidas', 'BAYN.DE': 'Bayer', 'HEI.DE': 'Heidelberg Mat.', 'ZAL.DE': 'Zalando',
    'DB1.DE': 'Dt. Börse', 'RHM.DE': 'Rheinmetall', 'MTX.DE': 'MTU Aero', 'AIR.DE': 'Airbus',
    'SRT3.DE': 'Sartorius', 'SY1.DE': 'Symrise', 'HEN3.DE': 'Henkel', '1COV.DE': 'Covestro',
    'P911.DE': 'Porsche AG', 'VNA.DE': 'Vonovia', 'FRE.DE': 'Fresenius', 'HFG.DE': 'HelloFresh',
    'DHER.DE': 'Delivery Hero', 'TMV.DE': 'TeamViewer', 'AIXA.DE': 'Aixtron', 'S92.DE': 'SMA Solar',
    'VAR1.DE': 'Varta', 'EVT.DE': 'Evotec', 'AFX.DE': 'Carl Zeiss', 'NEM.DE': 'Nemetschek',
    'UTDI.DE': 'United Internet', 'WAF.DE': 'Siltronic', 'JEN.DE': 'Jenoptik', 'COK.DE': 'Cancom',
    'GFT.DE': 'GFT Tech', 'LPKF.DE': 'LPKF Laser', 'DMP.DE': 'Dermapharm', 'ECV.DE': 'Encavis',
    'G24.DE': 'Scout24', 'EVD.DE': 'CTS Eventim', 'BOSS.DE': 'Hugo Boss', 'RAA.DE': 'Rational',
    'GBF.DE': 'Bilfinger', 'WCH.DE': 'Wacker Chemie', 'FRA.DE': 'Fraport', 'LHA.DE': 'Lufthansa',
    'HOT.DE': 'Hochtief', 'SZG.DE': 'Salzgitter', 'SDF.DE': 'K+S', '8TRA.DE': 'Traton',
    'KBX.DE': 'Knorr-Bremse', 'BEI.DE': 'Beiersdorf', 'HNR1.DE': 'Hannover Rück', 'BNR.DE': 'Brenntag',
    'SHL.DE': 'Siemens Health.', 'FME.DE': 'Fresenius MC', 'MRK.DE': 'Merck KGaA', 'QIA.DE': 'Qiagen',
    'PAH3.DE': 'Porsche SE', 'LEG.DE': 'LEG Immobilien', 'TEG.DE': 'TAG Immobilien', 'EVK.DE': 'Evonik',
    'KGX.DE': 'KION Group', 'G1A.DE': 'GEA Group', 'TLX.DE': 'Talanx', 'RRTL.DE': 'RTL Group',
    'FNTN.DE': 'Freenet', 'SGL.DE': 'SGL Carbon', 'NOEJ.DE': 'Norma Group', 'STM.DE': 'Stabilus',
    'VOS.DE': 'Vossloh', 'SHA.DE': 'Schaeffler', 'PFV.DE': 'Pfeiffer Vacuum', 'FIE.DE': 'Fielmann',
    'COP.DE': 'Compugroup Med.', 'DWNI.DE': 'Deutsche Wohnen', 'NA9.DE': 'Nagarro', 'SOW.DE': 'Software AG',
    'HYQ.DE': 'Hypoport', 'ILM1.DE': 'Medios', 'ZIL2.DE': 'ElringKlinger', 'DUE.DE': 'Dürr',
    'SPG.DE': 'Springer Nature', 'JUN3.DE': 'Jungheinrich', 'GYC.DE': 'Grand City Prop.', 'NDA.DE': 'Aurubis',
    'SMHN.DE': 'Süss MicroTec',
};

interface WatchlistPanelProps {
    selectedSymbol: string;
    onSymbolChange: (symbol: string) => void;
    timeframe: 1 | 5 | 15;
}

export default function WatchlistPanel({ selectedSymbol, onSymbolChange, timeframe }: WatchlistPanelProps) {
    const { watchlist, loading, addSymbol, deleteSymbol } = useWatchlist();
    const symbols = watchlist.map((item) => item.symbol);
    // Scanner results - initial load via API, updates via SignalR (both use user-selected timeframe)
    const { scanResults, loading: scanLoading, reload: reloadScanner, setScanResults } = useScanner(symbols, symbols.length > 0, timeframe);
    const { trades } = useOpenTrades(); // Uses SignalR for real-time updates
    const [deletingSymbol, setDeletingSymbol] = useState<string | null>(null);
    const [newSymbol, setNewSymbol] = useState('');
    const [newCompanyName, setNewCompanyName] = useState('');
    const [isAdding, setIsAdding] = useState(false);
    const [showAddForm, setShowAddForm] = useState(false);
    const [orderBy, setOrderBy] = useState<'symbol' | 'company' | 'volume' | 'trend' | 'confidence'>('symbol');
    const [order, setOrder] = useState<'asc' | 'desc'>('asc');
    const [scenarioState, setScenarioState] = useState<ScenarioState | null>(null);

    // Load scenario state on mount
    useEffect(() => {
        getScenarioState().then(setScenarioState).catch(console.error);
    }, []);

    // Keep symbols ref up to date for SignalR callback (avoids stale closure)
    const symbolsRef = useRef(symbols);
    useEffect(() => {
        symbolsRef.current = symbols;
    }, [symbols]);

    // Listen for SignalR scanner updates - merge with existing results
    // Backend now uses user-selected timeframe stored in database
    useSignalRScanResults((results) => {
        console.log('[WatchlistPanel] Received scanner results via SignalR:', results.length);

        // Create a map of new results for quick lookup
        const newResultsMap = new Map(results.map(r => [r.symbol, r]));
        const currentSymbols = symbolsRef.current;

        // Merge: update existing symbols if we have new data, keep others
        setScanResults(prev => {
            const merged = prev.map(existing => {
                const updated = newResultsMap.get(existing.symbol);
                return updated || existing;
            });

            // Add any new symbols from SignalR that are in our watchlist but weren't in prev
            const existingSymbols = new Set(prev.map(r => r.symbol));
            const watchlistSymbolSet = new Set(currentSymbols);

            results.forEach(result => {
                if (!existingSymbols.has(result.symbol) && watchlistSymbolSet.has(result.symbol)) {
                    merged.push(result);
                }
            });

            return merged;
        });
    });

    // Create a lookup map for quick result access
    const resultMap = new Map(scanResults.map((result) => [result.symbol, result]));

    // Create a set of symbols with open trades
    const symbolsWithOpenTrades = new Set(trades.map(trade => trade.symbol));

    // Sort handler
    const handleSort = (column: 'symbol' | 'company' | 'volume' | 'trend' | 'confidence') => {
        const isAsc = orderBy === column && order === 'asc';
        setOrder(isAsc ? 'desc' : 'asc');
        setOrderBy(column);
    };

    // Sort watchlist
    const sortedWatchlist = [...watchlist].sort((a, b) => {
        const resultA = resultMap.get(a.symbol);
        const resultB = resultMap.get(b.symbol);
        let comparison = 0;

        switch (orderBy) {
            case 'symbol':
                comparison = a.symbol.localeCompare(b.symbol);
                break;
            case 'company':
                const nameA = a.companyName || COMPANY_NAMES[a.symbol] || a.symbol;
                const nameB = b.companyName || COMPANY_NAMES[b.symbol] || b.symbol;
                comparison = nameA.localeCompare(nameB);
                break;
            case 'volume':
                const volOrder = { 'HIGH': 3, 'MEDIUM': 2, 'LOW': 1, 'NONE': 0 };
                const volA = resultA?.volumeStatus ? volOrder[resultA.volumeStatus as keyof typeof volOrder] || 0 : 0;
                const volB = resultB?.volumeStatus ? volOrder[resultB.volumeStatus as keyof typeof volOrder] || 0 : 0;
                comparison = volA - volB;
                break;
            case 'trend':
                const trendOrder = { 'LONG': 1, 'NONE': 0, 'SHORT': -1 };
                const trendA = resultA?.trend ? trendOrder[resultA.trend as keyof typeof trendOrder] || 0 : 0;
                const trendB = resultB?.trend ? trendOrder[resultB.trend as keyof typeof trendOrder] || 0 : 0;
                comparison = trendA - trendB;
                break;
            case 'confidence':
                const confA = resultA?.confidence || 0;
                const confB = resultB?.confidence || 0;
                comparison = confA - confB;
                break;
        }

        return order === 'asc' ? comparison : -comparison;
    });

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

    const handleAddSymbol = async () => {
        if (!newSymbol.trim()) return;

        setIsAdding(true);
        try {
            await addSymbol(newSymbol.trim().toUpperCase(), newCompanyName.trim() || undefined);
            setNewSymbol('');
            setNewCompanyName('');
            setShowAddForm(false);
        } catch (error) {
            console.error('Failed to add symbol:', error);
            alert(`Failed to add ${newSymbol}: ${error instanceof Error ? error.message : 'Unknown error'}`);
        } finally {
            setIsAdding(false);
        }
    };

    const handleKeyPress = (e: React.KeyboardEvent) => {
        if (e.key === 'Enter') {
            handleAddSymbol();
        } else if (e.key === 'Escape') {
            setShowAddForm(false);
            setNewSymbol('');
            setNewCompanyName('');
        }
    };

    const getVolumeColor = (volumeStatus: string) => {
        switch (volumeStatus) {
            case 'HIGH': return 'success';
            case 'MEDIUM': return 'warning';
            default: return 'default';
        }
    };

    const getTrendIcon = (trend: string) => {
        switch (trend) {
            case 'LONG': return <TrendingUpIcon fontSize="small" sx={{ color: 'success.main' }} />;
            case 'SHORT': return <TrendingDownIcon fontSize="small" sx={{ color: 'error.main' }} />;
            default: return <TrendingFlatIcon fontSize="small" sx={{ color: 'text.secondary' }} />;
        }
    };

    return (
        <Paper
            sx={{
                width: '100%',
                display: 'flex',
                flexDirection: 'column',
                overflow: 'hidden',
                height: '100%',
                borderRadius: 2,
                bgcolor: 'background.paper'
            }}
            elevation={2}
        >
            {/* Header */}
            <Box sx={{ p: 2, pb: 1, flexShrink: 0, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <Typography variant="h6" sx={{ fontWeight: 600 }}>Watchlist</Typography>
                <Box sx={{ display: 'flex', gap: 0.5 }}>
                    <Tooltip title="Refresh scanner data">
                        <IconButton size="small" onClick={reloadScanner} disabled={scanLoading || symbols.length === 0}>
                            {scanLoading ? <CircularProgress size={18} /> : <RefreshIcon fontSize="small" />}
                        </IconButton>
                    </Tooltip>
                    <Tooltip title="Add symbol">
                        <IconButton size="small" onClick={() => setShowAddForm(!showAddForm)} color={showAddForm ? 'primary' : 'default'}>
                            <AddIcon fontSize="small" />
                        </IconButton>
                    </Tooltip>
                </Box>
            </Box>

            {/* Add Symbol Form */}
            {showAddForm && (
                <Box sx={{ px: 2, pb: 1, display: 'flex', flexDirection: 'column', gap: 1 }}>
                    <TextField
                        size="small"
                        placeholder="Symbol (e.g. AAPL)"
                        value={newSymbol}
                        onChange={(e) => setNewSymbol(e.target.value.toUpperCase())}
                        onKeyDown={handleKeyPress}
                        disabled={isAdding}
                        autoFocus
                        InputProps={{
                            endAdornment: (
                                <InputAdornment position="end">
                                    <IconButton
                                        size="small"
                                        onClick={handleAddSymbol}
                                        disabled={!newSymbol.trim() || isAdding}
                                        color="primary"
                                    >
                                        {isAdding ? <CircularProgress size={18} /> : <AddIcon fontSize="small" />}
                                    </IconButton>
                                </InputAdornment>
                            ),
                        }}
                        sx={{ '& .MuiOutlinedInput-root': { borderRadius: 1 } }}
                    />
                    <TextField
                        size="small"
                        placeholder="Company Name (optional)"
                        value={newCompanyName}
                        onChange={(e) => setNewCompanyName(e.target.value)}
                        onKeyDown={handleKeyPress}
                        disabled={isAdding}
                        sx={{ '& .MuiOutlinedInput-root': { borderRadius: 1 } }}
                    />
                </Box>
            )}

            <Divider />

            {/* Table */}
            <Box sx={{ flexGrow: 1, overflow: 'auto' }}>
                {loading ? (
                    <Box sx={{ p: 2, display: 'flex', justifyContent: 'center' }}>
                        <LoadingSpinner />
                    </Box>
                ) : watchlist.length === 0 ? (
                    <Box sx={{ p: 3, textAlign: 'center' }}>
                        <Typography variant="body2" color="text.secondary">
                            No symbols in watchlist
                        </Typography>
                        <Typography variant="caption" color="text.secondary">
                            Click + to add a symbol
                        </Typography>
                    </Box>
                ) : (
                    <TableContainer sx={{ maxHeight: '100%', overflow: 'auto' }}>
                        <Table size="small" stickyHeader>
                            <TableHead>
                                <TableRow>
                                    <TableCell sx={{ fontWeight: 600, bgcolor: 'background.paper', py: 0.5, fontSize: '0.75rem', whiteSpace: 'nowrap' }}>
                                        <TableSortLabel
                                            active={orderBy === 'symbol'}
                                            direction={orderBy === 'symbol' ? order : 'asc'}
                                            onClick={() => handleSort('symbol')}
                                        >
                                            Symbol
                                        </TableSortLabel>
                                    </TableCell>
                                    <TableCell sx={{ fontWeight: 600, bgcolor: 'background.paper' }}>
                                        <TableSortLabel
                                            active={orderBy === 'company'}
                                            direction={orderBy === 'company' ? order : 'asc'}
                                            onClick={() => handleSort('company')}
                                        >
                                            Company
                                        </TableSortLabel>
                                    </TableCell>
                                    <TableCell sx={{ fontWeight: 600, bgcolor: 'background.paper', py: 0.5, width: 40, fontSize: '0.75rem' }} align="center">
                                        <Tooltip title="Scenario"><AutoGraphIcon fontSize="small" /></Tooltip>
                                    </TableCell>
                                    <TableCell sx={{ fontWeight: 600, bgcolor: 'background.paper', py: 0.5, width: 70, fontSize: '0.75rem' }} align="center">
                                        <TableSortLabel
                                            active={orderBy === 'volume'}
                                            direction={orderBy === 'volume' ? order : 'asc'}
                                            onClick={() => handleSort('volume')}
                                        >
                                            Vol
                                        </TableSortLabel>
                                    </TableCell>
                                    <TableCell sx={{ fontWeight: 600, bgcolor: 'background.paper', py: 0.5, width: 40, fontSize: '0.75rem' }} align="center">
                                        <Tooltip title="Trend">
                                            <TableSortLabel
                                                active={orderBy === 'trend'}
                                                direction={orderBy === 'trend' ? order : 'asc'}
                                                onClick={() => handleSort('trend')}
                                            >
                                                T
                                            </TableSortLabel>
                                        </Tooltip>
                                    </TableCell>
                                    <TableCell sx={{ fontWeight: 600, bgcolor: 'background.paper', py: 0.5, width: 40, fontSize: '0.75rem' }} align="center">
                                        <Tooltip title="Confidence">
                                            <TableSortLabel
                                                active={orderBy === 'confidence'}
                                                direction={orderBy === 'confidence' ? order : 'asc'}
                                                onClick={() => handleSort('confidence')}
                                            >
                                                C
                                            </TableSortLabel>
                                        </Tooltip>
                                    </TableCell>
                                    <TableCell sx={{ fontWeight: 600, bgcolor: 'background.paper', py: 0.5, width: 40, fontSize: '0.75rem' }} align="center">
                                        <Tooltip title="Has News"><NewspaperIcon fontSize="small" /></Tooltip>
                                    </TableCell>
                                    <TableCell sx={{ fontWeight: 600, bgcolor: 'background.paper', py: 0.5, width: 40 }} align="right"></TableCell>
                                </TableRow>
                            </TableHead>
                            <TableBody>
                                {sortedWatchlist.map((item) => {
                                    const result = resultMap.get(item.symbol);
                                    const hasOpenTrades = symbolsWithOpenTrades.has(item.symbol);
                                    const isDeleting = deletingSymbol === item.symbol;
                                    const isSelected = selectedSymbol === item.symbol;

                                    return (
                                        <TableRow
                                            key={item.symbol}
                                            hover
                                            selected={isSelected}
                                            onClick={() => onSymbolChange(item.symbol)}
                                            sx={{
                                                cursor: 'pointer',
                                                '&:hover': { bgcolor: 'action.hover' },
                                                '&.Mui-selected': {
                                                    bgcolor: 'primary.dark',
                                                    '&:hover': { bgcolor: 'primary.dark' }
                                                },
                                                transition: 'background-color 0.15s'
                                            }}
                                        >
                                            <TableCell sx={{ py: 0.5, fontWeight: isSelected ? 600 : 400, fontSize: '0.75rem', whiteSpace: 'nowrap' }}>
                                                {item.symbol}
                                            </TableCell>
                                            <TableCell sx={{ py: 0.5, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                                                <Tooltip title={item.companyName || COMPANY_NAMES[item.symbol] || item.symbol}>
                                                    <Typography variant="caption" color="text.secondary" noWrap>
                                                        {item.companyName || COMPANY_NAMES[item.symbol] || '—'}
                                                    </Typography>
                                                </Tooltip>
                                            </TableCell>
                                            <TableCell align="center" sx={{ py: 0.5 }}>
                                                {(() => {
                                                    const assignment = scenarioState?.symbolAssignments?.find(a => a.symbol === item.symbol);
                                                    if (!assignment) return <Typography variant="caption" color="text.disabled">—</Typography>;
                                                    const scenarioLabel = assignment.scenarioPreset.replace(/_/g, ' ');
                                                    return (
                                                        <Tooltip title={`Scenario: ${scenarioLabel}`}>
                                                            <AutoGraphIcon
                                                                fontSize="small"
                                                                sx={{
                                                                    color: assignment.scenarioPreset.includes('BULL') ? 'success.main'
                                                                        : assignment.scenarioPreset.includes('BEAR') ? 'error.main'
                                                                            : 'warning.main',
                                                                    fontSize: '1rem'
                                                                }}
                                                            />
                                                        </Tooltip>
                                                    );
                                                })()}
                                            </TableCell>
                                            <TableCell align="center">
                                                {result && !result.hasError ? (
                                                    <Chip
                                                        label={result.volumeStatus}
                                                        size="small"
                                                        color={getVolumeColor(result.volumeStatus) as 'success' | 'warning' | 'default'}
                                                        sx={{
                                                            height: 18,
                                                            fontSize: '0.6rem',
                                                            '& .MuiChip-label': { px: 0.5 }
                                                        }}
                                                    />
                                                ) : (
                                                    <Typography variant="caption" color="text.disabled">—</Typography>
                                                )}
                                            </TableCell>
                                            <TableCell align="center" sx={{ py: 0.5 }}>
                                                {result && !result.hasError ? (
                                                    <Tooltip title={`Trend: ${result.trend}`}>
                                                        {getTrendIcon(result.trend)}
                                                    </Tooltip>
                                                ) : (
                                                    <Typography variant="caption" color="text.disabled">—</Typography>
                                                )}
                                            </TableCell>
                                            <TableCell align="center">
                                                {result && !result.hasError ? (
                                                    <Tooltip title={`Confidence: ${result.confidence}%`}>
                                                        <Typography
                                                            variant="caption"
                                                            sx={{
                                                                fontWeight: 600,
                                                                color: result.confidence >= 75
                                                                    ? 'success.main'
                                                                    : result.confidence >= 60
                                                                        ? 'warning.main'
                                                                        : 'error.main',
                                                            }}
                                                        >
                                                            {result.confidence}
                                                        </Typography>
                                                    </Tooltip>
                                                ) : (
                                                    <Typography variant="caption" color="text.disabled">—</Typography>
                                                )}
                                            </TableCell>
                                            <TableCell align="center">
                                                {result && !result.hasError && result.hasNews ? (
                                                    <Tooltip title="Has recent news">
                                                        <NewspaperIcon fontSize="small" color="info" />
                                                    </Tooltip>
                                                ) : (
                                                    <Typography variant="caption" color="text.disabled">—</Typography>
                                                )}
                                            </TableCell>
                                            <TableCell align="right" sx={{ py: 0.25 }}>
                                                <Tooltip
                                                    title={
                                                        hasOpenTrades
                                                            ? 'Cannot remove: Symbol has open trades'
                                                            : 'Remove from watchlist'
                                                    }
                                                >
                                                    <span>
                                                        <IconButton
                                                            size="small"
                                                            onClick={(e) => handleDelete(e, item.symbol)}
                                                            disabled={hasOpenTrades || isDeleting || loading}
                                                            sx={{
                                                                opacity: hasOpenTrades ? 0.3 : 0.7,
                                                                '&:hover': {
                                                                    color: hasOpenTrades ? 'inherit' : 'error.main',
                                                                    opacity: 1
                                                                }
                                                            }}
                                                        >
                                                            <DeleteIcon fontSize="small" />
                                                        </IconButton>
                                                    </span>
                                                </Tooltip>
                                            </TableCell>
                                        </TableRow>
                                    );
                                })}
                            </TableBody>
                        </Table>
                    </TableContainer>
                )}
            </Box>
        </Paper>
    );
}
