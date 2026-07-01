export interface EconomicEvent {
  id: string;
  title: string;
  currency: string;
  time: string;
  impact: "HIGH" | "MEDIUM" | "LOW";
  forecast: string | null;
  actual: string | null;
  previous: string;
  expectedVolatilityATR?: number;
  goldSensitivityWeight?: number;
  riskScore?: number;
}

export interface EventReaction {
  event: string;
  date: string;
  deviation: string;
  surprise: string;
  reactionDXY: string;
  reactionUS10Y: string;
  reactionGold: string;
  surpriseIndex: number;
}

export function getDefaultEconomicCalendar() {
  const now = Date.now();
  const upcoming: EconomicEvent[] = [
    {
      id: "evt-001",
      title: "CPI MoM (Consumer Price Index)",
      currency: "USD",
      time: new Date(now + 25 * 60000).toISOString(),
      impact: "HIGH",
      forecast: "0.3%",
      actual: null,
      previous: "0.2%",
      expectedVolatilityATR: 4.25,
      goldSensitivityWeight: -0.65,
      riskScore: 8.5,
    },
    {
      id: "evt-002",
      title: "Non-Farm Employment Change (NFP)",
      currency: "USD",
      time: new Date(now + 180 * 60000).toISOString(),
      impact: "HIGH",
      forecast: "185K",
      actual: null,
      previous: "172K",
      expectedVolatilityATR: 6.8,
      goldSensitivityWeight: -0.8,
      riskScore: 9.2,
    },
    {
      id: "evt-003",
      title: "FOMC Interest Rate Decision",
      currency: "USD",
      time: new Date(now + 420 * 60000).toISOString(),
      impact: "HIGH",
      forecast: "5.25%",
      actual: null,
      previous: "5.25%",
      expectedVolatilityATR: 8.5,
      goldSensitivityWeight: -0.95,
      riskScore: 9.8,
    },
    {
      id: "evt-004",
      title: "Retail Sales MoM",
      currency: "USD",
      time: new Date(now + 1440 * 60000).toISOString(),
      impact: "MEDIUM",
      forecast: "0.4%",
      actual: null,
      previous: "0.1%",
      expectedVolatilityATR: 2.1,
      goldSensitivityWeight: -0.4,
      riskScore: 5.5,
    },
  ];

  const reactions: EventReaction[] = [
    {
      event: "PCE Price Index MoM",
      date: "2026-06-18",
      deviation: "+0.1%",
      surprise: "HAWKISH",
      reactionDXY: "+18 pips",
      reactionUS10Y: "+4.2 bps",
      reactionGold: "-$12.40",
      surpriseIndex: 1.5,
    },
    {
      event: "Unemployment Claims",
      date: "2026-06-17",
      deviation: "+12K",
      surprise: "DOVISH",
      reactionDXY: "-24 pips",
      reactionUS10Y: "-3.5 bps",
      reactionGold: "+$18.10",
      surpriseIndex: -2.1,
    },
  ];

  return { upcoming, reactions };
}

export function generateSyntheticPrice(
  basePrice: number,
  volatility: number,
  steps: number,
  drift = 0
): number[] {
  const prices: number[] = [basePrice];
  let u = 0, v = 0;
  for (let i = 0; i < steps; i++) {
    while (u === 0) u = Math.random();
    while (v === 0) v = Math.random();
    const z = Math.sqrt(-2 * Math.log(u)) * Math.cos(2 * Math.PI * v);
    u = v = 0;
    prices.push(prices[i] * (1 + drift + volatility * z));
  }
  return prices;
}
