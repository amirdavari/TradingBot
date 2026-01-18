import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Stack from '@mui/material/Stack';
import SentimentSatisfiedIcon from '@mui/icons-material/SentimentSatisfied';
import SentimentNeutralIcon from '@mui/icons-material/SentimentNeutral';
import SentimentDissatisfiedIcon from '@mui/icons-material/SentimentDissatisfied';
import LoadingSpinner from './LoadingSpinner';
import type { NewsItem } from '../models';

interface NewsPanelProps {
    news: NewsItem[];
    loading: boolean;
    symbol: string;
}

export default function NewsPanel({ news, loading, symbol }: NewsPanelProps) {
    const getSentimentIcon = (sentiment: string) => {
        switch (sentiment) {
            case 'positive': return <SentimentSatisfiedIcon color="success" />;
            case 'neutral': return <SentimentNeutralIcon color="warning" />;
            case 'negative': return <SentimentDissatisfiedIcon color="error" />;
            default: return <SentimentNeutralIcon />;
        }
    };

    return (
        <Box sx={{ p: 2, borderTop: 1, borderColor: 'divider', flexShrink: 0, maxHeight: '200px', overflow: 'auto' }}>
            <Typography variant="h6" gutterBottom>News</Typography>
            {loading ? (
                <Box sx={{ display: 'flex', justifyContent: 'center', py: 2 }}>
                    <LoadingSpinner />
                </Box>
            ) : news.length > 0 ? (
                <Stack spacing={1}>
                    {news.map((item, index) => (
                        <Stack key={index} direction="row" spacing={1} alignItems="flex-start">
                            {getSentimentIcon(item.sentiment)}
                            <Box sx={{ flexGrow: 1 }}>
                                <Typography variant="body2" sx={{ fontWeight: 'medium' }}>
                                    {item.title}
                                </Typography>
                                {item.summary && (
                                    <Typography variant="caption" color="text.secondary" sx={{ display: 'block', mt: 0.5 }}>
                                        {item.summary.length > 100 ? `${item.summary.substring(0, 100)}...` : item.summary}
                                    </Typography>
                                )}
                                <Typography variant="caption" color="text.secondary">
                                    {new Date(item.publishedAt).toLocaleString()} • {item.source}
                                </Typography>
                            </Box>
                        </Stack>
                    ))}
                </Stack>
            ) : (
                <Typography variant="body2" color="text.secondary">
                    No news available for {symbol}
                </Typography>
            )}
        </Box>
    );
}
