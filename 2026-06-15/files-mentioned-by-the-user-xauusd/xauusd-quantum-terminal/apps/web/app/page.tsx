'use client';

import { useEffect, useState } from 'react';
import { TopBar } from '../components/TopBar';
import { LeftPanel } from '../components/LeftPanel';
import { ChartPanel } from '../components/ChartPanel';
import { RightPanel } from '../components/RightPanel';
import { BottomPanel } from '../components/BottomPanel';
import { useTerminalStore } from '../lib/store';

export default function Dashboard() {
  const { initialized, initialize, price, wsConnected } = useTerminalStore();

  useEffect(() => {
    if (!initialized) {
      initialize();
    }
  }, [initialized, initialize]);

  return (
    <div className="dashboard-grid">
      <TopBar />
      <LeftPanel />
      <ChartPanel />
      <RightPanel />
      <BottomPanel />
    </div>
  );
}
