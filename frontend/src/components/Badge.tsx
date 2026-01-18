import Chip from '@mui/material/Chip';

interface ScoreBadgeProps {
    score: number;
}

export function ScoreBadge({ score }: ScoreBadgeProps) {
    const getColor = () => {
        if (score >= 75) return 'success';
        if (score >= 60) return 'warning';
        return 'error';
    };

    const getLabel = () => {
        if (score >= 75) return 'GOOD';
        if (score >= 60) return 'WATCH';
        return 'SKIP';
    };

    return (
        <Chip
            label={`${score} - ${getLabel()}`}
            color={getColor()}
            size="small"
            sx={{ fontWeight: 'bold', minWidth: 90 }}
        />
    );
}

interface TrendBadgeProps {
    trend: 'LONG' | 'SHORT' | 'NONE';
}

export function TrendBadge({ trend }: TrendBadgeProps) {
    const getColor = () => {
        if (trend === 'LONG') return 'success';
        if (trend === 'SHORT') return 'error';
        return 'default';
    };

    return (
        <Chip
            label={trend}
            color={getColor()}
            size="small"
            sx={{ minWidth: 70 }}
        />
    );
}

interface VolumeBadgeProps {
    status: 'LOW' | 'MEDIUM' | 'HIGH';
}

export function VolumeBadge({ status }: VolumeBadgeProps) {
    const getColor = () => {
        if (status === 'HIGH') return 'success';
        if (status === 'MEDIUM') return 'warning';
        return 'default';
    };

    return (
        <Chip
            label={status}
            color={getColor()}
            size="small"
            variant="outlined"
            sx={{ minWidth: 75 }}
        />
    );
}

interface NewsBadgeProps {
    hasNews: boolean;
}

export function NewsBadge({ hasNews }: NewsBadgeProps) {
    return (
        <Chip
            label={hasNews ? 'YES' : 'NO'}
            color={hasNews ? 'info' : 'default'}
            size="small"
            variant="outlined"
            sx={{ minWidth: 50 }}
        />
    );
}

interface ConfidenceBadgeProps {
    confidence: number;
}

export function ConfidenceBadge({ confidence }: ConfidenceBadgeProps) {
    const percentage = Math.round(confidence);
    const getColor = () => {
        if (percentage >= 70) return 'success';
        if (percentage >= 50) return 'warning';
        return 'error';
    };

    return (
        <Chip
            label={`${percentage}%`}
            color={getColor()}
            size="small"
            sx={{ minWidth: 55 }}
        />
    );
}
