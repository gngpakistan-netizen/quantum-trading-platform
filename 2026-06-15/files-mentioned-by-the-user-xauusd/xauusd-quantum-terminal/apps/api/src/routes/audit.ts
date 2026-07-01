import type { Env } from '../index';
import { corsHeaders } from '../router';

function json(data: unknown, status = 200): Response {
  return new Response(JSON.stringify(data), {
    status,
    headers: { 'Content-Type': 'application/json', ...corsHeaders() },
  });
}

export const auditRoutes = {
  async getReport(request: Request, env: Env, ctx: ExecutionContext) {
    const { results } = await env.DB.prepare(
      `SELECT * FROM audit_log ORDER BY checked_at DESC LIMIT 50`
    ).all();
    return json(results || []);
  },

  async getMetrics(request: Request, env: Env, ctx: ExecutionContext) {
    const { results } = await env.DB.prepare(
      `SELECT * FROM performance_metrics ORDER BY date DESC LIMIT 30`
    ).all();
    return json(results || []);
  },

  async getTrace(request: Request, env: Env, ctx: ExecutionContext) {
    const url = new URL(request.url);
    const traceId = url.searchParams.get('traceId') || '';
    if (traceId) {
      const { results } = await env.DB.prepare(
        `SELECT * FROM calculation_trace WHERE trace_id = ?`
      ).bind(traceId).all();
      return json(results?.[0] || null);
    }
    const { results } = await env.DB.prepare(
      `SELECT * FROM calculation_trace ORDER BY timestamp DESC LIMIT 20`
    ).all();
    return json(results || []);
  },

  async getPerformance(request: Request, env: Env, ctx: ExecutionContext) {
    const { results } = await env.DB.prepare(
      `SELECT * FROM performance_metrics ORDER BY date DESC LIMIT 1`
    ).all();
    if (!results || results.length === 0) {
      return json({ successRate: 0, sharpe: 0, maxDrawdown: 0, profitFactor: 0, totalSignals: 0 });
    }
    return json(results[0]);
  },

  async getPerformanceHistory(request: Request, env: Env, ctx: ExecutionContext) {
    const { results } = await env.DB.prepare(
      `SELECT date, success_rate, sharpe, max_drawdown FROM performance_metrics ORDER BY date ASC LIMIT 100`
    ).all();
    return json(results || []);
  },
};
