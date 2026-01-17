import { useState } from 'react';
import {
    Box,
    Paper,
    Typography,
    Button,
    ButtonGroup,
    TextField,
    Switch,
    FormControlLabel,
    Stack,
    Divider,
    Alert,
    CircularProgress,
} from '@mui/material';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import PauseIcon from '@mui/icons-material/Pause';
import RestartAltIcon from '@mui/icons-material/RestartAlt';
import { useReplayState } from '../hooks/useReplayState';

/**
 * Simulation Control Panel for development and testing.
 * Allows switching between Live and Replay mode and controlling replay simulation.
 */
export default function SimulationControlPanel() {
    const {
        state,
        loading,
        error,
        start,
        pause,
        reset,
        setSpeed,
        setTime,
        setMode,
    } = useReplayState(2000); // Poll every 2 seconds for responsive UI

    const [selectedDate, setSelectedDate] = useState('');
    const [actionLoading, setActionLoading] = useState(false);
    const [actionError, setActionError] = useState<string | null>(null);

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
    const isRunning = state.isRunning;

    /**
     * Handle mode toggle between Live and Replay
     */
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

    /**
     * Handle speed change
     */
    const handleSpeedChange = async (speed: number) => {
        setActionLoading(true);
        setActionError(null);
        try {
            await setSpeed(speed);
        } catch (err) {
            setActionError(err instanceof Error ? err.message : 'Failed to set speed');
        } finally {
            setActionLoading(false);
        }
    };

    /**
     * Handle replay date/time change
     */
    const handleSetReplayTime = async () => {
        if (!selectedDate) {
            setActionError('Please select a date and time');
            return;
        }

        setActionLoading(true);
        setActionError(null);
        try {
            // Convert local datetime-local input to ISO string
            const date = new Date(selectedDate);
            await setTime(date.toISOString());
            setSelectedDate('');
        } catch (err) {
            setActionError(err instanceof Error ? err.message : 'Failed to set time');
        } finally {
            setActionLoading(false);
        }
    };

    /**
     * Handle start/pause/reset actions
     */
    const handleStart = async () => {
        setActionLoading(true);
        setActionError(null);
        try {
            await start();
        } catch (err) {
            setActionError(err instanceof Error ? err.message : 'Failed to start');
        } finally {
            setActionLoading(false);
        }
    };

    const handlePause = async () => {
        setActionLoading(true);
        setActionError(null);
        try {
            await pause();
        } catch (err) {
            setActionError(err instanceof Error ? err.message : 'Failed to pause');
        } finally {
            setActionLoading(false);
        }
    };

    const handleReset = async () => {
        setActionLoading(true);
        setActionError(null);
        try {
            await reset();
        } catch (err) {
            setActionError(err instanceof Error ? err.message : 'Failed to reset');
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
                Development tool for testing with historical data
            </Typography>

            {(error || actionError) && (
                <Alert severity="error" sx={{ mb: 2 }} onClose={() => setActionError(null)}>
                    {error || actionError}
                </Alert>
            )}

            <Stack spacing={3}>
                {/* Mode Toggle */}
                <Paper sx={{ p: 3 }}>
                    <Typography variant="h6" gutterBottom>
                        Market Mode
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
                                    {isLive ? 'Live Mode' : 'Replay Mode'}
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
                                            SIMULATION
                                        </Typography>
                                    </Box>
                                )}
                            </Box>
                        }
                    />
                    <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 1 }}>
                        Toggle between live market data and historical replay
                    </Typography>
                </Paper>

                {/* Replay Controls - Only visible in Replay mode */}
                {!isLive && (
                    <>
                        {/* Date/Time Selection */}
                        <Paper sx={{ p: 3 }}>
                            <Typography variant="h6" gutterBottom>
                                Replay Start Time
                            </Typography>
                            <Box sx={{ display: 'flex', gap: 2, alignItems: 'flex-start' }}>
                                <TextField
                                    type="datetime-local"
                                    value={selectedDate}
                                    onChange={(e) => setSelectedDate(e.target.value)}
                                    disabled={actionLoading}
                                    fullWidth
                                    InputLabelProps={{ shrink: true }}
                                    helperText="Select a date and time in the past"
                                />
                                <Button
                                    variant="contained"
                                    onClick={handleSetReplayTime}
                                    disabled={actionLoading || !selectedDate}
                                    sx={{ minWidth: 100 }}
                                >
                                    Set Time
                                </Button>
                            </Box>
                            <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 1 }}>
                                Current: {new Date(state.currentTime).toLocaleString('de-DE')}
                            </Typography>
                        </Paper>

                        {/* Speed Control */}
                        <Paper sx={{ p: 3 }}>
                            <Typography variant="h6" gutterBottom>
                                Replay Speed
                            </Typography>
                            <ButtonGroup variant="outlined" fullWidth disabled={actionLoading}>
                                <Button
                                    onClick={() => handleSpeedChange(1)}
                                    variant={state.speed === 1 ? 'contained' : 'outlined'}
                                >
                                    1x
                                </Button>
                                <Button
                                    onClick={() => handleSpeedChange(5)}
                                    variant={state.speed === 5 ? 'contained' : 'outlined'}
                                >
                                    5x
                                </Button>
                                <Button
                                    onClick={() => handleSpeedChange(10)}
                                    variant={state.speed === 10 ? 'contained' : 'outlined'}
                                >
                                    10x
                                </Button>
                            </ButtonGroup>
                            <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 1 }}>
                                Current speed: {state.speed}x
                            </Typography>
                        </Paper>

                        <Divider />

                        {/* Playback Controls */}
                        <Paper sx={{ p: 3 }}>
                            <Typography variant="h6" gutterBottom>
                                Playback Controls
                            </Typography>
                            <Box sx={{ display: 'flex', gap: 2 }}>
                                {isRunning ? (
                                    <Button
                                        variant="contained"
                                        color="warning"
                                        startIcon={<PauseIcon />}
                                        onClick={handlePause}
                                        disabled={actionLoading}
                                        fullWidth
                                    >
                                        Pause
                                    </Button>
                                ) : (
                                    <Button
                                        variant="contained"
                                        color="success"
                                        startIcon={<PlayArrowIcon />}
                                        onClick={handleStart}
                                        disabled={actionLoading}
                                        fullWidth
                                    >
                                        Start
                                    </Button>
                                )}
                                <Button
                                    variant="outlined"
                                    startIcon={<RestartAltIcon />}
                                    onClick={handleReset}
                                    disabled={actionLoading}
                                    fullWidth
                                >
                                    Reset
                                </Button>
                            </Box>
                            <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 1 }}>
                                Status: {isRunning ? 'Running' : 'Paused'}
                            </Typography>
                        </Paper>
                    </>
                )}

                {/* Current State Display */}
                <Paper sx={{ p: 3, bgcolor: 'grey.50' }}>
                    <Typography variant="h6" gutterBottom>
                        Current State
                    </Typography>
                    <Stack spacing={1}>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                            <Typography variant="body2" color="text.secondary">Mode:</Typography>
                            <Typography variant="body2" fontWeight="bold">{state.mode}</Typography>
                        </Box>
                        <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                            <Typography variant="body2" color="text.secondary">Current Time:</Typography>
                            <Typography variant="body2" fontWeight="bold">
                                {new Date(state.currentTime).toLocaleString('de-DE')}
                            </Typography>
                        </Box>
                        {!isLive && (
                            <>
                                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                                    <Typography variant="body2" color="text.secondary">Start Time:</Typography>
                                    <Typography variant="body2" fontWeight="bold">
                                        {new Date(state.replayStartTime).toLocaleString('de-DE')}
                                    </Typography>
                                </Box>
                                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                                    <Typography variant="body2" color="text.secondary">Speed:</Typography>
                                    <Typography variant="body2" fontWeight="bold">{state.speed}x</Typography>
                                </Box>
                                <Box sx={{ display: 'flex', justifyContent: 'space-between' }}>
                                    <Typography variant="body2" color="text.secondary">Running:</Typography>
                                    <Typography variant="body2" fontWeight="bold">
                                        {state.isRunning ? 'Yes' : 'No'}
                                    </Typography>
                                </Box>
                            </>
                        )}
                    </Stack>
                </Paper>
            </Stack>
        </Box>
    );
}
