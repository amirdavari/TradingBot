import { Component } from 'react';
import type { ReactNode } from 'react';
import Box from '@mui/material/Box';
import Paper from '@mui/material/Paper';
import Typography from '@mui/material/Typography';
import Button from '@mui/material/Button';
import Alert from '@mui/material/Alert';
import AlertTitle from '@mui/material/AlertTitle';

interface Props {
    children: ReactNode;
}

interface State {
    hasError: boolean;
    error: Error | null;
}

export default class ErrorBoundary extends Component<Props, State> {
    constructor(props: Props) {
        super(props);
        this.state = { hasError: false, error: null };
    }

    static getDerivedStateFromError(error: Error): State {
        return { hasError: true, error };
    }

    componentDidCatch(error: Error, errorInfo: React.ErrorInfo) {
        console.error('ErrorBoundary caught an error:', error, errorInfo);
    }

    handleReset = () => {
        this.setState({ hasError: false, error: null });
    };

    render() {
        if (this.state.hasError) {
            return (
                <Box
                    sx={{
                        display: 'flex',
                        justifyContent: 'center',
                        alignItems: 'center',
                        minHeight: '100vh',
                        p: 3,
                    }}
                >
                    <Paper sx={{ p: 4, maxWidth: 600 }}>
                        <Alert severity="error">
                            <AlertTitle>Etwas ist schiefgelaufen</AlertTitle>
                            <Typography variant="body2" sx={{ mt: 2 }}>
                                {this.state.error?.message || 'Ein unerwarteter Fehler ist aufgetreten'}
                            </Typography>
                        </Alert>
                        <Button
                            variant="contained"
                            onClick={this.handleReset}
                            sx={{ mt: 3 }}
                            fullWidth
                        >
                            Neu laden
                        </Button>
                    </Paper>
                </Box>
            );
        }

        return this.props.children;
    }
}
