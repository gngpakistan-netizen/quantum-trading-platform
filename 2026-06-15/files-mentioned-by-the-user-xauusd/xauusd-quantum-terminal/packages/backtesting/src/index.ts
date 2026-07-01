export function boxMuller(): number {
  let u = 0, v = 0;
  while (u === 0) u = Math.random();
  while (v === 0) v = Math.random();
  return Math.sqrt(-2 * Math.log(u)) * Math.cos(2 * Math.PI * v);
}

export function monteCarlo(params: {
  startingBalance: number;
  riskFraction: number;
  rewardUnits: number;
  winRate: number;
  numTrades: number;
  numPaths?: number;
  ruinThreshold?: number;
}) {
  const {
    startingBalance,
    riskFraction,
    rewardUnits,
    winRate,
    numTrades,
    numPaths = 100,
    ruinThreshold = 0.7,
  } = params;

  const paths: number[][] = [];
  let ruinedCount = 0;
  const finalBalances: number[] = [];

  for (let path = 0; path < numPaths; path++) {
    let bal = startingBalance;
    const pathPrices = [startingBalance];
    for (let t = 0; t < numTrades; t++) {
      const isWin = Math.random() < winRate;
      bal *= isWin ? 1 + riskFraction * rewardUnits : 1 - riskFraction;
      if (bal < 0) bal = 0;
      pathPrices.push(bal);
    }
    paths.push(pathPrices);
    finalBalances.push(bal);
    if (bal < startingBalance * ruinThreshold) ruinedCount++;
  }

  finalBalances.sort((a, b) => a - b);
  const median = finalBalances[Math.floor(numPaths / 2)];
  const winRatio = finalBalances.filter((f) => f > startingBalance).length / numPaths;

  const chartData: any[] = [];
  for (let t = 0; t <= numTrades; t++) {
    const point: any = { trade: `T${t}` };
    for (let p = 0; p < Math.min(8, numPaths); p++)
      point[`path_${p}`] = parseFloat(paths[p][t].toFixed(1));
    chartData.push(point);
  }

  return {
    medianBalance: parseFloat(median.toFixed(2)),
    winRatioPaths: winRatio,
    probabilityOfRuin: ruinedCount / numPaths,
    startingBalance,
    riskFraction,
    rewardUnits,
    winRate,
    chartData,
  };
}

export function expectedValue(params: {
  winProb: number;
  lossProb: number;
  rewardRatio: number;
  riskAmount?: number;
  maxEVThreshold?: number;
}) {
  const { winProb, lossProb, rewardRatio, riskAmount = 1, maxEVThreshold = 0.2 } = params;
  const ev = winProb * rewardRatio - lossProb;
  return {
    expectedValue: parseFloat(ev.toFixed(4)),
    riskRewardRatio: rewardRatio,
    executionAction: ev >= maxEVThreshold ? "EXEC_TRIGGER" : "EXEC_HOLD",
  };
}
