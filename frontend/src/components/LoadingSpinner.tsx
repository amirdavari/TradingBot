import Box from '@mui/material/Box';
import CircularProgress from '@mui/material/CircularProgress';
import Typography from '@mui/material/Typography';

interface LoadingSpinnerProps {
    message?: string;
}

export default function LoadingSpinner({ message = 'Laden...' }: LoadingSpinnerProps) {
    return (
        <Box
            sx={{
                display: 'flex',
                flexDirection: 'column',
                justifyContent: 'center',
                alignItems: 'center',
                minHeight: 200,
                gap: 2,
            }}
        >
            <CircularProgress />
            <Typography variant="body2" color="text.secondary">
                {message}
            </Typography>
        </Box>
    );
}
