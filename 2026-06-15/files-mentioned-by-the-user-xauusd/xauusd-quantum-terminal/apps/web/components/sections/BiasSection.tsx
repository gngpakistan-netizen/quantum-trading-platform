'use client';
import { useTerminalStore } from '../../lib/store';
import { clsx } from 'clsx';

export function BiasSection() {
  const { bias } = useTerminalStore();

  const rows: Array<{ label: string; value: string; score: number; color: string }> = [
    { label: 'MACRO BIAS', value: bias.macro.toUpperCase(), score: 0, color: bias.macro === 'bullish' ? 'text-terminal-green' : bias.macro === 'bearish' ? 'text-terminal-red' : 'text-terminal-yellow' },
    { label: 'MICRO BIAS', value: bias.micro.toUpperCase(), score: 0, color: bias.micro === 'bullish' ? 'text-terminal-green' : bias.micro === 'bearish' ? 'text-terminal-red' : 'text-terminal-yellow' },
    { label: 'CURRENT BIAS', value: bias.current.toUpperCase(), score: 0, color: bias.current === 'bullish' ? 'text-terminal-green' : bias.current === 'bearish' ? 'text-terminal-red' : 'text-terminal-yellow' },
    { label: 'LONG-TERM BIAS', value: bias.longTerm.toUpperCase(), score: 0, color: bias.longTerm === 'bullish' ? 'text-terminal-green' : bias.longTerm === 'bearish' ? 'text-terminal-red' : 'text-terminal-yellow' },
  ];

  return (
    <>
      <div className="panel-header">BIAS</div>
      <div className="p-2">
        {rows.map(r => (
          <div key={r.label} className="data-row">
            <span className="data-label">{r.label}</span>
            <span className={`data-value ${r.color}`}>{r.value}</span>
          </div>
        ))}
        <div className="mt-2 px-2">
          <div className="text-2xs text-terminal-muted mb-1">CONVICTION</div>
          <div className="prob-bar">
            <div className="prob-bar-fill bg-terminal-blue" style={{ width: `${bias.conviction}%` }} />
          </div>
          <div className="text-right text-2xs font-mono text-terminal-muted mt-0.5">{bias.conviction.toFixed(0)}%</div>
        </div>
      </div>
    </>
  );
}
