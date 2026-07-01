'use client';
import { useTerminalStore } from '../../lib/store';

export function SMCSection() {
  const { smc } = useTerminalStore();
  const s = smc || { fvg: null, orderBlocks: null, liquidity: null };

  const items = [
    { label: 'FVG', active: !!(s.fvg?.length), color: 'bg-terminal-blue' },
    { label: 'OB', active: !!(s.orderBlocks?.length), color: 'bg-terminal-purple' },
    { label: 'LIQ SWEEP', active: !!(s.liquidity?.sweptAbove || s.liquidity?.sweptBelow), color: 'bg-terminal-orange' },
  ];

  return (
    <>
      <div className="panel-header">SMC</div>
      <div className="p-2 space-y-1">
        {items.map((item) => (
          <div key={item.label} className="data-row">
            <span className="data-label">{item.label}</span>
            <span className={`inline-block w-2 h-2 rounded-full ${item.active ? item.color : 'bg-terminal-muted'}`} />
          </div>
        ))}
      </div>
    </>
  );
}
