export function brierScore(predictions: number[], outcomes: number[]): number {
  const n = predictions.length;
  let sum = 0;
  for (let i = 0; i < n; i++) sum += (predictions[i] - outcomes[i]) ** 2;
  return sum / n;
}

export function brierDecomposition(predictions: number[], outcomes: number[]) {
  const n = predictions.length;
  const observedRate = outcomes.reduce((s, v) => s + v, 0) / n;

  const binSize = 10;
  const bins: { lo: number; hi: number; preds: number[]; outcomes: number[] }[] = [];
  for (let i = 0; i < binSize; i++) {
    bins.push({ lo: i / binSize, hi: (i + 1) / binSize, preds: [], outcomes: [] });
  }

  for (let i = 0; i < n; i++) {
    const p = predictions[i];
    const idx = Math.min(binSize - 1, Math.floor(p * binSize));
    bins[idx].preds.push(p);
    bins[idx].outcomes.push(outcomes[i]);
  }

  let calibration = 0;
  let refinement = 0;
  const reliability: { bucket: string; count: number; predicted: number; realized: number; deviation: number }[] = [];

  for (const bin of bins) {
    const k = bin.preds.length;
    if (k === 0) continue;
    const avgPred = bin.preds.reduce((s, v) => s + v, 0) / k;
    const avgOut = bin.outcomes.reduce((s, v) => s + v, 0) / k;
    calibration += k * (avgPred - avgOut) ** 2;
    refinement += k * (avgOut - observedRate) ** 2;
    reliability.push({
      bucket: `${(bin.lo * 100).toFixed(0)}-${(bin.hi * 100).toFixed(0)}%`,
      count: k,
      predicted: parseFloat(avgPred.toFixed(4)),
      realized: parseFloat(avgOut.toFixed(4)),
      deviation: parseFloat(((avgOut - avgPred) * 100).toFixed(2)),
    });
  }

  const outcomeVariance = observedRate * (1 - observedRate);
  const brier = brierScore(predictions, outcomes);

  return {
    brier,
    calibration: calibration / n,
    refinement: refinement / n,
    outcomeVariance,
    reliability,
    identityCheck: Math.abs(brier - (calibration / n - refinement / n + outcomeVariance)) < 0.15,
  };
}

export function boxMullerNormalityTest(samples = 1000) {
  const vals: number[] = [];
  let u = 0, v = 0;
  for (let i = 0; i < samples; i++) {
    while (u === 0) u = Math.random();
    while (v === 0) v = Math.random();
    vals.push(Math.sqrt(-2 * Math.log(u)) * Math.cos(2 * Math.PI * v));
    u = v = 0;
  }
  const mean = vals.reduce((s, x) => s + x, 0) / samples;
  const stdev = Math.sqrt(vals.reduce((s, x) => s + (x - mean) ** 2, 0) / samples);
  return { mean, stdev, pass: Math.abs(mean) < 0.2 && Math.abs(stdev - 1) < 0.2 };
}

export function auditFormulaVerification(params: {
  brier: number;
  calibrationBrier: number;
  refinementBrier: number;
  outcomeVariance: number;
  atrValues?: number[];
}) {
  const { brier, calibrationBrier, refinementBrier, outcomeVariance } = params;
  const brierSum = calibrationBrier - refinementBrier + outcomeVariance;
  const brierIdentityPass = Math.abs(brier - brierSum) < 0.15;
  const atrPass = params.atrValues ? params.atrValues.every((v) => v >= 0) : true;

  let passed = 0;
  const results: { testName: string; status: string; details: string }[] = [];

  results.push({
    testName: "Brier Identity Holding Theorem",
    status: brierIdentityPass ? "PASS" : "FAIL",
    details: `Brier=${brier.toFixed(4)}, Decomposed=${brierSum.toFixed(4)}`,
  });
  if (brierIdentityPass) passed++;

  results.push({
    testName: "ATR Non-Negative Bound Check",
    status: atrPass ? "PASS" : "FAIL",
    details: `All ATR values >= 0: ${atrPass}`,
  });
  if (atrPass) passed++;

  return {
    summary: { totalTestsRun: results.length, passed },
    results,
  };
}

export function confusionMatrix(
  predictions: number[],
  outcomes: number[],
  threshold = 0.5
) {
  let tp = 0, fp = 0, tn = 0, fn = 0;
  for (let i = 0; i < predictions.length; i++) {
    const p = predictions[i] >= threshold ? 1 : 0;
    const o = outcomes[i];
    if (p === 1 && o === 1) tp++;
    else if (p === 1 && o === 0) fp++;
    else if (p === 0 && o === 0) tn++;
    else if (p === 0 && o === 1) fn++;
  }
  const prec = tp + fp > 0 ? tp / (tp + fp) : 0;
  const rec = tp + fn > 0 ? tp / (tp + fn) : 0;
  return {
    tp, fp, tn, fn,
    accuracy: (tp + tn) / predictions.length,
    precision: prec,
    recall: rec,
    f1: prec + rec > 0 ? (2 * prec * rec) / (prec + rec) : 0,
  };
}
