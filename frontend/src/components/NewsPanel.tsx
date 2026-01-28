import Box from '@mui/material/Box';
import Typography from '@mui/material/Typography';
import Table from '@mui/material/Table';
import TableBody from '@mui/material/TableBody';
import TableCell from '@mui/material/TableCell';
import TableContainer from '@mui/material/TableContainer';
import TableHead from '@mui/material/TableHead';
import TableRow from '@mui/material/TableRow';
import Chip from '@mui/material/Chip';
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
            case 'positive': return <SentimentSatisfiedIcon fontSize="small" color="success" />;
            case 'neutral': return <SentimentNeutralIcon fontSize="small" color="warning" />;
            case 'negative': return <SentimentDissatisfiedIcon fontSize="small" color="error" />;
            default: return <SentimentNeutralIcon fontSize="small" />;
        }
    };

    const getSentimentColor = (sentiment: string): "success" | "warning" | "error" | "default" => {
        switch (sentiment) {
            case 'positive': return 'success';
            case 'neutral': return 'warning';
            case 'negative': return 'error';
            default: return 'default';
        }
    };

    // Sort news by publishedAt descending (newest first)
    const sortedNews = [...news].sort((a, b) => 
        new Date(b.publishedAt).getTime() - new Date(a.publishedAt).getTime()
    );

    return (
        <Box sx={{ p: 1, borderTop: 1, borderColor: 'divider', flexShrink: 0, maxHeight: '140px', overflow: 'auto' }}>
            <Typography variant="subtitle2" gutterBottom>News</Typography>
            {loading ? (
                <Box sx={{ display: 'flex', justifyContent: 'center', py: 1 }}>
                    <LoadingSpinner />
                </Box>
            ) : news.length > 0 ? (
                <TableContainer>
                    <Table size="small" sx={{ minWidth: 650 }}>
                        <TableHead>
                            <TableRow sx={{ '& th': { py: 0.25, fontSize: '0.7rem' } }}>
                                <TableCell width="100px"><strong>Time</strong></TableCell>
                                <TableCell width="80px"><strong>Sentiment</strong></TableCell>
                                <TableCell><strong>Title</strong></TableCell>
                                <TableCell width="80px"><strong>Source</strong></TableCell>
                            </TableRow>
                        </TableHead>
                        <TableBody>
                            {sortedNews.map((item, index) => (
                                <TableRow 
                                    key={index}
                                    hover
                                    sx={{ 
                                        '&:last-child td, &:last-child th': { border: 0 },
                                        cursor: 'default',
                                        '& td': { py: 0.25 }
                                    }}
                                >
                                    <TableCell>
                                        <Typography variant="caption" sx={{ fontSize: '0.65rem' }}>
                                            {new Date(item.publishedAt).toLocaleString('de-DE', {
                                                day: '2-digit',
                                                month: '2-digit',
                                                hour: '2-digit',
                                                minute: '2-digit'
                                            })}
                                        </Typography>
                                    </TableCell>
                                    <TableCell>
                                        <Chip 
                                            icon={getSentimentIcon(item.sentiment)}
                                            label={item.sentiment}
                                            size="small"
                                            color={getSentimentColor(item.sentiment)}
                                            sx={{ textTransform: 'capitalize', height: 18, fontSize: '0.6rem', '& .MuiChip-icon': { fontSize: '0.8rem' } }}
                                        />
                                    </TableCell>
                                    <TableCell>
                                        <Typography variant="caption" sx={{ fontWeight: 'medium', fontSize: '0.7rem' }}>
                                            {item.title.length > 80 ? `${item.title.substring(0, 80)}...` : item.title}
                                        </Typography>
                                    </TableCell>
                                    <TableCell>
                                        <Typography variant="caption" color="text.secondary" sx={{ fontSize: '0.65rem' }}>
                                            {item.source}
                                        </Typography>
                                    </TableCell>
                                </TableRow>
                            ))}
                        </TableBody>
                    </Table>
                </TableContainer>
            ) : (
                <Typography variant="caption" color="text.secondary">
                    No news available for {symbol}
                </Typography>
            )}
        </Box>
    );
}
