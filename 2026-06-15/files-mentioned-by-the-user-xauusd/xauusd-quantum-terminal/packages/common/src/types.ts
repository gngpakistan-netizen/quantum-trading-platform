//=============================================================================
// Core type definitions for XAUUSD Quantum Terminal
//=============================================================================

export type Timeframe = '1m' | '5m' | '15m' | '30m' | '1h' | '4h' | '1d';
export type Bias = 'bullish' | 'bearish' | 'neutral';
export type Regime = 'trending' | 'ranging' | 'volatile' | 'dead';
export type Session = 'asia' | 'london' | 'ny' | 'off';
export type SignalGrade = 'A' | 'B' | 'C' | 'D' | 'F';

export interface Candle {
  symbol: string;
  timeframe: Timeframe;
  timestamp: number;
  open: number;
  high: number;
  low: number;
  close: number;
  volume: number;
}

export interface Tick {
  symbol: string;
  timestamp: number;
  price: number;
  volume?: number;
  bid?: number;
  ask?: number;
}

export interface CorrelationResult {
  pair: string;
  correlation30: number;
  correlation60: number;
  correlation120: number;
  beta: number;
  leadLag: number;
  cointegrated: boolean;
  regime: Regime;
  timestamp: number;
}

export interface MacroBias {
  macro: Bias;
  micro: Bias;
  current: Bias;
  longTerm: Bias;
  macroScore: number;
  microScore: number;
  currentScore: number;
  longTermScore: number;
  conviction: number;
  drivers: MacroDriver[];
}

export interface MacroDriver {
  name: string;
  impact: number;
  direction: Bias;
  description: string;
}

export interface StructureOutput {
  bos: { bull: boolean; bear: boolean; score: number };
  choch: { bull: boolean; bear: boolean; score: number };
  mss: { bull: boolean; bear: boolean; active: boolean };
  pivots: { highs: number[]; lows: number[] };
  swingPivots: { highs: number[]; lows: number[] };
  timestamp: number;
}

export interface SMCOutput {
  fvg: { bull: FVGZone | null; bear: FVGZone | null; filled: boolean };
  orderBlocks: { bull: OBZone | null; bear: OBZone | null };
  liquidity: { above: number | null; below: number | null; sweptAbove: boolean; sweptBelow: boolean };
  premiumDiscount: { premium: number[]; discount: number[] };
  timestamp: number;
}

export interface FVGZone {
  upper: number;
  lower: number;
  size: number;
  createdAt: number;
  filled: boolean;
}

export interface OBZone {
  high: number;
  low: number;
  type: 'bullish' | 'bearish';
  createdAt: number;
  mitigated: boolean;
}

export interface LiquidityLevel {
  type: 'pdh' | 'pdl' | 'pwh' | 'pwl' | 'pmh' | 'pml' | 'eqh' | 'eql';
  price: number;
  timestamp: number;
  strength: number;
  hitCount: number;
  hitRate: number;
}

export interface ForecastOutput {
  horizon: Timeframe;
  expectedMove: number;
  confidenceInterval: [number, number];
  probabilityCone: Array<{ price: number; probability: number }>;
  scenarios: {
    bullish: number;
    bearish: number;
    neutral: number;
  };
  ensembleSize: number;
  modelConfidence: number;
  timestamp: number;
}

export interface ProbabilityOutput {
  pdhSweep: number;
  pdlSweep: number;
  bullish: number;
  bearish: number;
  continuation: number;
  reversal: number;
  meanReversion: number;
  trendExpansion: number;
  calibrated: number;
  timestamp: number;
}

export interface PerformanceMetrics {
  totalSignals: number;
  winCount: number;
  lossCount: number;
  successRate: number;
  precision: number;
  recall: number;
  f1Score: number;
  brierScore: number;
  calibrationError: number;
  sharpe: number;
  sortino: number;
  maxDrawdown: number;
  profitFactor: number;
  expectancy: number;
}

export interface CalculationTrace {
  traceId: string;
  timestamp: number;
  engine: string;
  inputs: Record<string, unknown>;
  formula: string;
  intermediates: Array<{ step: string; value: number; explanation: string }>;
  output: Record<string, unknown>;
  version: number;
  parentTraceId?: string;
}

export interface EngineSnapshot {
  id?: number;
  symbol: string;
  timeframe: Timeframe;
  timestamp: number;
  engineName: string;
  output: Record<string, unknown>;
  version: number;
}

export interface AuditReport {
  id?: number;
  timestamp: number;
  type: 'data_quality' | 'formula' | 'calibration' | 'drift' | 'security';
  status: 'pass' | 'warn' | 'fail';
  engine?: string;
  metric?: string;
  expected?: number;
  actual?: number;
  deviation?: number;
  details: Record<string, unknown>;
}

// WebSocket message types
export type WSMessageType =
  | 'candle_update'
  | 'tick'
  | 'engine_snapshot'
  | 'bias_update'
  | 'probability_update'
  | 'forecast_update'
  | 'alert'
  | 'audit_event';

export interface WSMessage {
  type: WSMessageType;
  data: unknown;
  timestamp: number;
}
