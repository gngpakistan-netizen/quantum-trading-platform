//=============================================================================
// Analysis Routes — Bias, Probability, Forecast, Correlation, Structure, SMC
//=============================================================================
import type { Env } from '../index';
import { corsHeaders } from '../router';

function json(data: unknown, status = 200): Response {
  return new Response(JSON.stringify(data), {
    status,
    headers: { 'Content-Type': 'application/json', ...corsHeaders() },
  });
}

export const analysisRoutes = {
  async getBias(request: Request, env: Env, ctx: ExecutionContext) {
    const url = new URL(request.url);
    const timeframe = url.searchParams.get('timeframe') || '5m';

    // Fetch latest engine snapshot for bias
    const { results } = await env.DB.prepare(
      `SELECT output_json FROM engine_snapshots 
       WHERE engine_name = 'macro' AND timeframe = ? 
       ORDER BY timestamp DESC LIMIT 1`
    ).bind(timeframe).all();

    if (!results || results.length === 0) {
      return json({ macro: 'neutral', micro: 'neutral', current: 'neutral', longTerm: 'neutral', conviction: 0 });
    }

    const snapshot = JSON.parse((results[0] as any).output_json as string);
    return json(snapshot);
  },

  async getProbability(request: Request, env: Env, ctx: ExecutionContext) {
    const url = new URL(request.url);
    const timeframe = url.searchParams.get('timeframe') || '5m';

    const { results } = await env.DB.prepare(
      `SELECT * FROM probability_snapshots ORDER BY timestamp DESC LIMIT 1`
    ).all();

    if (!results || results.length === 0) {
      return json({ bullish: 50, bearish: 50, pdhSweep: 50, pdlSweep: 50, continuations: 50, reversal: 50 });
    }

    return json(results[0]);
  },

  async getForecast(request: Request, env: Env, ctx: ExecutionContext) {
    const url = new URL(request.url);
    const horizon = url.searchParams.get('horizon') || '1h';

    const { results } = await env.DB.prepare(
      `SELECT * FROM forecast_snapshots WHERE horizon = ? ORDER BY timestamp DESC LIMIT 1`
    ).bind(horizon).all();

    if (!results || results.length === 0) {
      return json({ horizon, expectedMove: 0, confidenceInterval: [0, 0], scenarios: {} });
    }

    return json(results[0]);
  },

  async getCorrelation(request: Request, env: Env, ctx: ExecutionContext) {
    const url = new URL(request.url);
    const symbols = (url.searchParams.get('symbols') || 'DXY,US10Y,XAGUSD,EURUSD,SPX500').split(',');

    const results = [];
    for (const symbol of symbols) {
      const { results: rows } = await env.DB.prepare(
        `SELECT output_json FROM engine_snapshots 
         WHERE engine_name = 'correlation' AND symbol = ? 
         ORDER BY timestamp DESC LIMIT 1`
      ).bind(symbol).all();

      if (rows && rows.length > 0) {
        results.push({ symbol, ...JSON.parse((rows[0] as any).output_json as string) });
      }
    }

    return json(results);
  },

  async getStructure(request: Request, env: Env, ctx: ExecutionContext) {
    const url = new URL(request.url);
    const timeframe = url.searchParams.get('timeframe') || '5m';

    const { results } = await env.DB.prepare(
      `SELECT output_json FROM engine_snapshots 
       WHERE engine_name = 'structure' AND timeframe = ? 
       ORDER BY timestamp DESC LIMIT 1`
    ).bind(timeframe).all();

    if (!results || results.length === 0) {
      return json({ bos: { bull: false, bear: false }, choch: null, mss: null });
    }

    return json(JSON.parse((results[0] as any).output_json as string));
  },

  async getSMC(request: Request, env: Env, ctx: ExecutionContext) {
    const url = new URL(request.url);
    const timeframe = url.searchParams.get('timeframe') || '5m';

    const { results } = await env.DB.prepare(
      `SELECT output_json FROM engine_snapshots 
       WHERE engine_name = 'smc' AND timeframe = ? 
       ORDER BY timestamp DESC LIMIT 1`
    ).bind(timeframe).all();

    if (!results || results.length === 0) {
      return json({ fvg: null, orderBlocks: null, liquidity: null });
    }

    return json(JSON.parse((results[0] as any).output_json as string));
  },
};
