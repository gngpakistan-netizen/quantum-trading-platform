# XAUUSD Quantum Platform — Interface Control Document (API Contracts)

## Document Control
| Field | Value |
|-------|-------|
| Document ID | QTP-ICD-001 |
| Version | 1.0 |
| Status | DRAFT |

## 1. Common Conventions

### 1.1 Base URL
- Development: `http://localhost:8000/api/v1`
- Production: `https://api.quantum-trading.com/api/v1`

### 1.2 Authentication
Header: `X-API-Key: <key>`

### 1.3 Standard Response Envelope
```json
{
  "status": "success" | "error",
  "data": { ... },
  "error": { "code": "...", "message": "..." },
  "meta": { "timestamp": "ISO8601", "request_id": "uuid" }
}
```

### 1.4 Error Codes
| Code | HTTP Status | Description |
|------|-------------|-------------|
| INVALID_REQUEST | 400 | Malformed request |
| UNAUTHORIZED | 401 | Missing/invalid API key |
| RATE_LIMITED | 429 | Too many requests |
| INTERNAL_ERROR | 500 | Unexpected error |
| ENGINE_UNAVAILABLE | 503 | Engine not ready |

## 2. API Endpoints

### 2.1 Health & Status

#### `GET /health`
Response:
```json
{
  "status": "ok",
  "version": "4.0.0",
  "engines": {
    "market_data": "healthy",
    "forecast": "healthy",
    "risk": "healthy",
    "audit": "healthy"
  },
  "last_bar": "2026-07-01T12:00:00Z",
  "uptime_seconds": 3600
}
```

### 2.2 Signals

#### `GET /signals/current`
Returns the latest signal state.
```json
{
  "timestamp": "2026-07-01T12:00:00Z",
  "symbol": "XAUUSD",
  "timeframe": "5m",
  "bias": "bullish",
  "bias_scores": { "bull": 72, "bear": 18, "range": 10 },
  "confidence": 48,
  "confidence_components": {
    "trend_quality": 65,
    "liq_quality": 42,
    "session_quality": 55,
    "analog_score": 31
  },
  "entry_signal": {
    "direction": "long",
    "price": 4126.50,
    "stop": 4118.00,
    "tp1": 4135.00,
    "tp2": 4143.50,
    "tp1_rr": 1.0,
    "tp2_rr": 2.0
  },
  "trend": { "adx": 28, "direction": "up", "strength": "moderate" },
  "structure": { "order_block": "bullish", "fvg": "present", "swing": "higher_high" },
  "liquidity": { "score": 42, "level": "low", "zone": "below_current" },
  "session": { "current": "london", "quality": 55, "bias": "bullish" },
  "analog": { "match_quality": 31, "regime": "trending", "expected_outcome": "mixed" },
  "market_regime": "trending",
  "risk_assessment": {
    "position_sizing": { "suggested_size": 0.15, "unit": "lots" },
    "portfolio_heat": 0.23,
    "limits_ok": true
  },
  "meta": { "engine_version": "4.0.0", "ri1_requirement_ids": ["R-009", "R-010", "R-011", "R-012", "R-013", "R-014"] }
}
```

#### `GET /signals/history?from=ISO&to=ISO&limit=100`
Returns historical signal log with pagination.

### 2.3 Dashboard

#### `GET /dashboard`
Full dashboard state snapshot. Structure mirrors the TradingView dashboard with all ~50 cells.

### 2.4 Positions

#### `GET /positions`
```json
{
  "open": [
    {
      "id": "uuid",
      "symbol": "XAUUSD",
      "direction": "long",
      "entry_price": 4126.50,
      "entry_time": "2026-07-01T11:55:00Z",
      "current_price": 4130.20,
      "stop_loss": 4118.00,
      "tp1": 4135.00,
      "tp2": 4143.50,
      "size": 0.15,
      "pnl": 55.50,
      "pnl_pct": 0.41,
      "rr_realized": 0.45,
      "status": "open"
    }
  ],
  "closed": [],
  "summary": { "total_trades": 142, "win_rate": 0.58, "profit_factor": 1.42 }
}
```

### 2.5 Risk

#### `GET /risk/limits`
```json
{
  "daily_loss": { "limit": -500, "current": -120, "remaining": -380, "breached": false },
  "weekly_loss": { "limit": -1500, "current": -340, "remaining": -1160, "breached": false },
  "max_drawdown": { "limit": -0.15, "current": -0.04, "breached": false },
  "portfolio_heat": { "limit": 0.50, "current": 0.23, "breached": false },
  "correlation_exposure": { "limit": 0.70, "current": 0.45, "breached": false }
}
```

#### `PUT /risk/limits`
Update risk limit parameters.

### 2.6 Validation

#### `POST /validation/run`
```json
{
  "stream": "mathematical" | "strategy" | "dashboard" | "statistical" | "timing" | "all",
  "bars": 1000,
  "date_from": "2026-01-01",
  "date_to": "2026-06-30"
}
```
Response: `{ "run_id": "uuid", "status": "running", "estimated_completion": "ISO8601" }`

#### `GET /validation/report/{run_id}`
Returns completed validation report.

### 2.7 Audit

#### `GET /audit/score`
```json
{
  "overall": 94,
  "categories": {
    "mathematical_correctness": 97,
    "statistical_validity": 82,
    "dashboard_consistency": 96,
    "timing_integrity": 95,
    "risk_management": 78,
    "code_quality": 92
  },
  "last_run": "2026-07-01T12:00:00Z",
  "recommendations": ["Improve R² baseline", "Add position sizing validation"]
}
```

### 2.8 Replay

#### `POST /replay/run`
```json
{
  "date_from": "2026-01-01",
  "date_to": "2026-06-30",
  "capture_dashboard": true,
  "capture_timing": true,
  "symbols": ["XAUUSD", "DXY", "EURUSD"],
  "timeframe": "5m"
}
```
Response: `{ "run_id": "uuid", "estimated_bars": 15000, "status": "running" }`

### 2.9 Market Data

#### `GET /market/data/{symbol}/{timeframe}?from=ISO&to=ISO&limit=5000`
Returns OHLCV bars.

#### `GET /market/features/{symbol}`
Returns current feature values for all engineered features.

## 3. Event Messages (RabbitMQ)

### 3.1 Bar Close Event
```json
{
  "type": "bar.close",
  "symbol": "XAUUSD",
  "timeframe": "5m",
  "timestamp": "ISO8601",
  "ohlcv": { "open": 4120.0, "high": 4130.0, "low": 4115.0, "close": 4126.5, "volume": 12500 },
  "features": { "adx": 28, "ema50": 4110.0, "ema200": 4080.0, "atr14": 8.5, ... }
}
```

### 3.2 Signal Event
```json
{
  "type": "signal.generated",
  "symbol": "XAUUSD",
  "timestamp": "ISO8601",
  "direction": "long",
  "confidence": 48,
  "entry": 4126.50,
  "stop": 4118.00,
  "tp1": 4135.00,
  "tp2": 4143.50
}
```

### 3.3 Trade Event
```json
{
  "type": "trade.opened" | "trade.closed" | "trade.modified",
  "trade_id": "uuid",
  "symbol": "XAUUSD",
  "direction": "long",
  "entry_price": 4126.50,
  "current_price": 4130.20,
  "pnl": 55.50,
  "status": "open" | "closed"
}
```

## 4. Message Validation (Pydantic Schemas)

All messages are validated using Pydantic models defined in `backend/api_gateway/schemas/`. Each model provides:
- Type validation
- Range validation
- Required field enforcement
- Custom validators (e.g., price must be positive)
- Example data for documentation
