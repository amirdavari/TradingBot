import { useEffect, useRef } from 'react';
import { createChart, CandlestickSeries, type IChartApi, type ISeriesApi, type CandlestickData, type Time } from 'lightweight-charts';
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

    useEffect(() => {
        if (!chartContainerRef.current) return;

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

        chartRef.current = chart;
        candlestickSeriesRef.current = candlestickSeries;

        // Handle resize
        const handleResize = () => {
            if (chartContainerRef.current && chart) {
                chart.applyOptions({
                    width: chartContainerRef.current.clientWidth,
                    height: chartContainerRef.current.clientHeight,
                });
            }
        };

        window.addEventListener('resize', handleResize);

        // Cleanup
        return () => {
            window.removeEventListener('resize', handleResize);
            chart.remove();
            chartRef.current = null;
            candlestickSeriesRef.current = null;
        };
    }, []);

    // Update candle data
    useEffect(() => {
        if (!candlestickSeriesRef.current || !candles.length) return;

        // Convert candles to lightweight-charts format
        const chartData: CandlestickData[] = candles.map((candle) => ({
            time: new Date(candle.time).getTime() / 1000 as Time,
            open: candle.open,
            high: candle.high,
            low: candle.low,
            close: candle.close,
        }));

        candlestickSeriesRef.current.setData(chartData);
    }, [candles]);

    // Add signal lines (Entry, Stop-Loss, Take-Profit)
    useEffect(() => {
        if (!chartRef.current || !signal || !candles.length) return;

        // Remove existing price lines
        const series = candlestickSeriesRef.current;
        if (!series) return;

        // Add Entry line
        if (signal.entry > 0) {
            series.createPriceLine({
                price: signal.entry,
                color: '#2196f3',
                lineWidth: 2,
                lineStyle: 2, // Dashed
                axisLabelVisible: true,
                title: 'Entry',
            });
        }

        // Add Stop-Loss line (red)
        if (signal.stopLoss > 0) {
            series.createPriceLine({
                price: signal.stopLoss,
                color: '#f44336',
                lineWidth: 2,
                lineStyle: 0, // Solid
                axisLabelVisible: true,
                title: 'Stop Loss',
            });
        }

        // Add Take-Profit line (green)
        if (signal.takeProfit > 0) {
            series.createPriceLine({
                price: signal.takeProfit,
                color: '#4caf50',
                lineWidth: 2,
                lineStyle: 0, // Solid
                axisLabelVisible: true,
                title: 'Take Profit',
            });
        }

        // Note: Price lines are automatically removed when series is removed
    }, [signal, candles]);

    return (
        <Box
            ref={chartContainerRef}
            sx={{
                width: '100%',
                height: '100%',
                minHeight: 400,
            }}
        />
    );
}
