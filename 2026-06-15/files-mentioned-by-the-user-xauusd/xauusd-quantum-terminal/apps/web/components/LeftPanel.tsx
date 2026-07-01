'use client';
import { useTerminalStore } from '../lib/store';
import { BiasSection } from './sections/BiasSection';
import { StructureSection } from './sections/StructureSection';
import { SMCSection } from './sections/SMCSection';
import { SessionSection } from './sections/SessionSection';

export function LeftPanel() {
  const { bias, structure } = useTerminalStore();

  return (
    <div className="panel" style={{ gridRow: '2 / 4' }}>
      <BiasSection />
      <div className="panel-header">MARKET STRUCTURE</div>
      <div className="panel-header" style={{ marginTop: 0 }}>SMC</div>
      <div className="panel-header" style={{ marginTop: 0 }}>SESSION</div>
    </div>
  );
}
