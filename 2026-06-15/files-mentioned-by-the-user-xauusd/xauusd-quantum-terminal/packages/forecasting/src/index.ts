export function bayesianUpdate(params: {
  priorAlpha: number;
  priorBeta: number;
  observations: number[];
}) {
  const { priorAlpha, priorBeta, observations } = params;
  const wins = observations.filter((o) => o === 1).length;
  const losses = observations.filter((o) => o === 0).length;
  const postAlpha = priorAlpha + wins;
  const postBeta = priorBeta + losses;

  function lnGamma(z: number): number {
    let x = z;
    let s = 0.99999999999980993;
    const coeff = [
      676.5203681218851, -1259.1392167224028, 771.32342877765313,
      -176.61502916214059, 12.507317611649031, -0.13857109526572012,
      9.9843695780195716e-6, 1.5056327351493116e-7,
    ];
    for (let i = 0; i < coeff.length; i++) s += coeff[i] / (x + i + 1);
    const t = x + 7.5;
    return 0.5 * Math.log(2 * Math.PI) + (x + 0.5) * Math.log(t) - t + Math.log(s);
  }

  const lnBeta = lnGamma(priorAlpha) + lnGamma(priorBeta) - lnGamma(priorAlpha + priorBeta);
  const lnBetaPost = lnGamma(postAlpha) + lnGamma(postBeta) - lnGamma(postAlpha + postBeta);
  const points = 50;
  const densityMap: { p: number; priorDensity: number; posteriorDensity: number }[] = [];

  for (let i = 1; i < points; i++) {
    const p = i / points;
    const lPrior = (priorAlpha - 1) * Math.log(p) + (priorBeta - 1) * Math.log(1 - p) - lnBeta;
    const lPost = (postAlpha - 1) * Math.log(p) + (postBeta - 1) * Math.log(1 - p) - lnBetaPost;
    densityMap.push({
      p: parseFloat(p.toFixed(3)),
      priorDensity: parseFloat(Math.min(30, Math.exp(lPrior)).toFixed(4)),
      posteriorDensity: parseFloat(Math.min(30, Math.exp(lPost)).toFixed(4)),
    });
  }

  return {
    priorAlpha,
    priorBeta,
    postAlpha,
    postBeta,
    wins,
    losses,
    expectedWinRatePrior: priorAlpha / (priorAlpha + priorBeta),
    expectedWinRatePosterior: postAlpha / (postAlpha + postBeta),
    densityMap,
  };
}

export function featureImportance(coefficients: number[], featureNames: string[]) {
  const total = coefficients.reduce((s, c) => s + Math.abs(c), 0);
  return coefficients.map((c, i) => ({
    feature: featureNames[i] || `w${i}`,
    score: parseFloat((Math.abs(c) / total).toFixed(4)),
    coefficient: parseFloat(c.toFixed(4)),
  }));
}
