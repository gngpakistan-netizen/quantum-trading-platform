'use client';
import { useTerminalStore } from '../lib/store';
import { clsx } from 'clsx';

const TIMEFRAMES = ['1m', '5m', '15m', '30m', '1h', '4h', '1d'] as const;

export function TopBar() {
  const { price, change, changePercent, timeframe, setTimeframe, symbol, wsConnected, bias } = useTerminalStore();
  const isUp = change >= 0;

  const biasColor = bias.current === 'bullish' ? 'text-terminal-green' :
    bias.current === 'bearish' ? 'text-terminal-red' : 'text-terminal-yellow';

  return (
    <div className="top-bar">
      <div className="top-bar-title">XAUUSD QUANTUM 3.0</div>

      <div className="flex items-center gap-2">
        <span className="text-xs text-terminal-muted">{symbol}</span>
        <span className={`top-bar-price ${isUp ? 'text-terminal-green' : 'text-terminal-red'}`}>
          {price.toFixed(2)}
        </span>
        <span className={`top-bar-change ${isUp ? 'text-terminal-green' : 'text-terminal-red'}`}>
          {isUp ? '+' : ''}{change.toFixed(2)} ({isUp ? '+' : ''}{changePercent.toFixed(2)}%)
        </span>
      </div>

      <div className="flex gap-1">
        {TIMEFRAMES.map(tf => (
          <button key={tf}
            onClick={() => setTimeframe(tf)}
            className={clsx(
              'px-2 py-0.5 text-xs font-mono rounded transition-colors',
              timeframe === tf
                ? 'bg-terminal-highlight text-white'
                : 'text-terminal-muted hover:text-white hover:bg-terminal-highlight/50'
            )}
          >
            {tf.toUpperCase()}
          </button>
        ))}
      </div>

      <div className="flex items-center gap-3 ml-auto">
        <span className={`text-2xs ${bias.current === 'bullish' ? 'text-terminal-green' : bias.current === 'bearish' ? 'text-terminal-red' : 'text-terminal-yellow'}`}>
          BIAS: {bias.current.toUpperCase()}
        </span>
        <span className={`w-2 h-2 rounded-full ${wsConnected ? 'bg-terminal-green' : 'bg-terminal-red'}`} />
      </div>
    </div>
  );
}
