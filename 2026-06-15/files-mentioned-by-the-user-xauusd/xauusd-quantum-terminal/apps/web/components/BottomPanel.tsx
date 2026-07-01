'use client';
import { useTerminalStore } from '../lib/store';
import { clsx } from 'clsx';

export function BottomPanel() {
  const { selectedTab, setSelectedTab, performance, correlations, forecast, economicEvents, recentAlerts } = useTerminalStore();

  const tabs = [
    { id: 'performance', label: 'PERFORMANCE' },
    { id: 'correlation', label: 'CORRELATION' },
    { id: 'economic', label: 'ECONOMIC' },
    { id: 'alerts', label: 'ALERTS' },
    { id: 'forecast', label: 'FORECAST' },
  ];

  return (
    <div className="panel" style={{ gridColumn: '2 / -1', gridRow: '3' }}>
      <div className="flex border-b border-terminal-border overflow-x-auto">
        {tabs.map(tab => (
          <button key={tab.id}
            onClick={() => setSelectedTab(tab.id)}
            className={clsx(
              'px-3 py-1.5 text-2xs font-medium uppercase tracking-wider transition-colors whitespace-nowrap',
              selectedTab === tab.id
                ? 'text-white border-b-2 border-terminal-blue bg-terminal-highlight/30'
                : 'text-terminal-muted hover:text-white'
            )}>
            {tab.label}
          </button>
        ))}
      </div>

      <div className="p-3 overflow-y-auto h-[calc(100%-32px)]">
        {selectedTab === 'performance' && (
          <div className="grid grid-cols-5 gap-3">
            <MetricCard label="Success Rate" value={`${performance.successRate.toFixed(1)}%`} />
            <MetricCard label="Sharpe" value={performance.sharpe.toFixed(2)} />
            <MetricCard label="Max DD" value={`${performance.maxDrawdown.toFixed(1)}%`} color="text-terminal-red" />
            <MetricCard label="Profit Factor" value={performance.profitFactor.toFixed(2)} />
            <MetricCard label="Brier Score" value={useTerminalStore.getState().brierScore.toFixed(3)} />
          </div>
        )}
        {selectedTab === 'correlation' && <CorrelationView />}
        {selectedTab === 'economic' && <EconomicCalendarView />}
        {selectedTab === 'alerts' && <AlertsView />}
        {selectedTab === 'forecast' && <ForecastView />}
      </div>
    </div>
  );
}

function MetricCard({ label, value, color = 'text-white' }: { label: string; value: string; color?: string }) {
  return (
    <div className="bg-terminal-bg rounded p-2 border border-terminal-border">
      <div className="text-2xs text-terminal-muted mb-1">{label}</div>
      <div className={`text-sm font-mono font-semibold ${color}`}>{value}</div>
    </div>
  );
}

function CorrelationView() {
  const { correlations } = useTerminalStore();
  return (
    <div className="grid grid-cols-3 gap-2">
      {correlations.map(c => (
        <div key={c.pair} className="bg-terminal-bg rounded p-2 border border-terminal-border">
          <div className="text-2xs text-terminal-muted mb-1">{c.pair}</div>
          <div className="flex gap-3 text-xs font-mono">
            <span className={c.correlation30 > 0.5 ? 'text-terminal-green' : c.correlation30 < -0.3 ? 'text-terminal-red' : ''}>
              ρ30: {c.correlation30?.toFixed(2) ?? '—'}
            </span>
            <span>β: {c.beta?.toFixed(2) ?? '—'}</span>
          </div>
        </div>
      ))}
      {correlations.length === 0 && <div className="text-terminal-muted text-xs col-span-3">No correlation data</div>}
    </div>
  );
}

function EconomicCalendarView() {
  const { economicEvents } = useTerminalStore();
  return (
    <div className="space-y-2">
      {economicEvents.length === 0 && <div className="text-terminal-muted text-xs">No upcoming events</div>}
      {economicEvents.map(evt => {
        const impactColor = evt.impact === 'HIGH' ? 'text-terminal-red' : 'text-terminal-yellow';
        const time = new Date(evt.time);
        const timeStr = `${time.getHours().toString().padStart(2, '0')}:${time.getMinutes().toString().padStart(2, '0')}`;
        return (
          <div key={evt.id} className="flex items-center justify-between bg-terminal-bg rounded p-2 border border-terminal-border text-2xs font-mono">
            <div className="flex items-center gap-2">
              <span className={clsx('font-bold', impactColor)}>{evt.impact}</span>
              <span className="text-white">{evt.title}</span>
              <span className="text-terminal-muted">{evt.currency}</span>
            </div>
            <div className="flex items-center gap-3">
              <span className="text-terminal-cyan">{timeStr}</span>
              <span className="text-terminal-muted">F: {evt.forecast || '—'}</span>
              <span className="text-terminal-muted">P: {evt.previous}</span>
              <span className={clsx(evt.goldSensitivityWeight < -0.7 ? 'text-terminal-red' : 'text-terminal-muted')}>
                γ: {evt.goldSensitivityWeight.toFixed(2)}
              </span>
              <span className={clsx('px-1.5 py-0.5 rounded', evt.riskScore >= 8 ? 'bg-terminal-red/20 text-terminal-red' : 'bg-terminal-yellow/20 text-terminal-yellow')}>
                {evt.riskScore.toFixed(1)}
              </span>
            </div>
          </div>
        );
      })}
    </div>
  );
}

function AlertsView() {
  const { recentAlerts } = useTerminalStore();
  return (
    <div className="space-y-1">
      <div className="flex items-center gap-2 mb-2 text-2xs text-terminal-muted">
        <span>Webhook: <span className="text-terminal-green">LIVE</span></span>
        <span>| {recentAlerts.length} recent</span>
      </div>
      {recentAlerts.length === 0 && <div className="text-terminal-muted text-xs">No alerts received. Configure TradingView webhook.</div>}
      {recentAlerts.map(alert => (
        <div key={alert.id} className="flex items-center justify-between bg-terminal-bg rounded px-2 py-1.5 border border-terminal-border text-2xs font-mono">
          <div className="flex items-center gap-2">
            <span className={clsx(
              alert.event === 'MSS' ? 'text-terminal-red' : alert.event === 'Liquidity Sweep' ? 'text-terminal-yellow' : alert.event === 'CHOCH' ? 'text-terminal-blue' : 'text-terminal-green'
            )}>{alert.event}</span>
            <span className="text-white">{alert.symbol}</span>
            <span className="text-terminal-cyan">${alert.price.toFixed(2)}</span>
          </div>
          <span className="text-terminal-muted">{new Date(alert.timestamp).toLocaleTimeString()}</span>
        </div>
      ))}
    </div>
  );
}

function ForecastView() {
  const { forecast } = useTerminalStore();
  if (!forecast) return <div className="text-terminal-muted text-xs">No forecast available</div>;
  return (
    <div className="grid grid-cols-4 gap-3">
      <MetricCard label="Expected Move" value={`${forecast.expectedMove?.toFixed(1) ?? '—'}`} />
      <MetricCard label="CI Lower" value={`${forecast.confidenceInterval?.[0]?.toFixed(1) ?? '—'}`} />
      <MetricCard label="CI Upper" value={`${forecast.confidenceInterval?.[1]?.toFixed(1) ?? '—'}`} />
      <MetricCard label="Confidence" value={`${((forecast.modelConfidence ?? 0) * 100).toFixed(0)}%`} />
    </div>
  );
}
