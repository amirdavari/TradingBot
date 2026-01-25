import { Box, Typography, Chip } from '@mui/material';
import PlayArrowIcon from '@mui/icons-material/PlayArrow';
import PauseIcon from '@mui/icons-material/Pause';
import SpeedIcon from '@mui/icons-material/Speed';
import { useReplayState } from '../hooks/useReplayState';

/**
 * Global simulation indicator that shows the current market mode.
 * Always visible in the header.
 * - LIVE mode: green indicator
 * - REPLAY mode: yellow indicator with date, time, and speed
 */
export default function SimulationIndicator() {
    const { state, loading } = useReplayState(); // Uses SignalR for real-time updates

    if (loading || !state) {
        return null;
    }

    const isLive = state.mode === 'LIVE';
    const isRunning = state.isRunning;

    // Format the current time for display
    const formatDateTime = (dateString: string) => {
        const date = new Date(dateString);
        return date.toLocaleString('de-DE', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit',
        });
    };

    return (
        <Box
            sx={{
                display: 'flex',
                alignItems: 'center',
                gap: 1,
                ml: 'auto',
            }}
        >
            {isLive ? (
                // Live mode indicator
                <Chip
                    label="LIVE"
                    color="success"
                    size="small"
                    sx={{
                        fontWeight: 'bold',
                        minWidth: 70,
                    }}
                />
            ) : (
                // Replay mode indicator
                <>
                    <Chip
                        label="SIMULATION"
                        color="warning"
                        size="small"
                        sx={{
                            fontWeight: 'bold',
                            minWidth: 100,
                        }}
                    />
                    <Box
                        sx={{
                            display: 'flex',
                            alignItems: 'center',
                            gap: 1,
                            bgcolor: 'rgba(255, 255, 255, 0.1)',
                            px: 1.5,
                            py: 0.5,
                            borderRadius: 1,
                        }}
                    >
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                            {isRunning ? (
                                <PlayArrowIcon sx={{ fontSize: 16, color: 'success.main' }} />
                            ) : (
                                <PauseIcon sx={{ fontSize: 16, color: 'warning.main' }} />
                            )}
                            <Typography variant="caption" sx={{ fontWeight: 500 }}>
                                {formatDateTime(state.currentTime)}
                            </Typography>
                        </Box>
                        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
                            <SpeedIcon sx={{ fontSize: 16 }} />
                            <Typography variant="caption" sx={{ fontWeight: 500 }}>
                                {state.speed}x
                            </Typography>
                        </Box>
                    </Box>
                </>
            )}
        </Box>
    );
}
