'use client';
import { useTerminalStore } from '../lib/store';
import { clsx } from 'clsx';

export function RightPanel() {
  const { probability, smc, calibrationCurves, brierScore, calibrationBrier, refinementBrier, expectedValue, evAction } = useTerminalStore();

  return (
    <div className="panel space-y-2 overflow-y-auto" style={{ gridColumn: '3', gridRow: '2' }}>
      {/* Probability Section */}
      <div className="panel-header">PROBABILITY</div>
      <div className="px-2">
        <ProbRow label="BULLISH" value={probability?.bullish ?? 50} color="#22c55e" />
        <ProbRow label="BEARISH" value={probability?.bearish ?? 50} color="#ef4444" />
        <ProbRow label="PDH SWEEP" value={probability?.pdhSweep ?? 50} color="#3b82f6" />
        <ProbRow label="PDL SWEEP" value={probability?.pdlSweep ?? 50} color="#eab308" />
        <ProbRow label="CONTINUATION" value={probability?.continuation ?? 50} color="#06b6d4" />
        <ProbRow label="REVERSAL" value={probability?.reversal ?? 50} color="#a855f7" />
        <ProbRow label="MEAN REVERSION" value={probability?.meanReversion ?? 50} color="#6b7280" />
      </div>

      {/* Signals Section */}
      <div className="panel-header">SIGNALS</div>
      <div className="px-2 space-y-0.5">
        <SignalRow label="BOS BULL" active={smc?.bos?.bull} />
        <SignalRow label="BOS BEAR" active={smc?.bos?.bear} />
        <SignalRow label="CHOCH" active={!!smc?.choch} />
        <SignalRow label="MSS" active={!!smc?.mss} />
        <SignalRow label="FVG" active={!!(smc?.fvg?.bullish?.length || smc?.fvg?.bearish?.length)} />
        <SignalRow label="OB" active={!!(smc?.orderBlocks?.length)} />
        <SignalRow label="LIQ SWEEP" active={!!(smc?.liquidity?.sweptAbove || smc?.liquidity?.sweptBelow)} />
      </div>

      {/* Calibration Section */}
      <div className="panel-header">CALIBRATION</div>
      <div className="px-2">
        <div className="grid grid-cols-2 gap-1 mb-2">
          <MetricSmall label="Brier" value={brierScore.toFixed(3)} />
          <MetricSmall label="ROC-AUC" value={useTerminalStore.getState().rocAuc.toFixed(3) || '—'} />
        </div>
        <div className="space-y-1">
          {calibrationCurves.slice(0, 5).map((bin) => (
            <div key={bin.bucket} className="flex items-center gap-1 text-2xs font-mono">
              <span className="text-terminal-muted w-12">{bin.bucket}</span>
              <div className="flex-1 bg-terminal-highlight h-2 rounded-full overflow-hidden">
                <div className={clsx('h-full rounded-full', bin.deviation > 3.5 ? 'bg-terminal-red' : 'bg-terminal-green')}
                  style={{ width: `${Math.min(100, bin.realized)}%` }} />
              </div>
              <span className={clsx('w-7 text-right', bin.deviation > 3.5 ? 'text-terminal-red' : 'text-terminal-green')}>
                {bin.realized}%
              </span>
            </div>
          ))}
        </div>
      </div>

      {/* Expected Value */}
      <div className="panel-header">EXPECTED VALUE</div>
      <div className="px-2 pb-2">
        <div className="flex items-center justify-between bg-terminal-bg rounded p-2 border border-terminal-border">
          <span className="text-xs font-mono">
            EV: <span className={expectedValue >= 0.2 ? 'text-terminal-green' : 'text-terminal-yellow'}>{expectedValue.toFixed(2)}</span>
          </span>
          <span className={clsx('text-2xs font-bold px-2 py-0.5 rounded', evAction === 'EXEC_TRIGGER' ? 'bg-terminal-green/20 text-terminal-green' : 'bg-terminal-muted/20 text-terminal-muted')}>
            {evAction === 'EXEC_TRIGGER' ? '● TRIGGER' : '○ HOLD'}
          </span>
        </div>
      </div>
    </div>
  );
}

function ProbRow({ label, value, color }: { label: string; value: number; color: string }) {
  return (
    <div className="mb-1">
      <div className="flex justify-between text-xs font-mono mb-0.5">
        <span className="font-medium" style={{ color }}>{label}</span>
        <span className="text-white">{value.toFixed(1)}%</span>
      </div>
      <div className="prob-bar">
        <div className="prob-bar-fill" style={{ width: `${value}%`, backgroundColor: color }} />
      </div>
    </div>
  );
}

function SignalRow({ label, active }: { label: string; active?: boolean }) {
  return (
    <div className="data-row">
      <span className="text-2xs text-terminal-muted">{label}</span>
      <span className={clsx('text-2xs font-mono', active ? 'text-terminal-green' : 'text-terminal-muted')}>
        {active ? '●' : '○'}
      </span>
    </div>
  );
}

function MetricSmall({ label, value }: { label: string; value: string }) {
  return (
    <div className="bg-terminal-bg rounded p-1.5 border border-terminal-border">
      <div className="text-2xs text-terminal-muted">{label}</div>
      <div className="text-xs font-mono font-semibold text-white">{value}</div>
    </div>
  );
}
