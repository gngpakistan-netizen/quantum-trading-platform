'use client';
import { useEffect, useRef } from 'react';
import { createChart, type IChartApi, type ISeriesApi, type CandlestickSeriesPartialOptions } from 'lightweight-charts';
import { useTerminalStore } from '../lib/store';

export function ChartPanel() {
  const chartRef = useRef<HTMLDivElement>(null);
  const chartApi = useRef<IChartApi | null>(null);
  const series = useRef<ISeriesApi<'Candlestick'> | null>(null);
  const { price, timeframe } = useTerminalStore();

  useEffect(() => {
    if (!chartRef.current) return;

    const chart = createChart(chartRef.current, {
      layout: {
        background: { type: 'solid', color: '#0a0a0f' },
        textColor: '#6b6b80',
      },
      grid: {
        vertLines: { color: '#1e1e2a' },
        horzLines: { color: '#1e1e2a' },
      },
      crosshair: {
        mode: 0,
        vertLine: { color: '#2a2a3a', width: 1, style: 2, labelBackgroundColor: '#111118' },
        horzLine: { color: '#2a2a3a', width: 1, style: 2, labelBackgroundColor: '#111118' },
      },
      timeScale: {
        borderColor: '#1e1e2a',
        timeVisible: true,
        secondsVisible: false,
      },
      rightPriceScale: {
        borderColor: '#1e1e2a',
      },
      width: chartRef.current.clientWidth,
      height: chartRef.current.clientHeight,
    });

    const candleSeries = chart.addCandlestickSeries({
      upColor: '#00c853',
      downColor: '#ff1744',
      borderUpColor: '#00c853',
      borderDownColor: '#ff1744',
      wickUpColor: '#00c853',
      wickDownColor: '#ff1744',
    });

    chartApi.current = chart;
    series.current = candleSeries;

    const handleResize = () => {
      if (chartRef.current) {
        chart.applyOptions({
          width: chartRef.current.clientWidth,
          height: chartRef.current.clientHeight,
        });
      }
    };
    window.addEventListener('resize', handleResize);

    return () => {
      window.removeEventListener('resize', handleResize);
      chart.remove();
    };
  }, []);

  // Fetch candle data when timeframe changes
  useEffect(() => {
    if (!series.current) return;
    const api = useTerminalStore.getState().apiUrl;

    fetch(`${api}/api/v1/market/candles?symbol=XAUUSD&timeframe=${timeframe}&limit=200`)
      .then(res => res.json())
      .then(data => {
        if (data.candles && series.current) {
          series.current.setData(data.candles.map((c: any) => ({
            time: Math.floor(c.timestamp / 1000) as any,
            open: c.open,
            high: c.high,
            low: c.low,
            close: c.close,
          })));
        }
      })
      .catch(console.error);
  }, [timeframe]);

  return (
    <div className="panel" style={{ gridColumn: '2', gridRow: '2', position: 'relative' }}>
      <div ref={chartRef} style={{ width: '100%', height: '100%' }} />
      {/* Real-time price overlay */}
      <div className="absolute top-2 right-3 text-xs font-mono text-terminal-muted">
        <span className="text-terminal-green">● LIVE</span>
      </div>
    </div>
  );
}
