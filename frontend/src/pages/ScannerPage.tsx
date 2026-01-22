import { useState, useCallback, useMemo, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Paper from '@mui/material/Paper';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import TableSortLabel from '@mui/material/TableSortLabel';
import Button from '@mui/material/Button';
import FormControlLabel from '@mui/material/FormControlLabel';
import Switch from '@mui/material/Switch';
import AddIcon from '@mui/icons-material/Add';
import CheckIcon from '@mui/icons-material/Check';
import { ScoreBadge, TrendBadge, VolumeBadge, NewsBadge, ConfidenceBadge } from '../components/Badge';
import LoadingSpinner from '../components/LoadingSpinner';
import ErrorAlert from '../components/ErrorAlert';
import { useWatchlist } from '../hooks/useWatchlist';
import { useScanner } from '../hooks/useScanner';
import { useReplayRefresh } from '../hooks/useReplayRefresh';

// Fixed DAX stock list
const DAX_STOCKS = [
    { name: 'SAP', ticker: 'SAP.DE' },
    { name: 'Siemens', ticker: 'SIE.DE' },
    { name: 'Allianz', ticker: 'ALV.DE' },
    { name: 'BASF', ticker: 'BAS.DE' },
    { name: 'Infineon', ticker: 'IFX.DE' },
    { name: 'BMW', ticker: 'BMW.DE' },
    { name: 'Mercedes-Benz Group', ticker: 'MBG.DE' },
    { name: 'Volkswagen', ticker: 'VOW3.DE' },
    { name: 'Deutsche Telekom', ticker: 'DTE.DE' },
    { name: 'RWE', ticker: 'RWE.DE' },
    { name: 'E.ON', ticker: 'EOAN.DE' },
    { name: 'Munich Re', ticker: 'MUV2.DE' },
    { name: 'Commerzbank', ticker: 'CBK.DE' },
    { name: 'Deutsche Bank', ticker: 'DBK.DE' },
    { name: 'Siemens Energy', ticker: 'ENR.DE' },
    { name: 'Adidas', ticker: 'ADS.DE' },
    { name: 'Bayer', ticker: 'BAYN.DE' },
    { name: 'Heidelberg Materials', ticker: 'HEI.DE' },
    { name: 'Zalando', ticker: 'ZAL.DE' },
    { name: 'Deutsche Börse', ticker: 'DB1.DE' },
    { name: 'Rheinmetall', ticker: 'RHM.DE' },
    { name: 'MTU Aero Engines', ticker: 'MTX.DE' },
    { name: 'Airbus', ticker: 'AIR.DE' },
    { name: 'Sartorius', ticker: 'SRT3.DE' },
    { name: 'Symrise', ticker: 'SY1.DE' },
    { name: 'Henkel', ticker: 'HEN3.DE' },
    { name: 'Covestro', ticker: '1COV.DE' },
    { name: 'Porsche AG', ticker: 'P911.DE' },
    { name: 'Vonovia', ticker: 'VNA.DE' },
    { name: 'Fresenius', ticker: 'FRE.DE' },
    { name: 'HelloFresh', ticker: 'HFG.DE' },
    { name: 'Delivery Hero', ticker: 'DHER.DE' },
    { name: 'TeamViewer', ticker: 'TMV.DE' },
    { name: 'Aixtron', ticker: 'AIXA.DE' },
    { name: 'SMA Solar', ticker: 'S92.DE' },
    { name: 'Varta', ticker: 'VAR1.DE' },
    { name: 'Evotec', ticker: 'EVT.DE' },
    { name: 'Carl Zeiss Meditec', ticker: 'AFX.DE' },
    { name: 'Nemetschek', ticker: 'NEM.DE' },
    { name: 'United Internet', ticker: 'UTDI.DE' },
    { name: 'Siltronic', ticker: 'WAF.DE' },
    { name: 'Jenoptik', ticker: 'JEN.DE' },
    { name: 'Cancom', ticker: 'COK.DE' },
    { name: 'GFT Technologies', ticker: 'GFT.DE' },
    { name: 'LPKF Laser', ticker: 'LPKF.DE' },
    { name: 'Dermapharm', ticker: 'DMP.DE' },
    { name: 'Encavis', ticker: 'ECV.DE' },
    { name: 'Scout24', ticker: 'G24.DE' },
    { name: 'CTS Eventim', ticker: 'EVD.DE' },
    { name: 'Hugo Boss', ticker: 'BOSS.DE' },
    { name: 'Rational', ticker: 'RAA.DE' },
    { name: 'Bilfinger', ticker: 'GBF.DE' },
    { name: 'Wacker Chemie', ticker: 'WCH.DE' },
    { name: 'Fraport', ticker: 'FRA.DE' },
    { name: 'Lufthansa', ticker: 'LHA.DE' },
    { name: 'Hochtief', ticker: 'HOT.DE' },
    { name: 'Salzgitter', ticker: 'SZG.DE' },
    { name: 'K+S', ticker: 'SDF.DE' },
    { name: 'Traton', ticker: '8TRA.DE' },
    { name: 'Knorr-Bremse', ticker: 'KBX.DE' },
    { name: 'Beiersdorf', ticker: 'BEI.DE' },
    { name: 'Hannover Rück', ticker: 'HNR1.DE' },
    { name: 'Brenntag', ticker: 'BNR.DE' },
    { name: 'Siemens Healthineers', ticker: 'SHL.DE' },
    { name: 'Fresenius Medical Care', ticker: 'FME.DE' },
    { name: 'Merck KGaA', ticker: 'MRK.DE' },
    { name: 'Qiagen', ticker: 'QIA.DE' },
    { name: 'Porsche SE', ticker: 'PAH3.DE' },
    { name: 'LEG Immobilien', ticker: 'LEG.DE' },
    { name: 'TAG Immobilien', ticker: 'TEG.DE' },
    { name: 'Evonik', ticker: 'EVK.DE' },
    { name: 'KION Group', ticker: 'KGX.DE' },
    { name: 'GEA Group', ticker: 'G1A.DE' },
    { name: 'Talanx', ticker: 'TLX.DE' },
    { name: 'RTL Group', ticker: 'RRTL.DE' },
    { name: 'Freenet', ticker: 'FNTN.DE' },
    { name: 'SGL Carbon', ticker: 'SGL.DE' },
    { name: 'Norma Group', ticker: 'NOEJ.DE' },
    { name: 'Stabilus', ticker: 'STM.DE' },
    { name: 'Vossloh', ticker: 'VOS.DE' },
    { name: 'Schaeffler', ticker: 'SHA.DE' },
    { name: 'Pfeiffer Vacuum', ticker: 'PFV.DE' },
    { name: 'Fielmann', ticker: 'FIE.DE' },
    { name: 'Compugroup Medical', ticker: 'COP.DE' },
    { name: 'Deutsche Wohnen', ticker: 'DWNI.DE' },
    { name: 'Nagarro', ticker: 'NA9.DE' },
    { name: 'Software AG', ticker: 'SOW.DE' },
    { name: 'Hypoport', ticker: 'HYQ.DE' },
    { name: 'Medios', ticker: 'ILM1.DE' },
    { name: 'ElringKlinger', ticker: 'ZIL2.DE' },
    { name: 'Dürr', ticker: 'DUE.DE' },
    { name: 'Springer Nature', ticker: 'SPG.DE' },
    { name: 'Jungheinrich', ticker: 'JUN3.DE' },
    { name: 'Grand City Properties', ticker: 'GYC.DE' },
    { name: 'Aurubis', ticker: 'NDA.DE' },
    { name: 'Süss MicroTec', ticker: 'SMHN.DE' },
];

export default function ScannerPage() {
    const navigate = useNavigate();
    const { watchlist, loading: watchlistLoading, addSymbol } = useWatchlist();

    // All DAX symbols for scanning
    const allSymbols = useMemo(() => DAX_STOCKS.map(s => s.ticker), []);

    // Create a set of watchlist symbols for quick lookup
    const watchlistSymbols = useMemo(() => new Set(watchlist.map(w => w.symbol)), [watchlist]);

    const { scanResults, loading: scanLoading, error: scanError, reload } = useScanner(allSymbols, true);

    // Auto-refresh scanner during replay simulation (every 10 seconds)
    const handleReplayRefresh = useCallback(() => {
        reload();
    }, [reload]);

    useReplayRefresh(handleReplayRefresh, 10000);

    const [filterMinScore, setFilterMinScore] = useState(false);
    const [addingSymbol, setAddingSymbol] = useState<string | null>(null);
    const [orderBy, setOrderBy] = useState<'score' | 'confidence' | 'company'>('score');
    const [order, setOrder] = useState<'asc' | 'desc'>('desc');

    const filteredResults = filterMinScore
        ? scanResults.filter(r => r.score >= 70)
        : scanResults;

    // Sort stocks by selected column
    const sortedStocks = useMemo(() => {
        const stocksWithResults = DAX_STOCKS.map(stock => ({
            ...stock,
            result: scanResults.find(r => r.symbol === stock.ticker)
        }));

        return stocksWithResults.sort((a, b) => {
            let compareValue = 0;

            if (orderBy === 'score') {
                const scoreA = a.result?.score ?? -1;
                const scoreB = b.result?.score ?? -1;
                compareValue = scoreB - scoreA;
            } else if (orderBy === 'confidence') {
                const confA = a.result?.confidence ?? -1;
                const confB = b.result?.confidence ?? -1;
                compareValue = confB - confA;
            } else if (orderBy === 'company') {
                compareValue = a.name.localeCompare(b.name);
            }

            return order === 'asc' ? -compareValue : compareValue;
        });
    }, [scanResults, orderBy, order]);

    // Debug logging
    useEffect(() => {
        console.log('ScannerPage: scanResults updated:', scanResults.length, scanResults);
        console.log('ScannerPage: filteredResults:', filteredResults.length, filteredResults);
    }, [scanResults, filteredResults]);

    const handleRowClick = (symbol: string) => {
        navigate(`/symbol/${symbol}`);
    };

    const handleAddToWatchlist = async (e: React.MouseEvent, ticker: string) => {
        e.stopPropagation();
        setAddingSymbol(ticker);
        try {
            await addSymbol(ticker);
        } catch (error) {
            console.error('Failed to add symbol to watchlist:', error);
            alert(`Failed to add ${ticker} to watchlist: ${error instanceof Error ? error.message : 'Unknown error'}`);
        } finally {
            setAddingSymbol(null);
        }
    };

    const handleRequestSort = (property: 'score' | 'confidence' | 'company') => {
        const isAsc = orderBy === property && order === 'asc';
        setOrder(isAsc ? 'desc' : 'asc');
        setOrderBy(property);
    };

    if (watchlistLoading) {
        return (
            <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '60vh' }}>
                <LoadingSpinner />
            </Box>
        );
    }

    return (
        <Box sx={{ p: 3, height: '100vh', display: 'flex', flexDirection: 'column' }}>
            <Typography variant="h4" gutterBottom>
                German Stock Scanner
            </Typography>
            <Typography variant="body2" color="text.secondary" gutterBottom>
                Scan German stocks for daytrading opportunities. Add interesting stocks to your watchlist to track them in the Dashboard.
            </Typography>

            {/* Scan Errors */}
            {scanError && <ErrorAlert error={scanError} />}

            {/* Filter */}
            <Box sx={{ mb: 2, mt: 3, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <FormControlLabel
                    control={
                        <Switch
                            checked={filterMinScore}
                            onChange={(e) => setFilterMinScore(e.target.checked)}
                        />
                    }
                    label="Only show Score >= 70"
                />
                <Typography variant="body2" color="text.secondary">
                    {filteredResults.length} of {DAX_STOCKS.length} stocks | {watchlistSymbols.size} in watchlist
                </Typography>
            </Box>

            {/* Scanner Table */}
            <TableContainer component={Paper} sx={{ flexGrow: 1, overflow: 'auto', maxHeight: 'calc(100vh - 250px)' }}>
                {scanLoading && (
                    <Box sx={{ position: 'absolute', top: 8, right: 8, zIndex: 1 }}>
                        <LoadingSpinner size={24} />
                    </Box>
                )}
                <Table>
                    <TableHead>
                        <TableRow>
                            <TableCell>
                                <TableSortLabel
                                    active={orderBy === 'company'}
                                    direction={orderBy === 'company' ? order : 'asc'}
                                    onClick={() => handleRequestSort('company')}
                                >
                                    <strong>Company</strong>
                                </TableSortLabel>
                            </TableCell>
                            <TableCell><strong>Ticker</strong></TableCell>
                            <TableCell>
                                <TableSortLabel
                                    active={orderBy === 'score'}
                                    direction={orderBy === 'score' ? order : 'desc'}
                                    onClick={() => handleRequestSort('score')}
                                >
                                    <strong>Score</strong>
                                </TableSortLabel>
                            </TableCell>
                            <TableCell><strong>Trend</strong></TableCell>
                            <TableCell><strong>Volume</strong></TableCell>
                            <TableCell><strong>News</strong></TableCell>
                            <TableCell>
                                <TableSortLabel
                                    active={orderBy === 'confidence'}
                                    direction={orderBy === 'confidence' ? order : 'desc'}
                                    onClick={() => handleRequestSort('confidence')}
                                >
                                    <strong>Confidence</strong>
                                </TableSortLabel>
                            </TableCell>
                            <TableCell><strong>Reasons</strong></TableCell>
                            <TableCell align="right"><strong>Watchlist</strong></TableCell>
                        </TableRow>
                    </TableHead>
                    <TableBody>
                        {sortedStocks.map((stock) => {
                            const result = stock.result;
                            const isInWatchlist = watchlistSymbols.has(stock.ticker);
                            const isAdding = addingSymbol === stock.ticker;

                            // Skip if filtering and score is too low
                            if (filterMinScore && result && result.score < 70) {
                                return null;
                            }

                            return (
                                <TableRow
                                    key={stock.ticker}
                                    hover
                                    onClick={() => result && !result.hasError && handleRowClick(stock.ticker)}
                                    sx={{
                                        cursor: result?.hasError ? 'default' : 'pointer',
                                        bgcolor: result?.hasError ? 'error.dark' : 'inherit',
                                        opacity: result?.hasError ? 0.7 : 1
                                    }}
                                >
                                    <TableCell>
                                        <Typography variant="body1" fontWeight="bold">
                                            {stock.name}
                                        </Typography>
                                    </TableCell>
                                    <TableCell>
                                        <Typography variant="body2" sx={{ fontFamily: 'monospace' }}>
                                            {stock.ticker}
                                        </Typography>
                                        {result?.hasError && (
                                            <Typography variant="caption" color="error" sx={{ display: 'block' }}>
                                                ⚠️ {result.errorMessage}
                                            </Typography>
                                        )}
                                    </TableCell>
                                    <TableCell>
                                        {result && !result.hasError ? (
                                            <ScoreBadge score={result.score} />
                                        ) : (
                                            <Typography variant="body2" color="text.secondary">-</Typography>
                                        )}
                                    </TableCell>
                                    <TableCell>
                                        {result && !result.hasError ? (
                                            <TrendBadge trend={result.trend} />
                                        ) : (
                                            <Typography variant="body2" color="text.secondary">-</Typography>
                                        )}
                                    </TableCell>
                                    <TableCell>
                                        {result && !result.hasError ? (
                                            <VolumeBadge status={result.volumeStatus} />
                                        ) : (
                                            <Typography variant="body2" color="text.secondary">-</Typography>
                                        )}
                                    </TableCell>
                                    <TableCell>
                                        {result && !result.hasError ? (
                                            <NewsBadge hasNews={result.hasNews} />
                                        ) : (
                                            <Typography variant="body2" color="text.secondary">-</Typography>
                                        )}
                                    </TableCell>
                                    <TableCell>
                                        {result && !result.hasError ? (
                                            <ConfidenceBadge confidence={result.confidence} />
                                        ) : (
                                            <Typography variant="body2" color="text.secondary">-</Typography>
                                        )}
                                    </TableCell>
                                    <TableCell>
                                        {result && !result.hasError && result.reasons.length > 0 ? (
                                            <Box component="ul" sx={{ margin: 0, paddingLeft: 2 }}>
                                                {result.reasons.map((reason: string, idx: number) => (
                                                    <li key={idx}>
                                                        <Typography variant="body2">{reason}</Typography>
                                                    </li>
                                                ))}
                                            </Box>
                                        ) : (
                                            <Typography variant="body2" color="text.secondary">-</Typography>
                                        )}
                                    </TableCell>
                                    <TableCell align="right">
                                        {isInWatchlist ? (
                                            <Button
                                                size="small"
                                                variant="outlined"
                                                color="success"
                                                startIcon={<CheckIcon />}
                                                disabled
                                            >
                                                In Watchlist
                                            </Button>
                                        ) : (
                                            <Button
                                                size="small"
                                                variant="contained"
                                                color="primary"
                                                startIcon={<AddIcon />}
                                                onClick={(e) => handleAddToWatchlist(e, stock.ticker)}
                                                disabled={isAdding || watchlistLoading}
                                            >
                                                {isAdding ? 'Adding...' : 'Add'}
                                            </Button>
                                        )}
                                    </TableCell>
                                </TableRow>
                            );
                        })}
                    </TableBody>
                </Table>
            </TableContainer>
        </Box>
    );
}
