import { useState } from 'react';
import Box from '@mui/material/Box';
import TextField from '@mui/material/TextField';
import Button from '@mui/material/Button';
import Alert from '@mui/material/Alert';
import AddIcon from '@mui/icons-material/Add';

interface WatchlistAddFormProps {
    onAdd: (symbol: string) => Promise<void>;
}

export default function WatchlistAddForm({ onAdd }: WatchlistAddFormProps) {
    const [symbol, setSymbol] = useState('');
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        const trimmedSymbol = symbol.trim().toUpperCase();
        if (!trimmedSymbol) return;



        setLoading(true);
        setError(null);

        try {
            await onAdd(trimmedSymbol);
            setSymbol('');
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to add symbol');
        } finally {
            setLoading(false);
        }
    };

    return (
        <Box component="form" onSubmit={handleSubmit} sx={{ mb: 3 }}>
            <Box sx={{ display: 'flex', gap: 1, alignItems: 'flex-start' }}>
                <TextField
                    size="small"
                    placeholder="Add symbol (e.g., AAPL)"
                    value={symbol}
                    onChange={(e) => {
                        setSymbol(e.target.value.toUpperCase());
                        setError(null);
                    }}
                    disabled={loading}
                    sx={{ flexGrow: 1, maxWidth: 200 }}
                />
                <Button
                    type="submit"
                    variant="contained"
                    disabled={loading || !symbol.trim()}
                    startIcon={<AddIcon />}
                >
                    Add
                </Button>
            </Box>
            {error && (
                <Alert severity="error" sx={{ mt: 1 }}>
                    {error}
                </Alert>
            )}
        </Box>
    );
}
