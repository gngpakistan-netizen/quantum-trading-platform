export interface TrainResult {
  weights: number[];
  bias: number;
  losses: { epoch: number; loss: number }[];
}

export function sigmoid(z: number): number {
  return 1.0 / (1.0 + Math.exp(-Math.max(-500, Math.min(500, z))));
}

export function logisticRegression(
  X: number[][],
  y: number[],
  epochs = 1200,
  lr = 0.8
): TrainResult {
  const N = X.length;
  const D = X[0].length;
  const weights = Array(D).fill(0.1);
  let bias = 0.1;
  const losses: { epoch: number; loss: number }[] = [];

  for (let epoch = 0; epoch < epochs; epoch++) {
    let errorBiasSum = 0;
    const errorWeightSums = Array(D).fill(0);
    let lossSum = 0;

    for (let i = 0; i < N; i++) {
      let linear = bias;
      for (let j = 0; j < D; j++) linear += X[i][j] * weights[j];
      const p = sigmoid(linear);
      const error = p - y[i];
      const safeP = Math.max(1e-15, Math.min(1 - 1e-15, p));
      lossSum -= y[i] * Math.log(safeP) + (1 - y[i]) * Math.log(1 - safeP);
      errorBiasSum += error;
      for (let j = 0; j < D; j++) errorWeightSums[j] += error * X[i][j];
    }

    bias -= (lr / N) * errorBiasSum;
    for (let j = 0; j < D; j++) weights[j] -= (lr / N) * errorWeightSums[j];

    if (epoch % 150 === 0 || epoch === epochs - 1)
      losses.push({ epoch, loss: parseFloat((lossSum / N).toFixed(5)) });
  }

  return { weights, bias, losses };
}

export function predictLogistic(
  X: number[][],
  weights: number[],
  bias: number
): number[] {
  return X.map((row) => {
    let linear = bias;
    for (let j = 0; j < row.length; j++) linear += row[j] * weights[j];
    return sigmoid(linear);
  });
}

export function calculateRocAuc(
  items: { prob: number; label: number }[]
): number {
  const pos = items.filter((i) => i.label === 1);
  const neg = items.filter((i) => i.label === 0);
  if (pos.length === 0 || neg.length === 0) return 0.5;
  let count = 0;
  for (const p of pos)
    for (const n of neg)
      count += p.prob > n.prob ? 1 : p.prob === n.prob ? 0.5 : 0;
  return count / (pos.length * neg.length);
}

export interface BinCalibration {
  bucket: string;
  min: number;
  max: number;
  mid: number;
  count: number;
  wins: number;
  predSum: number;
  predicted: number;
  realized: number;
  deviation: number;
  status: string;
}

export function binPredictions(
  items: { forecast: number; outcome: number }[]
): {
  bins: BinCalibration[];
  brier: number;
  accuracy: number;
  precision: number;
  recall: number;
  f1: number;
  rocAuc: number;
  calibrationBrier: number;
  refinementBrier: number;
  outcomeVariance: number;
  observedRate: number;
} {
  const binDefs = [
    { label: "0-20%", min: 0.0, max: 0.2, mid: 10, count: 0, wins: 0, predSum: 0 },
    { label: "20-40%", min: 0.2, max: 0.4, mid: 30, count: 0, wins: 0, predSum: 0 },
    { label: "45-55%", min: 0.4, max: 0.6, mid: 50, count: 0, wins: 0, predSum: 0 },
    { label: "60-80%", min: 0.6, max: 0.8, mid: 70, count: 0, wins: 0, predSum: 0 },
    { label: "80-100%", min: 0.8, max: 1.0, mid: 90, count: 0, wins: 0, predSum: 0 },
  ];

  let sumBrier = 0;
  let correct = 0;
  let tp = 0, fp = 0, tn = 0, fn = 0;
  const n = items.length;
  const rocItems: { prob: number; label: number }[] = [];

  for (const item of items) {
    const f = item.forecast;
    const o = item.outcome;
    sumBrier += (f - o) ** 2;
    rocItems.push({ prob: f, label: o });
    const pred = f >= 0.5 ? 1 : 0;
    if (pred === 1 && o === 1) tp++;
    else if (pred === 1 && o === 0) fp++;
    else if (pred === 0 && o === 0) tn++;
    else if (pred === 0 && o === 1) fn++;
    if (pred === o) correct++;
    for (const bin of binDefs) {
      if (f >= bin.min && (f < bin.max || (bin.max === 1.0 && f <= 1.0))) {
        bin.count++;
        bin.wins += o;
        bin.predSum += f;
        break;
      }
    }
  }

  const observedRate = items.filter((i) => i.outcome === 1).length / n;
  let sumRel = 0, sumRes = 0;

  const bins = binDefs.map((bin) => {
    const avgF = bin.count > 0 ? bin.predSum / bin.count : bin.mid / 100;
    const avgO = bin.count > 0 ? bin.wins / bin.count : 0;
    if (bin.count > 0) {
      sumRel += bin.count * (avgF - avgO) ** 2;
      sumRes += bin.count * (avgO - observedRate) ** 2;
    }
    const dev = (avgO - avgF) * 100;
    const absDev = Math.abs(dev);
    return {
      bucket: bin.label,
      count: bin.count,
      wins: bin.wins,
      predicted: Math.round(avgF * 100),
      realized: Math.round(avgO * 100),
      deviation: parseFloat(dev.toFixed(1)),
      status: absDev <= 3.5 ? "Excellent" : absDev <= 7 ? "Stable" : "Calibrating",
    };
  });

  const brier = sumBrier / n;
  const prec = tp + fp > 0 ? tp / (tp + fp) : 0;
  const rec = tp + fn > 0 ? tp / (tp + fn) : 0;

  return {
    bins,
    brier,
    accuracy: correct / n,
    precision: prec,
    recall: rec,
    f1: prec + rec > 0 ? (2 * prec * rec) / (prec + rec) : 0,
    rocAuc: calculateRocAuc(rocItems),
    calibrationBrier: sumRel / n,
    refinementBrier: sumRes / n,
    outcomeVariance: observedRate * (1 - observedRate),
    observedRate,
  };
}

export function walkForwardSplit(
  X: number[][],
  y: number[],
  splitIndex: number
) {
  const trainX = X.slice(0, splitIndex);
  const trainY = y.slice(0, splitIndex);
  const testX = X.slice(splitIndex);
  const testY = y.slice(splitIndex);
  return { trainX, trainY, testX, testY };
}
