import { create } from 'zustand';
import type { Bias, Timeframe, CorrelationResult, SMCOutput, StructureOutput, ProbabilityOutput, ForecastOutput } from '@xauusd/common';

interface CalibrationBin {
  bucket: string; count: number; predicted: number; realized: number; deviation: number; status: string;
}

interface WebhookAlert {
  id: string; event: string; price: number; symbol: string; timestamp: string;
}

interface EconomicEvent {
  id: string; title: string; currency: string; time: string; impact: string; forecast: string | null; previous: string; goldSensitivityWeight: number; riskScore: number;
}

interface TerminalState {
  initialized: boolean; wsConnected: boolean; apiUrl: string; wsUrl: string;
  price: number; change: number; changePercent: number; high: number; low: number; volume: number; timeframe: Timeframe; symbol: string;
  bias: { macro: Bias; micro: Bias; current: Bias; longTerm: Bias; conviction: number };
  probability: ProbabilityOutput | null; forecast: ForecastOutput | null; correlations: CorrelationResult[]; structure: StructureOutput | null; smc: SMCOutput | null;
  performance: { successRate: number; sharpe: number; maxDrawdown: number; profitFactor: number };

  // AI Studio features
  calibrationCurves: CalibrationBin[];
  brierScore: number;
  calibrationBrier: number;
  refinementBrier: number;
  rocAuc: number;
  modelCoefficients: { w0: number; w1: number; w2: number; w3: number; w4: number } | null;
  lossHistory: { epoch: number; loss: number }[];
  expectedValue: number;
  evAction: string;
  recentAlerts: WebhookAlert[];
  economicEvents: EconomicEvent[];

  // UI State
  selectedTab: string; selectedTraceId: string | null; showExplainability: boolean;

  // Actions
  initialize: () => Promise<void>;
  setTimeframe: (tf: Timeframe) => void;
  setSelectedTab: (tab: string) => void;
  fetchAnalysis: () => Promise<void>;
  fetchAnalytics: () => Promise<void>;
  connectWS: () => void;
}

const API = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8787';

export const useTerminalStore = create<TerminalState>((set, get) => ({
  initialized: false, wsConnected: false,
  apiUrl: API, wsUrl: process.env.NEXT_PUBLIC_WS_URL || `wss://${new URL(API).host}/ws`,
  price: 0, change: 0, changePercent: 0, high: 0, low: 0, volume: 0, timeframe: '5m', symbol: 'XAUUSD',
  bias: { macro: 'neutral', micro: 'neutral', current: 'neutral', longTerm: 'neutral', conviction: 0 },
  probability: null, forecast: null, correlations: [], structure: null, smc: null,
  performance: { successRate: 0, sharpe: 0, maxDrawdown: 0, profitFactor: 0 },

  calibrationCurves: [], brierScore: 0, calibrationBrier: 0, refinementBrier: 0, rocAuc: 0,
  modelCoefficients: null, lossHistory: [],
  expectedValue: 0, evAction: 'HOLD',
  recentAlerts: [], economicEvents: [],

  selectedTab: 'performance', selectedTraceId: null, showExplainability: false,

  initialize: async () => {
    set({ initialized: true });
    await Promise.all([get().fetchAnalysis(), get().fetchAnalytics()]);
    get().connectWS();
  },

  setTimeframe: (tf) => { set({ timeframe: tf }); get().fetchAnalysis(); },
  setSelectedTab: (tab) => set({ selectedTab: tab }),

  fetchAnalysis: async () => {
    const { apiUrl, timeframe } = get();
    try {
      const [biasRes, probRes, corrRes, structRes, smcRes, perfRes, econRes] = await Promise.all([
        fetch(`${apiUrl}/api/v1/analysis/bias?timeframe=${timeframe}`),
        fetch(`${apiUrl}/api/v1/analysis/probability?timeframe=${timeframe}`),
        fetch(`${apiUrl}/api/v1/analysis/correlation`),
        fetch(`${apiUrl}/api/v1/analysis/structure?timeframe=${timeframe}`),
        fetch(`${apiUrl}/api/v1/analysis/smc?timeframe=${timeframe}`),
        fetch(`${apiUrl}/api/v1/performance/metrics`),
        fetch(`${apiUrl}/api/v1/market/economic-calendar`),
      ]);
      if (biasRes.ok) set({ bias: await biasRes.json() });
      if (probRes.ok) set({ probability: await probRes.json() });
      if (corrRes.ok) set({ correlations: await corrRes.json() });
      if (structRes.ok) set({ structure: await structRes.json() });
      if (smcRes.ok) set({ smc: await smcRes.json() });
      if (perfRes.ok) set({ performance: await perfRes.json() });
      if (econRes.ok) {
        const econ = await econRes.json();
        set({ economicEvents: econ.upcomingEvents || [] });
      }
    } catch (err) { console.error('fetchAnalysis error:', err); }
  },

  fetchAnalytics: async () => {
    const { apiUrl } = get();
    try {
      const [calRes, evRes, alertRes] = await Promise.all([
        fetch(`${apiUrl}/api/v1/analytics/calibration`),
        fetch(`${apiUrl}/api/v1/analytics/ev?winProb=65&rewardRatio=3`),
        fetch(`${apiUrl}/api/v1/alerts`).catch(() => null),
      ]);
      if (calRes.ok) {
        const cal = await calRes.json();
        set({
          calibrationCurves: cal.calibrationCurves || [],
          brierScore: parseFloat(cal.brierScore) || 0,
          calibrationBrier: parseFloat(cal.calibrationBrier) || 0,
          refinementBrier: parseFloat(cal.refinementBrier) || 0,
        });
      }
      if (evRes.ok) {
        const ev = await evRes.json();
        if (ev.success) { set({ expectedValue: ev.expectedValue, evAction: ev.executionAction }); }
      }
      if (alertRes && alertRes.ok) set({ recentAlerts: await alertRes.json() });
    } catch (err) { console.error('fetchAnalytics error:', err); }
  },

  connectWS: () => {
    const { wsUrl } = get();
    try {
      const ws = new WebSocket(wsUrl);
      ws.onopen = () => set({ wsConnected: true });
      ws.onmessage = (event) => {
        try {
          const msg = JSON.parse(event.data);
          switch (msg.type) {
            case 'TICK': set({ price: msg.price }); break;
            case 'candle_update': set({ price: msg.data.close, high: msg.data.high, low: msg.data.low }); break;
            case 'bias_update': set({ bias: msg.data }); break;
            case 'probability_update': set({ probability: msg.data }); break;
          }
        } catch {}
      };
      ws.onclose = () => set({ wsConnected: false });
      ws.onerror = () => set({ wsConnected: false });
    } catch {}
  },
}));
