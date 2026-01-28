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
} from '@mui/material';
import RestartAltIcon from '@mui/icons-material/RestartAlt';
import AutoGraphIcon from '@mui/icons-material/AutoGraph';
import { useReplayState } from '../hooks/useReplayState';
import type { ScenarioState } from '../api/scenarioApi';
import {
    MarketRegime,
    getScenarioState,
    applyScenarioPreset,
    setScenarioEnabled,
    resetScenario,
    getRegimeDisplayName,
    getRegimeColor,
} from '../api/scenarioApi';

/**
 * Simulation Control Panel for development and testing.
 * Allows switching between Live (Yahoo) and Mock data provider.
 */
export default function SimulationControlPanel() {
    const {
        state,
        loading,
        error,
        setMode,
    } = useReplayState();

    const [actionLoading, setActionLoading] = useState(false);
    const [actionError, setActionError] = useState<string | null>(null);

    // Scenario state
    const [scenarioState, setScenarioState] = useState<ScenarioState | null>(null);
    const [scenarioLoading, setScenarioLoading] = useState(false);
    const [selectedPreset, setSelectedPreset] = useState<string>('');

    // Load scenario state on mount
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

    if (loading) {
        return (
            <Box sx={{ display: 'flex', justifyContent: 'center', p: 4 }}>
                <CircularProgress />
            </Box>
        );
    }

    if (!state) {
        return (
            <Alert severity="error">
                Failed to load simulation state
            </Alert>
        );
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
                                {isLive ? (
                                    <Box sx={{ px: 1, py: 0.5, bgcolor: 'success.main', borderRadius: 1 }}>
                                        <Typography variant="caption" sx={{ color: 'white', fontWeight: 'bold' }}>
                                            LIVE
                                        </Typography>
                                    </Box>
                                ) : (
                                    <Box sx={{ px: 1, py: 0.5, bgcolor: 'warning.main', borderRadius: 1 }}>
                                        <Typography variant="caption" sx={{ color: 'white', fontWeight: 'bold' }}>
                                            MOCK
                                        </Typography>
                                    </Box>
                                )}
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
                                <Typography variant="h6">
                                    Market Scenarios
                                </Typography>
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
                            Configure realistic market conditions with regimes and patterns
                        </Typography>

                        {scenarioState?.isEnabled && (
                            <>
                                {/* Preset Selection */}
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
                                                {preset}
                                            </MenuItem>
                                        ))}
                                    </Select>
                                </FormControl>

                                {/* Active Scenario Info */}
                                {scenarioState.activeConfig && (
                                    <Box sx={{ bgcolor: 'grey.900', borderRadius: 1, p: 2, mb: 2 }}>
                                        <Typography variant="subtitle2" gutterBottom color="grey.100">
                                            Active: {scenarioState.activeConfig.name}
                                        </Typography>

                                        {/* Regime Timeline */}
                                        <Typography variant="caption" color="grey.400" display="block" sx={{ mb: 1 }}>
                                            Regime Timeline:
                                        </Typography>
                                        <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap', mb: 1 }}>
                                            {scenarioState.activeConfig.regimes?.map((regime, idx) => (
                                                <Tooltip
                                                    key={idx}
                                                    title={`${regime.bars} bars`}
                                                >
                                                    <Chip
                                                        size="small"
                                                        label={getRegimeDisplayName(regime.type as MarketRegime)}
                                                        sx={{
                                                            bgcolor: getRegimeColor(regime.type as MarketRegime),
                                                            color: 'white',
                                                            fontSize: '0.7rem',
                                                        }}
                                                    />
                                                </Tooltip>
                                            )) ?? null}
                                        </Box>

                                        {/* Pattern Overlays */}
                                        {(scenarioState.activeConfig.overlays?.length ?? 0) > 0 && (
                                            <Typography variant="caption" color="grey.400" display="block">
                                                Pattern Overlays: {scenarioState.activeConfig.overlays?.length ?? 0}
                                            </Typography>
                                        )}
                                    </Box>
                                )}

                                {/* Reset Button */}
                                <Button
                                    variant="outlined"
                                    startIcon={<RestartAltIcon />}
                                    onClick={handleScenarioReset}
                                    disabled={scenarioLoading}
                                    size="small"
                                >
                                    Reset to Default
                                </Button>
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
