import { useState, useEffect } from 'react';
import {
    Dialog,
    DialogTitle,
    DialogContent,
    DialogActions,
    Button,
    TextField,
    Stack,
    Typography,
    Box
} from '@mui/material';
import SettingsIcon from '@mui/icons-material/Settings';
import { getRiskSettings, updateRiskSettings } from '../api/tradingApi';
import type { RiskSettings } from '../models';

export default function RiskSettingsDialog() {
    const [open, setOpen] = useState(false);
    const [settings, setSettings] = useState<RiskSettings | null>(null);
    const [loading, setLoading] = useState(false);
    const [saving, setSaving] = useState(false);

    useEffect(() => {
        if (open) {
            fetchSettings();
        }
    }, [open]);

    const fetchSettings = async () => {
        try {
            setLoading(true);
            const data = await getRiskSettings();
            setSettings(data);
        } catch (error) {
            console.error('Failed to fetch risk settings:', error);
        } finally {
            setLoading(false);
        }
    };

    const handleSave = async () => {
        if (!settings) return;

        try {
            setSaving(true);
            await updateRiskSettings(settings);
            setOpen(false);
        } catch (error) {
            console.error('Failed to update risk settings:', error);
            alert('Fehler beim Speichern der Einstellungen');
        } finally {
            setSaving(false);
        }
    };

    return (
        <>
            <Button
                variant="outlined"
                size="small"
                startIcon={<SettingsIcon />}
                onClick={() => setOpen(true)}
            >
                Risk Settings
            </Button>

            <Dialog open={open} onClose={() => setOpen(false)} maxWidth="sm" fullWidth>
                <DialogTitle>Risk Management Einstellungen</DialogTitle>
                <DialogContent>
                    {loading ? (
                        <Box display="flex" justifyContent="center" p={3}>
                            <Typography>Lade Einstellungen...</Typography>
                        </Box>
                    ) : settings ? (
                        <Stack spacing={3} sx={{ mt: 2 }}>
                            <Box>
                                <Typography variant="subtitle2" gutterBottom>
                                    Default Risk per Trade (%)
                                </Typography>
                                <TextField
                                    type="number"
                                    fullWidth
                                    value={settings.defaultRiskPercent}
                                    onChange={(e) => setSettings({
                                        ...settings,
                                        defaultRiskPercent: Number(e.target.value)
                                    })}
                                    inputProps={{ min: 0.1, max: settings.maxRiskPercent, step: 0.1 }}
                                    helperText="Standard-Risiko pro Trade (z.B. 1% = 100€ bei 10.000€ Balance)"
                                />
                            </Box>

                            <Box>
                                <Typography variant="subtitle2" gutterBottom>
                                    Max Risk per Trade (%)
                                </Typography>
                                <TextField
                                    type="number"
                                    fullWidth
                                    value={settings.maxRiskPercent}
                                    onChange={(e) => setSettings({
                                        ...settings,
                                        maxRiskPercent: Number(e.target.value)
                                    })}
                                    inputProps={{ min: 0.1, max: 10, step: 0.1 }}
                                    helperText="Maximum erlaubtes Risiko pro Trade"
                                />
                            </Box>

                            <Box>
                                <Typography variant="subtitle2" gutterBottom>
                                    Min Risk/Reward Ratio
                                </Typography>
                                <TextField
                                    type="number"
                                    fullWidth
                                    value={settings.minRiskRewardRatio}
                                    onChange={(e) => setSettings({
                                        ...settings,
                                        minRiskRewardRatio: Number(e.target.value)
                                    })}
                                    inputProps={{ min: 0.5, max: 5, step: 0.1 }}
                                    helperText="Mindest-Verhältnis von Gewinn zu Verlust (z.B. 1.5 = 1:1.5)"
                                />
                            </Box>

                            <Box>
                                <Typography variant="subtitle2" gutterBottom>
                                    Max Capital Allocation per Trade (%)
                                </Typography>
                                <TextField
                                    type="number"
                                    fullWidth
                                    value={settings.maxCapitalPercent}
                                    onChange={(e) => setSettings({
                                        ...settings,
                                        maxCapitalPercent: Number(e.target.value)
                                    })}
                                    inputProps={{ min: 1, max: 100, step: 1 }}
                                    helperText="Maximum Kapital-Einsatz pro Trade (z.B. 20% = max 2.000€ bei 10.000€)"
                                />
                            </Box>
                        </Stack>
                    ) : null}
                </DialogContent>
                <DialogActions>
                    <Button onClick={() => setOpen(false)}>Abbrechen</Button>
                    <Button
                        onClick={handleSave}
                        variant="contained"
                        disabled={!settings || saving}
                    >
                        {saving ? 'Speichern...' : 'Speichern'}
                    </Button>
                </DialogActions>
            </Dialog>
        </>
    );
}
