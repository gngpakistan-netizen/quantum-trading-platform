import type { Env } from '../index';
import { corsHeaders } from '../router';

function json(data: unknown, status = 200): Response {
  return new Response(JSON.stringify(data), {
    status,
    headers: { 'Content-Type': 'application/json', ...corsHeaders() },
  });
}

export const marketRoutes = {
  async getCandles(request: Request, env: Env, ctx: ExecutionContext) {
    const url = new URL(request.url);
    const symbol = url.searchParams.get('symbol') || 'XAUUSD';
    const timeframe = url.searchParams.get('timeframe') || '5m';
    const limit = Math.min(parseInt(url.searchParams.get('limit') || '200'), 5000);
    const { results } = await env.DB.prepare(
      `SELECT * FROM candles WHERE symbol = ? AND timeframe = ? ORDER BY timestamp DESC LIMIT ?`
    ).bind(symbol, timeframe, limit).all();
    return json(results || []);
  },

  async getQuote(request: Request, env: Env, ctx: ExecutionContext) {
    const url = new URL(request.url);
    const symbol = url.searchParams.get('symbol') || 'XAUUSD';
    const { results } = await env.DB.prepare(
      `SELECT * FROM candles WHERE symbol = ? ORDER BY timestamp DESC LIMIT 1`
    ).bind(symbol).all();
    if (!results || results.length === 0) {
      return json({ symbol, price: 0, change: 0, changePercent: 0 });
    }
    const c = results[0] as any;
    return json({ symbol, price: c.close, high: c.high, low: c.low, change: 0, changePercent: 0 });
  },

  async getSymbols(_request: Request, _env: Env, _ctx: ExecutionContext) {
    return json(['XAUUSD', 'XAGUSD', 'EURUSD', 'GBPUSD', 'USDJPY']);
  },
};
