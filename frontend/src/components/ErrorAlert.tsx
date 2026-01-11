import { Alert, AlertTitle, Button } from '@mui/material';

interface ErrorAlertProps {
    error: string;
    onRetry?: () => void;
}

export default function ErrorAlert({ error, onRetry }: ErrorAlertProps) {
    return (
        <Alert
            severity="error"
            action={
                onRetry && (
                    <Button color="inherit" size="small" onClick={onRetry}>
                        Erneut versuchen
                    </Button>
                )
            }
        >
            <AlertTitle>Fehler</AlertTitle>
            {error}
        </Alert>
    );
}
