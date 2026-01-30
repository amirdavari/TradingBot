import { useEffect, useRef } from 'react';
import { createChart, CandlestickSeries, HistogramSeries, type IChartApi, type ISeriesApi, type CandlestickData, type HistogramData, type Time } from 'lightweight-charts';
import type { Candle, TradeSignal } from '../models';
import Box from '@mui/material/Box';

interface CandlestickChartProps {
    candles: Candle[];
    signal?: TradeSignal | null;
}

export default function CandlestickChart({ candles, signal }: CandlestickChartProps) {
    const chartContainerRef = useRef<HTMLDivElement>(null);
    const chartRef = useRef<IChartApi | null>(null);
    const candlestickSeriesRef = useRef<ISeriesApi<'Candlestick'> | null>(null);
    const volumeSeriesRef = useRef<ISeriesApi<'Histogram'> | null>(null);
    const lastCandleCountRef = useRef<number>(0);
    const isDisposedRef = useRef<boolean>(false);

    useEffect(() => {
        if (!chartContainerRef.current) return;

        // Reset disposed flag on mount
        isDisposedRef.current = false;

        // Create chart
        const chart = createChart(chartContainerRef.current, {
            width: chartContainerRef.current.clientWidth,
            height: chartContainerRef.current.clientHeight,
            layout: {
                background: { color: '#1e1e1e' },
                textColor: '#d1d4dc',
            },
            grid: {
                vertLines: { color: '#2a2e39' },
                horzLines: { color: '#2a2e39' },
            },
            crosshair: {
                mode: 1,
            },
            rightPriceScale: {
                borderColor: '#2a2e39',
            },
            timeScale: {
                borderColor: '#2a2e39',
                timeVisible: true,
                secondsVisible: false,
            },
        });

        // Create candlestick series
        const candlestickSeries = chart.addSeries(CandlestickSeries, {
            upColor: '#26a69a',
            downColor: '#ef5350',
            borderVisible: false,
            wickUpColor: '#26a69a',
            wickDownColor: '#ef5350',
        });

        // Create volume histogram series
        const volumeSeries = chart.addSeries(HistogramSeries, {
            color: '#26a69a',
            priceFormat: {
                type: 'volume',
            },
            priceScaleId: '',
        });

        // Configure volume to use bottom 20% of chart
        volumeSeries.priceScale().applyOptions({
            scaleMargins: {
                top: 0.8,
                bottom: 0,
            },
        });

        chartRef.current = chart;
        candlestickSeriesRef.current = candlestickSeries;
        volumeSeriesRef.current = volumeSeries;

        // Handle resize with ResizeObserver for container changes
        const handleResize = () => {
            if (chartContainerRef.current && chartRef.current && !isDisposedRef.current) {
                const { clientWidth, clientHeight } = chartContainerRef.current;
                if (clientWidth > 0 && clientHeight > 0) {
                    chartRef.current.applyOptions({
                        width: clientWidth,
                        height: clientHeight,
                    });
                }
            }
        };

        // Use ResizeObserver for container size changes
        const resizeObserver = new ResizeObserver(handleResize);
        resizeObserver.observe(chartContainerRef.current);

        window.addEventListener('resize', handleResize);
        
        // Initial resize after a short delay to ensure container is rendered
        setTimeout(handleResize, 100);

        // Cleanup
        return () => {
            isDisposedRef.current = true;
            resizeObserver.disconnect();
            window.removeEventListener('resize', handleResize);
            if (chartRef.current) {
                chartRef.current.remove();
            }
            chartRef.current = null;
            candlestickSeriesRef.current = null;
            volumeSeriesRef.current = null;
        };
    }, []);

    // Update candle data
    useEffect(() => {
        // Guard against disposed chart
        if (isDisposedRef.current) return;
        if (!candlestickSeriesRef.current || !volumeSeriesRef.current || !candles.length || !chartRef.current) return;

        try {
            const timeScale = chartRef.current.timeScale();
            const currentCandleCount = candles.length;
            const hadCandles = lastCandleCountRef.current > 0;
            const newCandlesAdded = currentCandleCount > lastCandleCountRef.current;

            // Convert candles to lightweight-charts format
            const chartData: CandlestickData[] = candles.map((candle) => ({
                time: new Date(candle.time).getTime() / 1000 as Time,
                open: candle.open,
                high: candle.high,
                low: candle.low,
                close: candle.close,
            }));

            // Convert candles to volume data (colored by price direction)
            const volumeData: HistogramData[] = candles.map((candle) => ({
                time: new Date(candle.time).getTime() / 1000 as Time,
                value: candle.volume,
                color: candle.close >= candle.open ? '#26a69a80' : '#ef535080',
            }));

            // Use setData to replace all data (handles additions and updates)
            candlestickSeriesRef.current.setData(chartData);
            volumeSeriesRef.current.setData(volumeData);

            // Auto-scroll to latest data when new candles are added
            // or on first load
            if (!hadCandles || newCandlesAdded) {
                timeScale.fitContent();
            }
            // Otherwise, keep the user's current zoom/scroll position

            // Update the last candle count
            lastCandleCountRef.current = currentCandleCount;
        } catch (e) {
            // Chart may have been disposed during async operation
            console.warn('[CandlestickChart] Error updating candle data:', e);
        }
    }, [candles]);

    // Add signal lines (Entry, Stop-Loss, Take-Profit)
    useEffect(() => {
        // Guard against disposed chart
        if (isDisposedRef.current) return;

        const series = candlestickSeriesRef.current;
        if (!chartRef.current || !series || !signal || !candles.length) return;

        // Store price lines for cleanup
        // eslint-disable-next-line @typescript-eslint/no-explicit-any
        const priceLines: any[] = [];

        // Add Entry line
        if (signal.entry > 0) {
            const entryLine = series.createPriceLine({
                price: signal.entry,
                color: '#2196f3',
                lineWidth: 2,
                lineStyle: 2, // Dashed
                axisLabelVisible: true,
                title: 'Entry',
            });
            priceLines.push(entryLine);
        }

        // Add Stop-Loss line (red)
        if (signal.stopLoss > 0) {
            const slLine = series.createPriceLine({
                price: signal.stopLoss,
                color: '#f44336',
                lineWidth: 2,
                lineStyle: 0, // Solid
                axisLabelVisible: true,
                title: 'Stop Loss',
            });
            priceLines.push(slLine);
        }

        // Add Take-Profit line (green)
        if (signal.takeProfit > 0) {
            const tpLine = series.createPriceLine({
                price: signal.takeProfit,
                color: '#4caf50',
                lineWidth: 2,
                lineStyle: 0, // Solid
                axisLabelVisible: true,
                title: 'Take Profit',
            });
            priceLines.push(tpLine);
        }

        // Cleanup: Remove price lines when signal changes
        return () => {
            // Don't try to remove lines if chart is disposed
            if (isDisposedRef.current) return;

            priceLines.forEach(line => {
                try {
                    series.removePriceLine(line);
                } catch {
                    // Ignore errors if line already removed or chart disposed
                }
            });
        };
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [signal?.entry, signal?.stopLoss, signal?.takeProfit, candles.length]);

    return (
        <Box
            ref={chartContainerRef}
            sx={{
                width: '100%',
                height: '100%',
                minHeight: 350,
                position: 'relative',
            }}
        />
    );
}
