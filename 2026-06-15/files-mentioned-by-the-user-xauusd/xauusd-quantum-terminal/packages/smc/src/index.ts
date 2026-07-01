//=============================================================================
// Smart Money Concepts (SMC) Engine
// Ported and improved from XAUUSD Quantum 3.0 Pine Script
//=============================================================================
import type { Candle, SMCOutput, FVGZone, OBZone, Timeframe, LiquidityLevel } from '@xauusd/common';

export interface SMCEngineConfig {
  atrMultiplier: number;
  fvgMaxBars: number;
  obMaxBars: number;
  pivotLookback: number;
  swingLookback: number;
  liquidityBuffer: number;
}

const DEFAULT_CONFIG: SMCEngineConfig = {
  atrMultiplier: 14,
  fvgMaxBars: 30,
  obMaxBars: 20,
  pivotLookback: 5,
  swingLookback: 5,
  liquidityBuffer: 0.3,
};

export class SMCEngine {
  private config: SMCEngineConfig;
  private candles: Candle[] = [];
  private pendingFvg: FVGZone[] = [];
  private pendingOBs: OBZone[] = [];
  private knownLiquidity: LiquidityLevel[] = [];
  private atr: number = 0;

  constructor(config?: Partial<SMCEngineConfig>) {
    this.config = { ...DEFAULT_CONFIG, ...config };
  }

  ingestCandles(candles: Candle[]): void {
    this.candles = candles;
    this.computeATR();
  }

  private computeATR(): void {
    if (this.candles.length < 15) return;
    let sum = 0;
    for (let i = this.candles.length - 14; i < this.candles.length; i++) {
      const c = this.candles[i];
      const prev = this.candles[i - 1];
      const tr = Math.max(
        c.high - c.low,
        Math.abs(c.high - prev.close),
        Math.abs(c.low - prev.close)
      );
      sum += tr;
    }
    this.atr = sum / 14;
  }

  private getATR(): number {
    return Math.max(this.atr, this.candles[this.candles.length - 1]?.close * 0.0001 || 0.0001);
  }

  //-----------------------------------------------------------------------------
  // FVG Detection (Fair Value Gaps)
  //-----------------------------------------------------------------------------
  private detectFVG(index: number): { bull: FVGZone | null; bear: FVGZone | null } {
    const candles = this.candles;
    if (index < 3) return { bull: null, bear: null };
    const buffer = this.getATR() * 0.15;

    // Bullish FVG: Low[i+2] > High[i]
    for (let i = 2; i <= Math.min(10, index - 2); i++) {
      const gapLow = candles[index - i - 2]?.low ?? 0;
      const gapHigh = candles[index - i]?.high ?? 0;
      if (gapHigh < gapLow && gapLow - gapHigh > buffer) {
        const filled = candles[index].high >= gapHigh && candles[index].low <= gapLow;
        return {
          bull: { upper: gapHigh, lower: gapLow, size: gapLow - gapHigh, createdAt: index, filled },
          bear: null,
        };
      }
    }

    // Bearish FVG: Low[i] > High[i+2]
    for (let i = 2; i <= Math.min(10, index - 2); i++) {
      const gapLow = candles[index - i]?.low ?? 0;
      const gapHigh = candles[index - i - 2]?.high ?? 0;
      if (gapHigh < gapLow && gapLow - gapHigh > buffer) {
        const filled = candles[index].high >= gapHigh && candles[index].low <= gapLow;
        return {
          bull: null,
          bear: { upper: gapHigh, lower: gapLow, size: gapLow - gapHigh, createdAt: index, filled },
        };
      }
    }

    return { bull: null, bear: null };
  }

  //-----------------------------------------------------------------------------
  // Order Block Detection
  //-----------------------------------------------------------------------------
  private detectOB(index: number): { bull: OBZone | null; bear: OBZone | null } {
    if (index < 10) return { bull: null, bear: null };
    const candles = this.candles;
    const prev = candles[index - 1];
    const curr = candles[index];
    if (!prev || !curr) return { bull: null, bear: null };

    const body = Math.abs(prev.close - prev.open);
    const range = prev.high - prev.low;
    const bodyPct = range > 0 ? (body / range) * 100 : 0;

    // Bullish OB: bearish candle engulfed
    if (prev.close < prev.open && bodyPct > 40) {
      const engulfed = curr.open <= prev.high && curr.close >= prev.low;
      return {
        bull: engulfed
          ? { high: prev.high, low: prev.low, type: 'bullish', createdAt: index, mitigated: false }
          : null,
        bear: null,
      };
    }

    // Bearish OB: bullish candle engulfed
    if (prev.close > prev.open && bodyPct > 40) {
      const engulfed = curr.open >= prev.low && curr.close <= prev.high;
      return {
        bull: null,
        bear: engulfed
          ? { high: prev.high, low: prev.low, type: 'bearish', createdAt: index, mitigated: false }
          : null,
      };
    }

    return { bull: null, bear: null };
  }

  //-----------------------------------------------------------------------------
  // Liquidity Detection
  //-----------------------------------------------------------------------------
  private detectLiquidity(index: number) {
    const candles = this.candles;
    if (index < 50) return { above: null as number | null, below: null as number | null };
    const buffer = this.getATR() * 0.50;

    let above: number | null = null;
    let below: number | null = null;

    // Sweep-able highs (liquidity above)
    for (let i = 1; i <= 50 && index > i + 1; i++) {
      const c = candles[index - i];
      const cNext = candles[index - i + 1];
      const cPrev = candles[index - i - 1];
      if (c && cNext && cPrev && c.high > cNext.high && c.high > cPrev.high) {
        above = c.high;
        break;
      }
    }

    // Sweep-able lows (liquidity below)
    for (let i = 1; i <= 50 && index > i + 1; i++) {
      const c = candles[index - i];
      const cNext = candles[index - i + 1];
      const cPrev = candles[index - i - 1];
      if (c && cNext && cPrev && c.low < cNext.low && c.low < cPrev.low) {
        below = c.low;
        break;
      }
    }

    const curr = candles[index];
    return {
      above,
      below,
      sweptAbove: above !== null && curr.high >= above,
      sweptBelow: below !== null && curr.low <= below,
    };
  }

  //-----------------------------------------------------------------------------
  // Premium / Discount Arrays
  //-----------------------------------------------------------------------------
  private computePremiumDiscount(index: number): { premium: number[]; discount: number[] } {
    if (index < 20) return { premium: [], discount: [] };
    const lookback = 20;
    let high = -Infinity;
    let low = Infinity;
    for (let i = index - lookback; i <= index; i++) {
      const c = this.candles[i];
      if (!c) continue;
      if (c.high > high) high = c.high;
      if (c.low < low) low = c.low;
    }
    const midpoint = (high + low) / 2;
    const premium: number[] = [];
    const discount: number[] = [];
    for (let p = midpoint; p <= high; p += this.getATR() * 0.5) premium.push(p);
    for (let p = midpoint; p >= low; p -= this.getATR() * 0.5) discount.push(p);
    return { premium, discount };
  }

  //-----------------------------------------------------------------------------
  // BOS / CHOCH / MSS Detection (Market Structure)
  //-----------------------------------------------------------------------------
  private detectStructure(index: number) {
    const candles = this.candles;
    if (index < 10) {
      return {
        bos: { bull: false, bear: false, score: 0 },
        choch: { bull: false, bear: false, score: 0 },
        mss: { bull: false, bear: false, active: false },
        pivots: { highs: [] as number[], lows: [] as number[] },
        swingPivots: { highs: [] as number[], lows: [] as number[] },
      };
    }

    const pivots = this.findPivots(index, this.config.pivotLookback);
    const swingPivots = this.findPivots(index, this.config.swingLookback);

    const res = pivots.highs.length > 0 ? pivots.highs[pivots.highs.length - 1] : null;
    const sup = pivots.lows.length > 0 ? pivots.lows[pivots.lows.length - 1] : null;
    const curr = candles[index];

    const bullBOS = res !== null && curr.close > res;
    const bearBOS = sup !== null && curr.close < sup;
    const chochBull = bullBOS && curr.high > candles[index - 1].high;
    const chochBear = bearBOS && curr.low < candles[index - 1].low;
    const mss = (chochBull || chochBear) && bullBOS !== bearBOS;

    return {
      bos: { bull: bullBOS, bear: bearBOS, score: bullBOS ? 100 : bearBOS ? -100 : 0 },
      choch: { bull: chochBull, bear: chochBear, score: chochBull ? 100 : chochBear ? -100 : 0 },
      mss: { bull: bullBOS && chochBull, bear: bearBOS && chochBear, active: mss },
      pivots,
      swingPivots,
    };
  }

  private findPivots(index: number, lookback: number): { highs: number[]; lows: number[] } {
    const highs: number[] = [];
    const lows: number[] = [];
    const candles = this.candles;

    for (let i = lookback; i <= index - lookback; i += lookback) {
      const c = candles[i];
      if (!c) continue;
      let isHigh = true;
      let isLow = true;
      for (let j = i - lookback; j <= i + lookback; j++) {
        const cj = candles[j];
        if (!cj) continue;
        if (j !== i && cj.high >= c.high) isHigh = false;
        if (j !== i && cj.low <= c.low) isLow = false;
      }
      if (isHigh) highs.push(c.high);
      if (isLow) lows.push(c.low);
    }

    return { highs, lows };
  }

  //-----------------------------------------------------------------------------
  // Main Entry Point
  //-----------------------------------------------------------------------------
  analyze(index?: number): SMCOutput {
    const idx = index ?? this.candles.length - 1;
    const fvg = this.detectFVG(idx);
    const obs = this.detectOB(idx);
    const liquidity = this.detectLiquidity(idx);
    const premiumDiscount = this.computePremiumDiscount(idx);
    const structure = this.detectStructure(idx);

    return {
      fvg: { ...fvg, filled: fvg.bull?.filled || fvg.bear?.filled || false },
      orderBlocks: { bull: obs.bull, bear: obs.bear },
      liquidity: {
        above: liquidity.above,
        below: liquidity.below,
        sweptAbove: liquidity.sweptAbove,
        sweptBelow: liquidity.sweptBelow,
      },
      premiumDiscount,
      ...structure,
      timestamp: this.candles[idx]?.timestamp ?? Date.now(),
    };
  }

  getState() {
    return { atr: this.atr, pendingFvg: this.pendingFvg, pendingOBs: this.pendingOBs };
  }
}
