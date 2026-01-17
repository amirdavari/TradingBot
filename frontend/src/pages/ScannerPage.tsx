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
import IconButton from '@mui/material/IconButton';
import FormControlLabel from '@mui/material/FormControlLabel';
import Switch from '@mui/material/Switch';
import DeleteIcon from '@mui/icons-material/Delete';
import { ScoreBadge, TrendBadge, VolumeBadge, NewsBadge, ConfidenceBadge } from '../components/Badge';
import WatchlistAddForm from '../components/WatchlistAddForm';
import LoadingSpinner from '../components/LoadingSpinner';
import ErrorAlert from '../components/ErrorAlert';
import { useWatchlist } from '../hooks/useWatchlist';
import { useScanner } from '../hooks/useScanner';
import { useReplayRefresh } from '../hooks/useReplayRefresh';

export default function ScannerPage() {
    const navigate = useNavigate();
    const { watchlist, loading: watchlistLoading, addSymbol, deleteSymbol } = useWatchlist();
    
    // Memoize symbols array to prevent unnecessary re-scans
    const symbols = useMemo(() => watchlist.map(w => w.symbol), [watchlist]);
    
    const { scanResults, loading: scanLoading, error: scanError, reload } = useScanner(symbols, symbols.length > 0);

    // Auto-refresh scanner during replay simulation (every 10 seconds)
    // Use useCallback to stabilize the callback function
    const handleReplayRefresh = useCallback(() => {
        reload();
    }, [reload]);

    useReplayRefresh(handleReplayRefresh, 10000);

    const [filterMinScore, setFilterMinScore] = useState(false);

    const filteredResults = filterMinScore
        ? scanResults.filter(r => r.score >= 70)
        : scanResults;

    // Debug logging
    useEffect(() => {
        console.log('ScannerPage: scanResults updated:', scanResults.length, scanResults);
        console.log('ScannerPage: filteredResults:', filteredResults.length, filteredResults);
    }, [scanResults, filteredResults]);

    const handleRowClick = (symbol: string) => {
        navigate(`/symbol/${symbol}`);
    };

    const handleDelete = async (e: React.MouseEvent, symbol: string) => {
        e.stopPropagation();
        await deleteSymbol(symbol);
    };

    if (watchlistLoading) {
        return (
            <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '60vh' }}>
                <LoadingSpinner />
            </Box>
        );
    }

    return (
        <Box sx={{ p: 3 }}>
            <Typography variant="h4" gutterBottom>
                Stock Scanner
            </Typography>
            <Typography variant="body2" color="text.secondary" gutterBottom>
                Add symbols to your watchlist and scan for daytrading opportunities.
            </Typography>

            {/* Add Symbol Form */}
            <WatchlistAddForm onAdd={addSymbol} />

            {/* Scan Errors */}
            {scanError && <ErrorAlert error={scanError} />}

            {/* Filter */}
            <Box sx={{ mb: 2, display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
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
                    {filteredResults.length} of {scanResults.length} results
                </Typography>
            </Box>

            {/* Scanner Table */}
            {symbols.length === 0 ? (
                <Paper sx={{ p: 4, textAlign: 'center' }}>
                    <Typography variant="body1" color="text.secondary">
                        No symbols in watchlist. Add a symbol above to start scanning.
                    </Typography>
                </Paper>
            ) : filteredResults.length === 0 && !scanLoading ? (
                <Paper sx={{ p: 4, textAlign: 'center' }}>
                    <Typography variant="body1" color="text.secondary">
                        {scanResults.length === 0 ? 'Scanning symbols...' : 'No results match the current filter.'}
                    </Typography>
                </Paper>
            ) : (
                <TableContainer component={Paper}>
                    {scanLoading && (
                        <Box sx={{ position: 'absolute', top: 8, right: 8, zIndex: 1 }}>
                            <LoadingSpinner size={24} />
                        </Box>
                    )}
                    <Table>
                        <TableHead>
                            <TableRow>
                                <TableCell><strong>Symbol</strong></TableCell>
                                <TableCell><strong>Score</strong></TableCell>
                                <TableCell><strong>Trend</strong></TableCell>
                                <TableCell><strong>Volume</strong></TableCell>
                                <TableCell><strong>News</strong></TableCell>
                                <TableCell><strong>Confidence</strong></TableCell>
                                <TableCell><strong>Reasons</strong></TableCell>
                                <TableCell align="right"><strong>Actions</strong></TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {filteredResults.map((result) => (
                                <TableRow
                                    key={result.symbol}
                                    hover
                                    onClick={() => !result.hasError && handleRowClick(result.symbol)}
                                    sx={{ 
                                        cursor: result.hasError ? 'default' : 'pointer',
                                        bgcolor: result.hasError ? 'error.dark' : 'inherit',
                                        opacity: result.hasError ? 0.7 : 1
                                    }}
                                >
                                    <TableCell>
                                        <Typography variant="body1" fontWeight="bold">
                                            {result.symbol}
                                        </Typography>
                                        {result.hasError && (
                                            <Typography variant="caption" color="error" sx={{ display: 'block' }}>
                                                ⚠️ {result.errorMessage}
                                            </Typography>
                                        )}
                                    </TableCell>
                                    <TableCell>
                                        {!result.hasError ? (
                                            <ScoreBadge score={result.score} />
                                        ) : (
                                            <Typography variant="body2" color="text.secondary">-</Typography>
                                        )}
                                    </TableCell>
                                    <TableCell>
                                        {!result.hasError ? (
                                            <TrendBadge trend={result.trend} />
                                        ) : (
                                            <Typography variant="body2" color="text.secondary">-</Typography>
                                        )}
                                    </TableCell>
                                    <TableCell>
                                        {!result.hasError ? (
                                            <VolumeBadge status={result.volumeStatus} />
                                        ) : (
                                            <Typography variant="body2" color="text.secondary">-</Typography>
                                        )}
                                    </TableCell>
                                    <TableCell>
                                        {!result.hasError ? (
                                            <NewsBadge hasNews={result.hasNews} />
                                        ) : (
                                            <Typography variant="body2" color="text.secondary">-</Typography>
                                        )}
                                    </TableCell>
                                    <TableCell>
                                        {!result.hasError ? (
                                            <ConfidenceBadge confidence={result.confidence} />
                                        ) : (
                                            <Typography variant="body2" color="text.secondary">-</Typography>
                                        )}
                                    </TableCell>
                                    <TableCell>
                                        {!result.hasError && result.reasons.length > 0 ? (
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
                                        <IconButton
                                            size="small"
                                            color="error"
                                            onClick={(e) => handleDelete(e, result.symbol)}
                                            title="Remove from watchlist"
                                        >
                                            <DeleteIcon />
                                        </IconButton>
                                    </TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                </TableContainer>
            )}
        </Box>
    );
}
