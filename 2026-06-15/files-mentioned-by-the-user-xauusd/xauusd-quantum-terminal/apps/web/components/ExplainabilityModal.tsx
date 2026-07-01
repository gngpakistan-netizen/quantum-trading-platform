'use client';
import { useTerminalStore } from '../lib/store';
import type { CalculationTrace } from '@xauusd/common';

export function ExplainabilityModal({ trace, onClose }: { trace: CalculationTrace; onClose: () => void }) {
  return (
    <div className="fixed inset-0 bg-black/60 z-50 flex items-center justify-center" onClick={onClose}>
      <div className="bg-terminal-panel border border-terminal-border rounded-lg w-[640px] max-h-[80vh] overflow-y-auto" onClick={e => e.stopPropagation()}>
        {/* Header */}
        <div className="flex justify-between items-center p-4 border-b border-terminal-border">
          <div>
            <div className="text-sm font-semibold text-white">Calculation Trace</div>
            <div className="text-2xs text-terminal-muted font-mono mt-0.5">{trace.traceId}</div>
          </div>
          <button onClick={onClose} className="text-terminal-muted hover:text-white text-lg">&times;</button>
        </div>

        {/* Engine + Version */}
        <div className="px-4 py-2 border-b border-terminal-border flex gap-4 text-xs">
          <span className="text-terminal-muted">Engine: <span className="text-white font-mono">{trace.engine}</span></span>
          <span className="text-terminal-muted">Version: <span className="text-white font-mono">v{trace.version}</span></span>
          <span className="text-terminal-muted">
            Timestamp: <span className="text-white font-mono">{new Date(trace.timestamp).toISOString()}</span>
          </span>
        </div>

        {/* Formula */}
        <div className="p-4 border-b border-terminal-border">
          <div className="text-2xs text-terminal-muted mb-1 uppercase tracking-wider">Formula</div>
          <pre className="text-xs font-mono text-terminal-cyan bg-terminal-bg rounded p-2 overflow-x-auto">{trace.formula}</pre>
        </div>

        {/* Inputs */}
        <div className="p-4 border-b border-terminal-border">
          <div className="text-2xs text-terminal-muted mb-1 uppercase tracking-wider">Inputs</div>
          <div className="grid grid-cols-2 gap-1">
            {Object.entries(trace.inputs).map(([key, val]) => (
              <div key={key} className="data-row">
                <span className="data-label">{key}</span>
                <span className="data-value text-white">{String(val)}</span>
              </div>
            ))}
          </div>
        </div>

        {/* Intermediate Steps */}
        {trace.intermediates.length > 0 && (
          <div className="p-4 border-b border-terminal-border">
            <div className="text-2xs text-terminal-muted mb-1 uppercase tracking-wider">Intermediate Calculations</div>
            {trace.intermediates.map((step, i) => (
              <div key={i} className="mb-2 bg-terminal-bg rounded p-2">
                <div className="text-2xs text-terminal-muted mb-0.5">Step {i + 1}: {step.step}</div>
                <div className="text-xs font-mono text-white">{step.explanation}</div>
                <div className="text-xs font-mono text-terminal-green mt-0.5">= {step.value.toFixed(6)}</div>
              </div>
            ))}
          </div>
        )}

        {/* Output */}
        <div className="p-4">
          <div className="text-2xs text-terminal-muted mb-1 uppercase tracking-wider">Output</div>
          <pre className="text-xs font-mono text-terminal-green bg-terminal-bg rounded p-2">
            {JSON.stringify(trace.output, null, 2)}
          </pre>
        </div>
      </div>
    </div>
  );
}
