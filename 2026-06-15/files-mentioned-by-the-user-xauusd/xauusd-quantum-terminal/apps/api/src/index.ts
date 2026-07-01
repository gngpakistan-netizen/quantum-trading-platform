//=============================================================================
// XAUUSD Quantum Terminal — Cloudflare Workers API
//=============================================================================
import { Router } from './router';
import { handleCors } from './middleware';
import { marketRoutes } from './routes/market';
import { analysisRoutes } from './routes/analysis';
import { analyticsRoutes } from './routes/analytics';
import { engineRoutes } from './routes/engines';
import { auditRoutes } from './routes/audit';
import { wsHandler } from './workers/websocket';

export interface Env {
  DB: D1Database;
  MARKET_DATA: R2Bucket;
  REALTIME_STATE: KVNamespace;
  ANALYTICS: AnalyticsEngineDataset;
  ENVIRONMENT: string;
  SYMBOLS: string;
  TIMEFRAMES: string;
  HISTORICAL_YEARS: string;
  MAX_CANDLES_RESPONSE: string;
  AUDIT_ENABLED: string;
  FRED_API_KEY?: string;
  FMP_API_KEY?: string;
}

export default {
  async fetch(request: Request, env: Env, ctx: ExecutionContext): Promise<Response> {
    const url = new URL(request.url);
    const method = request.method;

    // CORS preflight
    if (method === 'OPTIONS') return handleCors(request);

    // WebSocket upgrade
    if (url.pathname === '/ws') return wsHandler(request, env);

    const router = new Router(env);

    // --- Market Data ---
    router.get('/api/v1/market/candles', marketRoutes.getCandles);
    router.get('/api/v1/market/quote', marketRoutes.getQuote);
    router.get('/api/v1/market/symbols', marketRoutes.getSymbols);

    // --- Analysis ---
    router.get('/api/v1/analysis/bias', analysisRoutes.getBias);
    router.get('/api/v1/analysis/probability', analysisRoutes.getProbability);
    router.get('/api/v1/analysis/forecast', analysisRoutes.getForecast);
    router.get('/api/v1/analysis/correlation', analysisRoutes.getCorrelation);
    router.get('/api/v1/analysis/structure', analysisRoutes.getStructure);
    router.get('/api/v1/analysis/smc', analysisRoutes.getSMC);

    // --- Engine Snapshots ---
    router.get('/api/v1/engines/snapshot', engineRoutes.getSnapshot);
    router.post('/api/v1/engines/run', engineRoutes.runEngines);
    router.get('/api/v1/engines/status', engineRoutes.getStatus);

    // --- Audit ---
    router.get('/api/v1/audit/report', auditRoutes.getReport);
    router.get('/api/v1/audit/metrics', auditRoutes.getMetrics);
    router.get('/api/v1/audit/trace', auditRoutes.getTrace);

    // --- Performance ---
    router.get('/api/v1/performance/metrics', auditRoutes.getPerformance);
    router.get('/api/v1/performance/history', auditRoutes.getPerformanceHistory);

    // --- Analytics/ML ---
    router.post('/api/v1/analytics/train', analyticsRoutes.train);
    router.get('/api/v1/analytics/calibration', analyticsRoutes.calibration);
    router.post('/api/v1/analytics/bayesian', analyticsRoutes.bayesian);
    router.post('/api/v1/analytics/montecarlo', analyticsRoutes.montecarlo);
    router.get('/api/v1/analytics/ev', analyticsRoutes.expectedValue);
    router.get('/api/v1/analytics/forecast', analyticsRoutes.mlForecast);
    router.get('/api/v1/tests/verify', analyticsRoutes.verify);
    router.get('/api/v1/market/economic-calendar', analyticsRoutes.economicCalendar);

    // --- Health ---
    router.get('/health', async () => new Response(JSON.stringify({ status: 'ok', version: '3.0.0' }), {
      headers: { 'Content-Type': 'application/json' },
    }));

    return router.handle(request);
  },

  async scheduled(event: ScheduledEvent, env: Env, ctx: ExecutionContext): Promise<void> {
    switch (event.cron) {
      case '*/5 * * * *':
        // Refresh market data and recompute engines every 5 minutes
        const { dataIngestionWorker } = await import('./workers/ingestion');
        await dataIngestionWorker.refresh(env);
        break;
      case '0 */6 * * *':
        // Run audit and performance validation every 6 hours
        const { auditWorker } = await import('./workers/audit');
        await auditWorker.runScheduledAudit(env);
        break;
    }
  },
};
