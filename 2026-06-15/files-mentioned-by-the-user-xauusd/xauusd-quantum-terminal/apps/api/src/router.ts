import type { Env } from './index';

type RouteHandler = (request: Request, env: Env, ctx: ExecutionContext, params: URLPatternResult | null) => Promise<Response>;

export class Router {
  private routes: Array<{ method: string; pattern: URLPattern; handler: RouteHandler }> = [];
  private env: Env;

  constructor(env: Env) {
    this.env = env;
  }

  get(path: string, handler: RouteHandler) {
    this.routes.push({ method: 'GET', pattern: new URLPattern({ pathname: path }), handler });
  }

  post(path: string, handler: RouteHandler) {
    this.routes.push({ method: 'POST', pattern: new URLPattern({ pathname: path }), handler });
  }

  async handle(request: Request): Promise<Response> {
    const url = new URL(request.url);
    for (const route of this.routes) {
      if (route.method !== request.method) continue;
      const match = route.pattern.exec(url);
      if (match) {
        return route.handler(request, this.env, {} as ExecutionContext, match);
      }
    }
    return new Response(JSON.stringify({ error: 'Not Found', path: url.pathname }), {
      status: 404,
      headers: { 'Content-Type': 'application/json', ...corsHeaders() },
    });
  }
}

export function corsHeaders(): Record<string, string> {
  return {
    'Access-Control-Allow-Origin': '*',
    'Access-Control-Allow-Methods': 'GET, POST, OPTIONS',
    'Access-Control-Allow-Headers': 'Content-Type, Authorization',
  };
}
