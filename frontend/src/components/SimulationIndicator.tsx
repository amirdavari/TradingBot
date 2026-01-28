import { Box, Chip } from '@mui/material';
import { useReplayState } from '../hooks/useReplayState';

/**
 * Global simulation indicator that shows the current market mode.
 * Always visible in the header.
 * - LIVE mode: green "LIVE" badge (Yahoo Finance data)
 * - MOCK mode: yellow "MOCK" badge (simulated data)
 */
export default function SimulationIndicator() {
    const { state, loading } = useReplayState();

    if (loading || !state) {
        return null;
    }

    const isLive = state.mode === 'LIVE';

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
                <Chip
                    label="MOCK"
                    color="warning"
                    size="small"
                    sx={{
                        fontWeight: 'bold',
                        minWidth: 70,
                    }}
                />
            )}
        </Box>
    );
}
