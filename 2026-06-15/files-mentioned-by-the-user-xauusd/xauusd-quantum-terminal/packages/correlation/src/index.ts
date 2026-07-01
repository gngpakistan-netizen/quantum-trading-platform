//=============================================================================
// Multi-Asset Correlation Engine
// Rolling correlations, dynamic beta, lead-lag detection, cointegration
//=============================================================================
import type { CorrelationResult, Regime } from '@xauusd/common';

export interface CorrelationConfig {
  shortWindow: number;
  medWindow: number;
  longWindow: number;
  cointegrationThreshold: number;
  regimeLookback: number;
}

const DEFAULT_CONFIG: CorrelationConfig = {
  shortWindow: 30,
  medWindow: 60,
  longWindow: 120,
  cointegrationThreshold: 0.05,
  regimeLookback: 20,
};

export class CorrelationEngine {
  private config: CorrelationConfig;
  private priceHistory: Map<string, number[]> = new Map();

  constructor(config?: Partial<CorrelationConfig>) {
    this.config = { ...DEFAULT_CONFIG, ...config };
  }

  ingest(symbol: string, price: number): void {
    if (!this.priceHistory.has(symbol)) {
      this.priceHistory.set(symbol, []);
    }
    const series = this.priceHistory.get(symbol)!;
    series.push(price);
    // Keep max longWindow * 2 for computations
    if (series.length > this.config.longWindow * 2) {
      series.splice(0, series.length - this.config.longWindow * 2);
    }
  }

  private returns(prices: number[]): number[] {
    const r: number[] = [];
    for (let i = 1; i < prices.length; i++) {
      r.push(prices[i] / prices[i - 1] - 1);
    }
    return r;
  }

  private pearsonCorrelation(x: number[], y: number[], window: number): number {
    const len = Math.min(x.length, y.length, window);
    if (len < 10) return 0;
    const xSlice = x.slice(-len);
    const ySlice = y.slice(-len);
    const mx = xSlice.reduce((s, v) => s + v, 0) / len;
    const my = ySlice.reduce((s, v) => s + v, 0) / len;
    let num = 0, dx2 = 0, dy2 = 0;
    for (let i = 0; i < len; i++) {
      const dx = xSlice[i] - mx;
      const dy = ySlice[i] - my;
      num += dx * dy;
      dx2 += dx * dx;
      dy2 += dy * dy;
    }
    const denom = Math.sqrt(dx2 * dy2);
    return denom > 0 ? num / denom : 0;
  }

  private dynamicBeta(x: number[], y: number[], window: number): number {
    const len = Math.min(x.length, y.length, window);
    if (len < 10) return 0;
    const xSlice = x.slice(-len);
    const ySlice = y.slice(-len);
    const mx = xSlice.reduce((s, v) => s + v, 0) / len;
    const my = ySlice.reduce((s, v) => s + v, 0) / len;
    let num = 0, denom = 0;
    for (let i = 0; i < len; i++) {
      num += (xSlice[i] - mx) * (ySlice[i] - my);
      denom += (xSlice[i] - mx) ** 2;
    }
    return denom > 0 ? num / denom : 0;
  }

  private adfTest(prices: number[], window: number): boolean {
    // Simplified ADF test — checks if spread is mean-reverting
    const len = Math.min(prices.length, window);
    if (len < 30) return false;
    const spread = prices.slice(-len);
    const diffs: number[] = [];
    for (let i = 1; i < spread.length; i++) diffs.push(spread[i] - spread[i - 1]);
    const spreadLag = spread.slice(0, -1);
    const n = diffs.length;
    const meanLag = spreadLag.reduce((s, v) => s + v, 0) / n;
    let num = 0, denom = 0;
    for (let i = 0; i < n; i++) {
      num += (spreadLag[i] - meanLag) * diffs[i];
      denom += (spreadLag[i] - meanLag) ** 2;
    }
    const beta = denom > 0 ? num / denom : 0;
    const residuals = diffs.map((d, i) => d - beta * (spreadLag[i] - meanLag));
    const se = Math.sqrt(residuals.reduce((s, r) => s + r * r, 0) / (n - 2));
    const tStat = se > 0 ? beta / (se / Math.sqrt(denom)) : 0;
    // Critical value ~ -2.89 for 5% significance
    return tStat < -2.89;
  }

  private leadLag(x: number[], y: number[], maxLag: number = 10): number {
    // Returns positive if x leads y, negative if y leads x
    let bestLag = 0;
    let bestCorr = -Infinity;
    for (let lag = -maxLag; lag <= maxLag; lag++) {
      const corr = lag >= 0
        ? this.pearsonCorrelation(x.slice(0, -lag || undefined), y.slice(lag), 30)
        : this.pearsonCorrelation(x.slice(-lag), y.slice(0, lag), 30);
      if (corr > bestCorr) {
        bestCorr = corr;
        bestLag = lag;
      }
    }
    return bestLag;
  }

  private detectRegime(x: number[]): Regime {
    const len = this.config.regimeLookback;
    if (x.length < len) return 'ranging';
    const recent = x.slice(-len);
    const vol = this.std(recent);
    const mean = recent.reduce((s, v) => s + v, 0) / len;
    const adx = Math.abs(mean) / Math.max(vol, 0.0001);
    if (adx > 1.5) return 'trending';
    if (adx > 0.5) return 'ranging';
    return 'dead';
  }

  private std(arr: number[]): number {
    const mean = arr.reduce((s, v) => s + v, 0) / arr.length;
    return Math.sqrt(arr.reduce((s, v) => s + (v - mean) ** 2, 0) / arr.length);
  }

  analyze(symbol1: string, symbol2: string): CorrelationResult {
    const s1 = this.priceHistory.get(symbol1);
    const s2 = this.priceHistory.get(symbol2);
    if (!s1 || !s2 || s1.length < 30 || s2.length < 30) {
      return {
        pair: `${symbol1}/${symbol2}`,
        correlation30: 0, correlation60: 0, correlation120: 0,
        beta: 0, leadLag: 0, cointegrated: false,
        regime: 'ranging',
        timestamp: Date.now(),
      };
    }
    const r1 = this.returns(s1);
    const r2 = this.returns(s2);

    return {
      pair: `${symbol1}/${symbol2}`,
      correlation30: this.pearsonCorrelation(r1, r2, this.config.shortWindow),
      correlation60: this.pearsonCorrelation(r1, r2, this.config.medWindow),
      correlation120: this.pearsonCorrelation(r1, r2, this.config.longWindow),
      beta: this.dynamicBeta(r1, r2, this.config.medWindow),
      leadLag: this.leadLag(r1, r2),
      cointegrated: this.adfTest(s1.slice(-s2.length), this.config.longWindow),
      regime: this.detectRegime(r1),
      timestamp: Date.now(),
    };
  }

  analyzeAllPairs(symbols: string[], baseSymbol: string = 'XAUUSD'): CorrelationResult[] {
    return symbols
      .filter(s => s !== baseSymbol)
      .map(s => this.analyze(baseSymbol, s));
  }
}
