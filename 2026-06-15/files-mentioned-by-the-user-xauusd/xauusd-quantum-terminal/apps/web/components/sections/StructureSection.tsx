'use client';
import { useTerminalStore } from '../../lib/store';

export function StructureSection() {
  const { structure } = useTerminalStore();
  const s = structure || { bos: { bull: false, bear: false }, choch: null, mss: null };

  const items = [
    { label: 'BOS', active: s.bos?.bull || s.bos?.bear, color: 'bg-terminal-green' },
    { label: 'CHOCH', active: !!s.choch, color: 'bg-terminal-yellow' },
    { label: 'MSS', active: !!s.mss, color: 'bg-terminal-red' },
  ];

  return (
    <>
      <div className="panel-header">MARKET STRUCTURE</div>
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
