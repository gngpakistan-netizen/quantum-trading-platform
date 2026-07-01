//=============================================================================
// Bayesian Probability Engine
// Implements calibrated probability estimates via Bayesian softmax
// Replaces Pine Script's ad-hoc normalization cascade
//=============================================================================
import type { ProbabilityOutput, Bias, Regime, Candle } from '@xauusd/common';

export interface ProbabilityEngineConfig {
  softmaxTemperature: number;
  minEvidenceThreshold: number;
  calibrationDecay: number;
}

const DEFAULT_CONFIG: ProbabilityEngineConfig = {
  softmaxTemperature: 1.0,
  minEvidenceThreshold: 0.01,
  calibrationDecay: 0.98,
};

export class ProbabilityEngine {
  private config: ProbabilityEngineConfig;
  private historicalPredictions: Array<{ predicted: number; actual: number }> = [];
  private calibrationFactor = 1.0;

  constructor(config?: Partial<ProbabilityEngineConfig>) {
    this.config = { ...DEFAULT_CONFIG, ...config };
  }

  //-----------------------------------------------------------------------------
  // Evidence Collection
  //-----------------------------------------------------------------------------
  private getHTFEvidence(htfScores: Record<string, number>): { bull: number; bear: number } {
    let bull = 0, bear = 0;
    for (const [tf, score] of Object.entries(htfScores)) {
      const weight = tf === '1d' ? 0.25 : tf === '4h' ? 0.20 : tf === '1h' ? 0.15 : 0.10;
      if (score > 60) bull += weight;
      else if (score < 40) bear += weight;
    }
    return { bull, bear };
  }

  private getTrendEvidence(
    bullScore: number,
    bearScore: number,
    trendThreshold: number
  ): { bull: number; bear: number } {
    return {
      bull: (bullScore / 100) * 0.20,
      bear: (bearScore / 100) * 0.20,
    };
  }

  private getMREvidence(mrComposite: number): { bull: number; bear: number } {
    const contra = mrComposite < 0 ? -mrComposite / 100 : 0;
    const pro = mrComposite > 0 ? mrComposite / 100 : 0;
    return {
      bull: contra * 0.10 + (mrComposite < -30 ? 0.10 : 0),
      bear: pro * 0.10 + (mrComposite > 30 ? 0.10 : 0),
    };
  }

  private getStructureEvidence(
    bosBull: boolean,
    bosBear: boolean,
    chochBull: boolean,
    chochBear: boolean,
    displacementBull: boolean,
    displacementBear: boolean
  ): { bull: number; bear: number } {
    let bull = 0, bear = 0;
    if (bosBull) bull += 0.15;
    if (bosBear) bear += 0.15;
    if (chochBull) bull += 0.10;
    if (chochBear) bear += 0.10;
    if (displacementBull) bull += 0.10;
    if (displacementBear) bear += 0.10;
    return { bull, bear };
  }

  private getDivergenceEvidence(
    rsiBullDiv: boolean,
    rsiBearDiv: boolean
  ): { bull: number; bear: number } {
    return {
      bull: rsiBullDiv ? 0.12 : 0,
      bear: rsiBearDiv ? 0.12 : 0,
    };
  }

  private getFVGEvidence(fvgBull: boolean, fvgBear: boolean): { bull: number; bear: number } {
    return {
      bull: fvgBull ? 0.08 : 0,
      bear: fvgBear ? 0.08 : 0,
    };
  }

  private getOBEvidence(obBull: boolean, obBear: boolean): { bull: number; bear: number } {
    return {
      bull: obBull ? 0.08 : 0,
      bear: obBear ? 0.08 : 0,
    };
  }

  private getSessionEvidence(inKillzone: boolean): { bull: number; bear: number } {
    return {
      bull: inKillzone ? 0.10 : 0,
      bear: inKillzone ? 0.10 : 0,
    };
  }

  private getCFEvidence(cfZ: number): { bull: number; bear: number } {
    const prob = this.cumulativeNormal(cfZ);
    return { bull: prob * 0.12, bear: (1 - prob) * 0.12 };
  }

  private cumulativeNormal(x: number): number {
    const a1 = 0.254829592, a2 = -0.284496736, a3 = 1.421413741;
    const a4 = -1.453152027, a5 = 1.061405429, p = 0.3275911;
    const sign = x < 0 ? -1 : 1;
    x = Math.abs(x) / Math.SQRT2;
    const t = 1 / (1 + p * x);
    const y = 1 - (((((a5 * t + a4) * t + a3) * t + a2) * t + a1) * t * Math.exp(-x * x));
    return 0.5 * (1 + sign * y);
  }

  //-----------------------------------------------------------------------------
  // Bayesian Softmax
  //-----------------------------------------------------------------------------
  private softmax(bullEvidence: number, bearEvidence: number): { bull: number; bear: number } {
    const tau = this.config.softmaxTemperature;
    const expBull = Math.exp(bullEvidence / tau);
    const expBear = Math.exp(bearEvidence / tau);
    const total = expBull + expBear;
    if (total <= 0) return { bull: 50, bear: 50 };
    return {
      bull: (expBull / total) * 100,
      bear: (expBear / total) * 100,
    };
  }

  //-----------------------------------------------------------------------------
  // Calibration
  //-----------------------------------------------------------------------------
  updateCalibration(predictedProb: number, actualOutcome: number): void {
    this.historicalPredictions.push({ predicted: predictedProb, actual: actualOutcome });
    if (this.historicalPredictions.length > 1000) {
      this.historicalPredictions = this.historicalPredictions.slice(-1000);
    }
    this.recalibrate();
  }

  private recalibrate(): void {
    const preds = this.historicalPredictions;
    if (preds.length < 30) return;

    // Reliability diagram: bin predictions into deciles
    const bins = Array.from({ length: 10 }, () => ({ count: 0, hits: 0 }));
    for (const p of preds) {
      const binIdx = Math.min(9, Math.floor(p.predicted / 10));
      bins[binIdx].count++;
      if (p.actual > 0) bins[binIdx].hits++;
    }

    // Compute expected vs observed calibration error
    let totalError = 0;
    let totalBins = 0;
    for (let i = 0; i < 10; i++) {
      const b = bins[i];
      if (b.count < 3) continue;
      const expected = (i * 10 + 5) / 100;
      const observed = b.hits / b.count;
      totalError += Math.abs(expected - observed);
      totalBins++;
    }

    this.calibrationFactor = totalBins > 0
      ? Math.max(0.5, Math.min(2.0, 1 - totalError / totalBins))
      : 1.0;
  }

  getCalibration(): { error: number; factor: number; sampleSize: number } {
    const preds = this.historicalPredictions;
    if (preds.length < 10) return { error: 0, factor: 1.0, sampleSize: preds.length };

    const mae = preds.reduce((s, p) => s + Math.abs(p.predicted / 100 - (p.actual > 0 ? 1 : 0)), 0) / preds.length;
    return { error: mae, factor: this.calibrationFactor, sampleSize: preds.length };
  }

  //-----------------------------------------------------------------------------
  // Main Computation
  //-----------------------------------------------------------------------------
  compute(params: {
    htfScores: Record<string, number>;
    bullTrendScore: number;
    bearTrendScore: number;
    trendThreshold: number;
    mrComposite: number;
    bosBull: boolean;
    bosBear: boolean;
    chochBull: boolean;
    chochBear: boolean;
    displacementBull: boolean;
    displacementBear: boolean;
    rsiBullDiv: boolean;
    rsiBearDiv: boolean;
    fvgBull: boolean;
    fvgBear: boolean;
    obBull: boolean;
    obBear: boolean;
    inKillzone: boolean;
    cfZ: number;
    regimeTrending: boolean;
    regimeRanging: boolean;
  }): ProbabilityOutput {
    let bullEvidence = 0, bearEvidence = 0;

    // Sum evidence from all modules
    const htf = this.getHTFEvidence(params.htfScores);
    bullEvidence += htf.bull; bearEvidence += htf.bear;

    const trend = this.getTrendEvidence(params.bullTrendScore, params.bearTrendScore, params.trendThreshold);
    bullEvidence += trend.bull; bearEvidence += trend.bear;

    const mr = this.getMREvidence(params.mrComposite);
    bullEvidence += mr.bull; bearEvidence += mr.bear;

    if (params.regimeTrending) {
      bullEvidence += params.bullTrendScore >= params.trendThreshold ? 0.10 : 0;
      bearEvidence += params.bearTrendScore >= params.trendThreshold ? 0.10 : 0;
    }
    if (params.regimeRanging) {
      bullEvidence += mr.bull * 1.5;
      bearEvidence += mr.bear * 1.5;
    }

    const struct = this.getStructureEvidence(
      params.bosBull, params.bosBear,
      params.chochBull, params.chochBear,
      params.displacementBull, params.displacementBear
    );
    bullEvidence += struct.bull; bearEvidence += struct.bear;

    const div = this.getDivergenceEvidence(params.rsiBullDiv, params.rsiBearDiv);
    bullEvidence += div.bull; bearEvidence += div.bear;

    const fvg = this.getFVGEvidence(params.fvgBull, params.fvgBear);
    bullEvidence += fvg.bull; bearEvidence += fvg.bear;

    const ob = this.getOBEvidence(params.obBull, params.obBear);
    bullEvidence += ob.bull; bearEvidence += ob.bear;

    const sess = this.getSessionEvidence(params.inKillzone);
    bullEvidence += sess.bull; bearEvidence += sess.bear;

    const cf = this.getCFEvidence(params.cfZ);
    bullEvidence += cf.bull; bearEvidence += cf.bear;

    // Apply calibration
    bullEvidence *= this.calibrationFactor;
    bearEvidence *= this.calibrationFactor;

    // Softmax
    const probs = this.softmax(bullEvidence, bearEvidence);

    // Derived probabilities
    const spread = Math.abs(probs.bull - probs.bear);
    const dominant = Math.max(probs.bull, probs.bear);

    return {
      pdhSweep: probs.bull * 0.6 + spread * 0.2,
      pdlSweep: probs.bear * 0.6 + spread * 0.2,
      bullish: probs.bull,
      bearish: probs.bear,
      continuation: dominant * (params.regimeTrending ? 0.7 : 0.4),
      reversal: (100 - dominant) * (params.regimeRanging ? 0.6 : 0.3),
      meanReversion: 100 - dominant,
      trendExpansion: dominant,
      calibrated: this.calibrationFactor,
      timestamp: Date.now(),
    };
  }
}
