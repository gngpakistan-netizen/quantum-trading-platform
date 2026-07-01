import type { Env } from '../index';
import { corsHeaders } from '../router';

function json(data: unknown, status = 200): Response {
  return new Response(JSON.stringify(data), {
    status,
    headers: { 'Content-Type': 'application/json', ...corsHeaders() },
  });
}

export const engineRoutes = {
  async getSnapshot(request: Request, env: Env, ctx: ExecutionContext) {
    const url = new URL(request.url);
    const engine = url.searchParams.get('engine') || '';
    const { results } = await env.DB.prepare(
      `SELECT * FROM engine_snapshots ORDER BY timestamp DESC LIMIT 20`
    ).all();
    return json(results || []);
  },

  async runEngines(_request: Request, _env: Env, _ctx: ExecutionContext) {
    return json({ success: true, message: 'Engine run triggered' });
  },

  async getStatus(_request: Request, _env: Env, _ctx: ExecutionContext) {
    return json({
      smc: { status: 'ready', lastRun: null },
      probability: { status: 'ready', lastRun: null },
      correlation: { status: 'ready', lastRun: null },
      macro: { status: 'ready', lastRun: null },
      structure: { status: 'ready', lastRun: null },
    });
  },
};
