// XAUUSD Quantum Platform — Cloudflare Workers API Gateway (v2)
// Security-hardened: rate limiting, replay protection, input validation, CORS, audit logging

// ============================================================
// Configuration
// ============================================================
function getConfig(env) {
  return {
    API_KEY: env.API_KEY || '',
    SUPABASE_URL: env.SUPABASE_URL || '',
    SUPABASE_KEY: env.SUPABASE_KEY || '',
    KV: env.KV_CACHE || null,
    ENVIRONMENT: env.ENVIRONMENT || 'development',
    // Security
    RATE_LIMIT_PER_MIN: 100,
    WEBHOOK_TIMESTAMP_TOLERANCE_SEC: 300, // 5 min tolerance for clock skew
    MAX_PAYLOAD_SIZE: 102400, // 100KB
    ALLOWED_ORIGINS: ['https://www.tradingview.com', 'https://*.tradingview.com'],
  };
}

// ============================================================
// Rate Limiter (KV-backed)
// ============================================================
async function checkRateLimit(kv, clientIp, limit) {
  const key = `ratelimit:${clientIp}`;
  const now = Math.floor(Date.now() / 1000);
  const windowStart = Math.floor(now / 60) * 60; // per-minute window

  try {
    const current = await kv.get(key, 'json');
    if (current && current.window === windowStart) {
      current.count += 1;
      if (current.count > limit) {
        return { allowed: false, retryAfter: 60 - (now - windowStart) };
      }
      await kv.put(key, JSON.stringify(current), { expirationTtl: 120 });
    } else {
      await kv.put(key, JSON.stringify({ window: windowStart, count: 1 }), { expirationTtl: 120 });
    }
    return { allowed: true };
  } catch {
    // If KV fails, allow through (fail open for availability)
    return { allowed: true };
  }
}

// ============================================================
// Replay Attack Protection
// ============================================================
async function checkReplayProtection(kv, timestamp, nonce) {
  if (!timestamp || !nonce) {
    return { valid: false, reason: 'Missing timestamp or nonce' };
  }

  const now = Math.floor(Date.now() / 1000);
  const ts = parseInt(timestamp, 10);

  if (isNaN(ts) || Math.abs(now - ts) > 300) {
    return { valid: false, reason: 'Timestamp out of tolerance' };
  }

  if (typeof nonce !== 'string' || nonce.length < 8 || nonce.length > 128) {
    return { valid: false, reason: 'Invalid nonce' };
  }

  // Check nonce hasn't been used before (requires KV)
  if (kv) {
    const nonceKey = `nonce:${nonce}`;
    const used = await kv.get(nonceKey);
    if (used) {
      return { valid: false, reason: 'Nonce already used' };
    }
    // Store for 5 minutes (same as timestamp tolerance + buffer)
    await kv.put(nonceKey, '1', { expirationTtl: 600 });
  }

  return { valid: true };
}

// ============================================================
// Input Validation
// ============================================================
const ALLOWED_SYMBOLS = ['XAUUSD', 'DXY', 'EURUSD', 'XAG', 'US10Y', 'SPX', 'BTCUSD'];
const ALLOWED_DIRECTIONS = ['long', 'short', 'neutral'];
const ALLOWED_TIMEFRAMES = ['1m', '5m', '15m', '1h', '4h', '1d'];

function validateWebhookPayload(body) {
  const errors = [];

  // Symbol
  if (!body.symbol || !ALLOWED_SYMBOLS.includes(body.symbol.toUpperCase())) {
    errors.push(`Invalid or missing symbol. Allowed: ${ALLOWED_SYMBOLS.join(', ')}`);
  }

  // Direction
  if (body.direction && !ALLOWED_DIRECTIONS.includes(body.direction.toLowerCase())) {
    errors.push(`Invalid direction. Allowed: ${ALLOWED_DIRECTIONS.join(', ')}`);
  }

  // Price
  if (body.price !== undefined && (typeof body.price !== 'number' || body.price <= 0)) {
    errors.push('Price must be a positive number');
  }

  // Stop loss
  if (body.stop_loss !== undefined && (typeof body.stop_loss !== 'number' || body.stop_loss <= 0)) {
    errors.push('stop_loss must be a positive number');
  }

  // TP1 / TP2
  if (body.tp1 !== undefined && (typeof body.tp1 !== 'number' || body.tp1 <= 0)) {
    errors.push('tp1 must be a positive number');
  }
  if (body.tp2 !== undefined && (typeof body.tp2 !== 'number' || body.tp2 <= 0)) {
    errors.push('tp2 must be a positive number');
  }

  // Confidence
  if (body.confidence !== undefined && (typeof body.confidence !== 'number' || body.confidence < 0 || body.confidence > 100)) {
    errors.push('confidence must be a number between 0 and 100');
  }

  // Timeframe
  if (body.timeframe && !ALLOWED_TIMEFRAMES.includes(body.timeframe)) {
    errors.push(`Invalid timeframe. Allowed: ${ALLOWED_TIMEFRAMES.join(', ')}`);
  }

  // Timestamp + nonce for replay protection
  if (body.timestamp && (typeof body.timestamp !== 'string' && typeof body.timestamp !== 'number')) {
    errors.push('timestamp must be a string or number');
  }
  if (body.nonce && typeof body.nonce !== 'string') {
    errors.push('nonce must be a string');
  }

  return errors;
}

// ============================================================
// CORS
// ============================================================
function corsHeaders(env) {
  const origin = env.ENVIRONMENT === 'production'
    ? 'https://www.tradingview.com'
    : '*';

  return {
    'Access-Control-Allow-Origin': origin,
    'Access-Control-Allow-Methods': 'GET, POST, PUT, DELETE, OPTIONS',
    'Access-Control-Allow-Headers': 'Content-Type, X-API-Key, X-Timestamp, X-Nonce',
    'Access-Control-Max-Age': '86400',
  };
}

// ============================================================
// Response Helpers
// ============================================================
function jsonResponse(data, status = 200, env = null) {
  return new Response(JSON.stringify(data), {
    status,
    headers: {
      'Content-Type': 'application/json',
      ...corsHeaders(env),
    },
  });
}

function errorResponse(message, code = 'INTERNAL_ERROR', status = 500, env = null) {
  return jsonResponse({
    status: 'error',
    error: { code, message },
    meta: { timestamp: new Date().toISOString(), request_id: crypto.randomUUID() },
  }, status, env);
}

function successResponse(data, status = 200, env = null) {
  return jsonResponse({
    status: 'success',
    data,
    meta: { timestamp: new Date().toISOString(), request_id: crypto.randomUUID() },
  }, status, env);
}

// ============================================================
// Supabase REST Client
// ============================================================
async function supabaseQuery(config, table, method = 'GET', body = null) {
  const url = `${config.SUPABASE_URL}/rest/v1/${table}`;
  const headers = {
    'apikey': config.SUPABASE_KEY,
    'Authorization': `Bearer ${config.SUPABASE_KEY}`,
    'Content-Type': 'application/json',
    'Prefer': 'return=representation',
  };

  const options = { method, headers };
  if (body && ['POST', 'PATCH', 'PUT'].includes(method)) {
    options.body = JSON.stringify(body);
  }

  const response = await fetch(url, options);
  if (!response.ok) {
    throw new Error(`Supabase error ${response.status}: ${await response.text()}`);
  }
  return response.json();
}

// ============================================================
// Webhook Handler
// ============================================================
async function handleWebhook(request, config) {
  try {
    // Check content type
    const contentType = request.headers.get('Content-Type') || '';
    if (!contentType.includes('application/json')) {
      return errorResponse('Content-Type must be application/json', 'INVALID_REQUEST', 400, config);
    }

    // Read body with size limit
    const bodyBytes = await request.arrayBuffer();
    if (bodyBytes.byteLength > config.MAX_PAYLOAD_SIZE) {
      return errorResponse('Payload exceeds maximum size (100KB)', 'PAYLOAD_TOO_LARGE', 413, config);
    }

    let body;
    try {
      body = JSON.parse(new TextDecoder().decode(bodyBytes));
    } catch {
      return errorResponse('Invalid JSON payload', 'INVALID_JSON', 400, config);
    }

    // Validate payload
    const validationErrors = validateWebhookPayload(body);
    if (validationErrors.length > 0) {
      return errorResponse(validationErrors.join('; '), 'VALIDATION_ERROR', 400, config);
    }

    // Replay protection
    if (config.KV) {
      const replay = await checkReplayProtection(config.KV, body.timestamp, body.nonce);
      if (!replay.valid) {
        return errorResponse(`Replay protection: ${replay.reason}`, 'REPLAY_DETECTED', 429, config);
      }
    }

    // Enrich payload
    const enriched = {
      ...body,
      received_at: new Date().toISOString(),
      source_ip: request.headers.get('CF-Connecting-IP') || 'unknown',
      user_agent: request.headers.get('User-Agent') || 'unknown',
    };

    // Log to audit trail
    await supabaseQuery(config, 'audit_log', 'POST', {
      engine: 'api_gateway',
      event_type: 'webhook_received',
      status: 'info',
      details: {
        symbol: body.symbol,
        direction: body.direction,
        price: body.price,
        source_ip: enriched.source_ip,
      },
    }).catch(() => {}); // Non-blocking

    // Store signal
    const signal = {
      symbol: body.symbol.toUpperCase(),
      timestamp: enriched.received_at,
      timeframe: body.timeframe || '5m',
      direction: (body.direction || 'neutral').toLowerCase(),
      entry_price: body.price || 0,
      stop_loss: body.stop_loss || 0,
      tp1: body.tp1 || 0,
      tp2: body.tp2 || 0,
      confidence: body.confidence || 0,
      source_engine: 'tradingview_webhook',
    };

    const result = await supabaseQuery(config, 'signals', 'POST', signal);

    return successResponse({ signal_id: result[0]?.id }, 200, config);

  } catch (err) {
    return errorResponse(`Webhook processing failed: ${err.message}`, 'WEBHOOK_ERROR', 500, config);
  }
}

// ============================================================
// REST API Handlers
// ============================================================
async function handleHealth(config) {
  return successResponse({
    status: 'ok',
    version: '4.0.0',
    environment: config.ENVIRONMENT,
    uptime_seconds: 0,
  }, 200, config);
}

async function handleGetSignals(config, url) {
  const limit = Math.min(parseInt(url.searchParams.get('limit') || '20'), 100);
  const symbol = url.searchParams.get('symbol') || 'XAUUSD';

  const data = await supabaseQuery(config,
    `signals?symbol=eq.${symbol}&order=created_at.desc&limit=${limit}`
  );
  return successResponse({ signals: data, count: data.length }, 200, config);
}

async function handleGetDashboard(config) {
  const data = await supabaseQuery(config,
    'dashboard_snapshots?order=timestamp.desc&limit=1'
  );
  return successResponse(data[0] || null, 200, config);
}

async function handleGetPositions(config) {
  const data = await supabaseQuery(config,
    "trades?status=eq.open&select=id,symbol,direction,entry_price,entry_time,stop_loss,tp1,tp2,size,pnl,status&order=entry_time.desc"
  );
  return successResponse(data, 200, config);
}

async function handleGetRiskLimits(config) {
  const data = await supabaseQuery(config, 'risk_limits');
  return successResponse(data, 200, config);
}

async function handleGetAuditScore(config) {
  const data = await supabaseQuery(config,
    'audit_scores?order=run_timestamp.desc&limit=1'
  );
  return successResponse(data[0] || null, 200, config);
}

async function handlePieStatus(config) {
  const reqCount = await supabaseQuery(config, 'requirements?select=id,status');
  const todoCount = await supabaseQuery(config, "pie_todos?select=id,status");
  const auditLatest = await supabaseQuery(config, 'audit_scores?order=run_timestamp.desc&limit=1');

  const reqDone = reqCount.filter(r => r.status === 'validated' || r.status === 'operational').length;
  const todoDone = todoCount.filter(t => t.status === 'completed').length;

  return successResponse({
    requirements: { total: reqCount.length, done: reqDone, pending: reqCount.length - reqDone },
    todos: { total: todoCount.length, done: todoDone, open: todoCount.length - todoDone },
    audit: auditLatest[0] || null,
  }, 200, config);
}

// ============================================================
// Main Router
// ============================================================
async function handleRequest(request, config) {
  const url = new URL(request.url);
  const path = url.pathname;
  const method = request.method;

  // CORS preflight
  if (method === 'OPTIONS') {
    return new Response(null, {
      status: 204,
      headers: corsHeaders(config),
    });
  }

  // Rate limiting
  if (config.KV) {
    const clientIp = request.headers.get('CF-Connecting-IP') || 'unknown';
    const rateCheck = await checkRateLimit(config.KV, clientIp, config.RATE_LIMIT_PER_MIN);
    if (!rateCheck.allowed) {
      return errorResponse(
        `Rate limit exceeded. Retry after ${rateCheck.retryAfter}s`,
        'RATE_LIMITED',
        429,
        config
      );
    }
  }

  // Public endpoints (no auth)
  if (path === '/health' && method === 'GET') {
    return handleHealth(config);
  }
  if (path === '/webhook' && method === 'POST') {
    return handleWebhook(request, config);
  }

  // All other endpoints require auth
  const apiKey = request.headers.get('X-API-Key');
  if (!apiKey || apiKey !== config.API_KEY) {
    return errorResponse('Invalid or missing API key', 'UNAUTHORIZED', 401, config);
  }

  // API v1 routes
  const routes = [
    { path: '/api/v1/signals', method: 'GET', handler: () => handleGetSignals(config, url) },
    { path: '/api/v1/signals/current', method: 'GET', handler: () => handleGetSignals(config, url) },
    { path: '/api/v1/dashboard', method: 'GET', handler: () => handleGetDashboard(config) },
    { path: '/api/v1/positions', method: 'GET', handler: () => handleGetPositions(config) },
    { path: '/api/v1/risk/limits', method: 'GET', handler: () => handleGetRiskLimits(config) },
    { path: '/api/v1/audit/score', method: 'GET', handler: () => handleGetAuditScore(config) },
    { path: '/api/v1/pie/status', method: 'GET', handler: () => handlePieStatus(config) },
  ];

  for (const route of routes) {
    if (path === route.path && method === route.method) {
      return route.handler();
    }
  }

  return errorResponse('Not found', 'NOT_FOUND', 404, config);
}

// ============================================================
// Worker Entry Point
// ============================================================
export default {
  async fetch(request, env, ctx) {
    const config = getConfig(env);

    // Validate required config
    if (!config.API_KEY && config.ENVIRONMENT === 'production') {
      return errorResponse('Server configuration error', 'CONFIG_ERROR', 500, config);
    }

    return handleRequest(request, config);
  },
};
