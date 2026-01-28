import { useState, useEffect } from 'react';
import {
    Box,
    Paper,
    Typography,
    Button,
    Switch,
    FormControlLabel,
    Stack,
    Alert,
    CircularProgress,
    Select,
    MenuItem,
    FormControl,
    InputLabel,
    Chip,
    Tooltip,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Collapse,
    IconButton,
} from '@mui/material';
import RestartAltIcon from '@mui/icons-material/RestartAlt';
import AutoGraphIcon from '@mui/icons-material/AutoGraph';
import InfoOutlinedIcon from '@mui/icons-material/InfoOutlined';
import ShuffleIcon from '@mui/icons-material/Shuffle';
import ExpandMoreIcon from '@mui/icons-material/ExpandMore';
import ExpandLessIcon from '@mui/icons-material/ExpandLess';
import { useReplayState } from '../hooks/useReplayState';
import type { ScenarioState } from '../api/scenarioApi';
import {
    getScenarioState,
    applyScenarioPreset,
    setScenarioEnabled,
    resetScenario,
    redistributeScenarios,
} from '../api/scenarioApi';

// Preset descriptions for tooltips
const PRESET_DESCRIPTIONS: Record<string, string> = {
    'Realistic Day': 'Typical trading day: quiet open → mid-morning activity → lunch lull → afternoon push. Natural market rhythm.',
    'Default': 'Gentle random walk with low volatility. Good baseline for basic testing without extreme movements.',
    'VWAP Long Setup': 'Gradual accumulation below VWAP → steady breakout → healthy uptrend with natural pullbacks.',
    'VWAP Short Setup': 'Distribution above VWAP → gradual breakdown → controlled downtrend with dead cat bounces.',
    'Volume Breakout': 'Extended quiet consolidation → volume builds gradually → breakout with sustained follow-through.',
    'Choppy Sideways': 'Frustrating sideways chop with false breakouts. Tests signal filtering in unclear conditions.',
    'Volatile Session': 'Elevated volatility day with wider price swings. Tests ATR-based position sizing.',
    'Trend Reversal': 'Healthy uptrend loses momentum → distribution top → gradual breakdown into downtrend.',
    'Flash Crash': 'Normal trading → sudden sharp selloff → panic → gradual stabilization and recovery.',
};

// Regime display names and descriptions
const REGIME_INFO: Record<string, { name: string; description: string; color: string }> = {
    'TREND_UP': { name: 'Trend ', description: 'Uptrend: Consistent buying pressure, higher highs', color: '#4caf50' },
    'TREND_DOWN': { name: 'Trend ', description: 'Downtrend: Consistent selling pressure, lower lows', color: '#f44336' },
    'RANGE': { name: 'Range', description: 'Sideways: Price oscillates between support/resistance', color: '#2196f3' },
    'HIGH_VOL': { name: 'High Vol', description: 'High Volatility: Large price swings, unpredictable', color: '#ff9800' },
    'LOW_VOL': { name: 'Low Vol', description: 'Low Volatility: Tight range, low activity', color: '#00bcd4' },
    'CRASH': { name: 'Crash', description: 'Market Crash: Extreme sell-off, panic selling', color: '#9c27b0' },
    'NEWS_SPIKE': { name: 'News', description: 'News Spike: Sudden move from news event', color: '#e91e63' },
};

export default function SimulationControlPanel() {
    const { state, loading, error, setMode } = useReplayState();

    const [actionLoading, setActionLoading] = useState(false);
    const [actionError, setActionError] = useState<string | null>(null);
    const [scenarioState, setScenarioState] = useState<ScenarioState | null>(null);
    const [scenarioLoading, setScenarioLoading] = useState(false);
    const [selectedPreset, setSelectedPreset] = useState<string>('');
    const [showAssignments, setShowAssignments] = useState(false);

    useEffect(() => {
        loadScenarioState();
    }, []);

    const loadScenarioState = async () => {
        try {
            const data = await getScenarioState();
            setScenarioState(data);
            if (data.activeConfig) {
                setSelectedPreset(data.activeConfig.name);
            }
        } catch (err) {
            console.error('Failed to load scenario state:', err);
        }
    };

    const handleScenarioToggle = async () => {
        if (!scenarioState) return;
        setScenarioLoading(true);
        setActionError(null);
        try {
            await setScenarioEnabled(!scenarioState.isEnabled);
            await loadScenarioState();
        } catch (err) {
            setActionError(err instanceof Error ? err.message : 'Failed to toggle scenario');
        } finally {
            setScenarioLoading(false);
        }
    };

    const handlePresetChange = async (presetName: string) => {
        setScenarioLoading(true);
        setActionError(null);
        try {
            await applyScenarioPreset(presetName);
            setSelectedPreset(presetName);
            await loadScenarioState();
        } catch (err) {
            setActionError(err instanceof Error ? err.message : 'Failed to apply preset');
        } finally {
            setScenarioLoading(false);
        }
    };

    const handleScenarioReset = async () => {
        setScenarioLoading(true);
        setActionError(null);
        try {
            await resetScenario();
            await loadScenarioState();
        } catch (err) {
            setActionError(err instanceof Error ? err.message : 'Failed to reset scenario');
        } finally {
            setScenarioLoading(false);
        }
    };

    const handleRedistribute = async () => {
        setScenarioLoading(true);
        setActionError(null);
        try {
            const newState = await redistributeScenarios();
            setScenarioState(newState);
        } catch (err) {
            setActionError(err instanceof Error ? err.message : 'Failed to redistribute scenarios');
        } finally {
            setScenarioLoading(false);
        }
    };

    const getRegimeInfo = (regimeType: string) => {
        return REGIME_INFO[regimeType] || { name: regimeType, description: 'Unknown regime type', color: '#757575' };
    };

    if (loading) {
        return (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
                <CircularProgress />
            </Box>
        );
    }

    if (!state) {
        return <Alert severity="error">Failed to load simulation state</Alert>;
    }

    const isLive = state.mode === 'LIVE';

    const handleModeToggle = async () => {
        setActionLoading(true);
        setActionError(null);
        try {
            await setMode(isLive ? 'REPLAY' : 'LIVE');
        } catch (err) {
            setActionError(err instanceof Error ? err.message : 'Failed to change mode');
        } finally {
            setActionLoading(false);
        }
    };

    return (
        <Box sx={{ p: 3, maxWidth: 800, mx: 'auto' }}>
            <Typography variant="h4" gutterBottom>
                Simulation Control Panel
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mb: 3 }}>
                Switch between live market data and mock simulation
            </Typography>

            {(error || actionError) && (
                <Alert severity="error" sx={{ mb: 2 }} onClose={() => setActionError(null)}>
                    {error || actionError}
                </Alert>
            )}

            <Stack spacing={3}>
                {/* Data Provider Toggle */}
                <Paper sx={{ p: 3 }}>
                    <Typography variant="h6" gutterBottom>
                        Data Provider
                    </Typography>
                    <FormControlLabel
                        control={
                            <Switch
                                checked={!isLive}
                                onChange={handleModeToggle}
                                disabled={actionLoading}
                            />
                        }
                        label={
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                <Typography>
                                    {isLive ? 'Yahoo Finance (Live)' : 'Mock Provider (Simulation)'}
                                </Typography>
                                <Box sx={{
                                    px: 1, py: 0.5,
                                    bgcolor: isLive ? 'success.main' : 'warning.main',
                                    borderRadius: 1
                                }}>
                                    <Typography variant="caption" sx={{ color: 'white', fontWeight: 'bold' }}>
                                        {isLive ? 'LIVE' : 'MOCK'}
                                    </Typography>
                                </Box>
                            </Box>
                        }
                    />
                    <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 1 }}>
                        {isLive
                            ? 'Using delayed Yahoo Finance data (15-20 min delay)'
                            : 'Using generated mock data with configurable scenarios'
                        }
                    </Typography>
                </Paper>

                {/* Market Scenarios Section - Only visible in Mock mode */}
                {!isLive && (
                    <Paper sx={{ p: 3 }}>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
                            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
                                <AutoGraphIcon color="primary" />
                                <Typography variant="h6">Market Scenarios</Typography>
                                <Tooltip title="Scenarios simulate different market conditions (trending, volatile, crashing) to test trading signals and strategies.">
                                    <InfoOutlinedIcon sx={{ fontSize: 18, color: 'grey.500', cursor: 'help' }} />
                                </Tooltip>
                            </Box>
                            <FormControlLabel
                                control={
                                    <Switch
                                        checked={scenarioState?.isEnabled ?? false}
                                        onChange={handleScenarioToggle}
                                        disabled={scenarioLoading}
                                    />
                                }
                                label={scenarioState?.isEnabled ? 'Enabled' : 'Disabled'}
                            />
                        </Box>

                        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
                            Configure realistic market conditions with regimes and patterns for testing
                        </Typography>

                        {scenarioState?.isEnabled && (
                            <>
                                {/* Preset Selection with Description */}
                                <FormControl fullWidth sx={{ mb: 2 }}>
                                    <InputLabel>Scenario Preset</InputLabel>
                                    <Select
                                        value={selectedPreset}
                                        label="Scenario Preset"
                                        onChange={(e) => handlePresetChange(e.target.value)}
                                        disabled={scenarioLoading}
                                    >
                                        {scenarioState.availablePresets.map((preset) => (
                                            <MenuItem key={preset} value={preset}>
                                                <Box sx={{ display: 'flex', flexDirection: 'column' }}>
                                                    <Typography>{preset}</Typography>
                                                    <Typography variant="caption" color="text.secondary">
                                                        {PRESET_DESCRIPTIONS[preset] || 'Custom scenario'}
                                                    </Typography>
                                                </Box>
                                            </MenuItem>
                                        ))}
                                    </Select>
                                </FormControl>

                                {/* Active Scenario Description */}
                                {selectedPreset && PRESET_DESCRIPTIONS[selectedPreset] && (
                                    <Alert severity="info" sx={{ mb: 2 }} icon={<InfoOutlinedIcon />}>
                                        <Typography variant="body2">
                                            <strong>{selectedPreset}:</strong> {PRESET_DESCRIPTIONS[selectedPreset]}
                                        </Typography>
                                    </Alert>
                                )}

                                {/* Active Scenario Info */}
                                {scenarioState.activeConfig && (
                                    <Box sx={{ bgcolor: 'grey.900', borderRadius: 1, p: 2, mb: 2 }}>
                                        <Typography variant="subtitle2" gutterBottom color="grey.100">
                                            Regime Timeline
                                            <Tooltip title="Market phases that will be simulated in sequence. Each phase has a specific behavior pattern.">
                                                <InfoOutlinedIcon sx={{ fontSize: 14, ml: 0.5, color: 'grey.500', cursor: 'help' }} />
                                            </Tooltip>
                                        </Typography>

                                        <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap', mb: 2 }}>
                                            {scenarioState.activeConfig.regimes?.map((regime, idx) => {
                                                const info = getRegimeInfo(regime.type as string);
                                                return (
                                                    <Tooltip
                                                        key={idx}
                                                        title={
                                                            <Box>
                                                                <Typography variant="body2" fontWeight="bold">{info.name}</Typography>
                                                                <Typography variant="caption">{info.description}</Typography>
                                                                <Typography variant="caption" display="block" sx={{ mt: 0.5 }}>
                                                                    Duration: {regime.bars} bars
                                                                </Typography>
                                                            </Box>
                                                        }
                                                        arrow
                                                    >
                                                        <Chip
                                                            size="small"
                                                            label={`${info.name} (${regime.bars})`}
                                                            sx={{
                                                                bgcolor: info.color,
                                                                color: 'white',
                                                                fontSize: '0.75rem',
                                                                cursor: 'help',
                                                            }}
                                                        />
                                                    </Tooltip>
                                                );
                                            }) ?? null}
                                        </Box>

                                        {/* Legend */}
                                        <Typography variant="caption" color="grey.500" display="block">
                                            Hover over phases for details  Bars = number of candles in each phase
                                        </Typography>
                                    </Box>
                                )}

                                <Box sx={{ display: 'flex', gap: 1 }}>
                                    <Button
                                        variant="outlined"
                                        startIcon={<RestartAltIcon />}
                                        onClick={handleScenarioReset}
                                        disabled={scenarioLoading}
                                        size="small"
                                    >
                                        Reset to Default
                                    </Button>
                                    <Button
                                        variant="outlined"
                                        startIcon={<ShuffleIcon />}
                                        onClick={handleRedistribute}
                                        disabled={scenarioLoading}
                                        size="small"
                                        color="secondary"
                                    >
                                        Redistribute
                                    </Button>
                                </Box>

                                {/* Symbol Assignments Section */}
                                <Box sx={{ mt: 3 }}>
                                    <Box
                                        sx={{
                                            display: 'flex',
                                            alignItems: 'center',
                                            cursor: 'pointer',
                                            '&:hover': { opacity: 0.8 }
                                        }}
                                        onClick={() => setShowAssignments(!showAssignments)}
                                    >
                                        <IconButton size="small">
                                            {showAssignments ? <ExpandLessIcon /> : <ExpandMoreIcon />}
                                        </IconButton>
                                        <Typography variant="subtitle2" color="grey.100">
                                            Symbol Assignments ({scenarioState.symbolAssignments?.length ?? 0} symbols)
                                        </Typography>
                                        <Tooltip title="Each symbol is randomly assigned a scenario preset. Click to view all assignments.">
                                            <InfoOutlinedIcon sx={{ fontSize: 14, ml: 0.5, color: 'grey.500', cursor: 'help' }} />
                                        </Tooltip>
                                    </Box>

                                    <Collapse in={showAssignments}>
                                        <TableContainer sx={{ maxHeight: 300, mt: 1 }}>
                                            <Table size="small" stickyHeader>
                                                <TableHead>
                                                    <TableRow>
                                                        <TableCell sx={{ bgcolor: 'grey.800', color: 'grey.100' }}>Symbol</TableCell>
                                                        <TableCell sx={{ bgcolor: 'grey.800', color: 'grey.100' }}>Scenario</TableCell>
                                                        <TableCell sx={{ bgcolor: 'grey.800', color: 'grey.100' }}>Strategy</TableCell>
                                                    </TableRow>
                                                </TableHead>
                                                <TableBody>
                                                    {scenarioState.symbolAssignments?.map((assignment) => (
                                                        <TableRow key={assignment.symbol} hover>
                                                            <TableCell sx={{ color: 'grey.300', fontFamily: 'monospace' }}>
                                                                {assignment.symbol}
                                                            </TableCell>
                                                            <TableCell>
                                                                <Chip
                                                                    label={assignment.scenarioPreset}
                                                                    size="small"
                                                                    sx={{
                                                                        fontSize: '0.7rem',
                                                                        height: 20,
                                                                    }}
                                                                />
                                                            </TableCell>
                                                            <TableCell sx={{ color: 'grey.400' }}>
                                                                {assignment.strategy}
                                                            </TableCell>
                                                        </TableRow>
                                                    ))}
                                                </TableBody>
                                            </Table>
                                        </TableContainer>
                                    </Collapse>
                                </Box>
                            </>
                        )}
                    </Paper>
                )}

                {/* Current State Display */}
                <Paper sx={{ p: 3, bgcolor: 'grey.900' }}>
                    <Typography variant="h6" gutterBottom color="grey.100">
                        Current State
                    </Typography>
                    <Stack spacing={1}>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                            <Typography variant="body2" color="grey.400">Provider:</Typography>
                            <Typography variant="body2" fontWeight="bold" color="grey.100">
                                {isLive ? 'Yahoo Finance' : 'Mock (Simulation)'}
                            </Typography>
                        </Box>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                            <Typography variant="body2" color="grey.400">Current Time:</Typography>
                            <Typography variant="body2" fontWeight="bold" color="grey.100">
                                {new Date(state.currentTime).toLocaleString('de-DE')}
                            </Typography>
                        </Box>
                        {!isLive && scenarioState?.isEnabled && (
                            <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                                <Typography variant="body2" color="grey.400">Scenario:</Typography>
                                <Typography variant="body2" fontWeight="bold" color="grey.100">
                                    {scenarioState.activeConfig?.name ?? 'None'}
                                </Typography>
                            </Box>
                        )}
                    </Stack>
                </Paper>
            </Stack>
        </Box>
    );
}
