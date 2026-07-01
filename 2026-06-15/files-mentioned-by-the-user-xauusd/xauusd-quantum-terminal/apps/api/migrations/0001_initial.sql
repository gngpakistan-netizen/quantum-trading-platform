-- XAUUSD Quantum Terminal — Database Schema
-- Cloudflare D1 (SQLite-compatible)

--=============================================================================
-- MARKET DATA
--=============================================================================
CREATE TABLE IF NOT EXISTS candles (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  symbol TEXT NOT NULL,
  timeframe TEXT NOT NULL,           -- '1m','5m','15m','30m','1h','4h','1d'
  timestamp INTEGER NOT NULL,         -- Unix seconds (UTC)
  open REAL NOT NULL,
  high REAL NOT NULL,
  low REAL NOT NULL,
  close REAL NOT NULL,
  volume REAL NOT NULL,
  tick_volume INTEGER DEFAULT 0,
  source TEXT DEFAULT 'external',     -- 'external','ingested','backfill'
  checksum TEXT,                      -- SHA256 for audit
  created_at INTEGER DEFAULT (unixepoch()),
  UNIQUE(symbol, timeframe, timestamp)
);
CREATE INDEX idx_candles_lookup ON candles(symbol, timeframe, timestamp);
CREATE INDEX idx_candles_range ON candles(symbol, timeframe, timestamp, high, low);

CREATE TABLE IF NOT EXISTS tick_data (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  symbol TEXT NOT NULL,
  timestamp INTEGER NOT NULL,
  price REAL NOT NULL,
  volume REAL,
  bid REAL,
  ask REAL,
  source TEXT DEFAULT 'live',
  created_at INTEGER DEFAULT (unixepoch())
);
CREATE INDEX idx_ticks ON tick_data(symbol, timestamp);

--=============================================================================
-- CORRELATED ASSETS
--=============================================================================
CREATE TABLE IF NOT EXISTS asset_prices (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  symbol TEXT NOT NULL,               -- 'DXY','US10Y','XAGUSD','EURUSD',etc
  timestamp INTEGER NOT NULL,
  open REAL,
  high REAL,
  low REAL,
  close REAL NOT NULL,
  volume REAL,
  source TEXT DEFAULT 'external',
  created_at INTEGER DEFAULT (unixepoch()),
  UNIQUE(symbol, timestamp)
);
CREATE INDEX idx_asset_prices ON asset_prices(symbol, timestamp);

--=============================================================================
-- ECONOMIC DATA
--=============================================================================
CREATE TABLE IF NOT EXISTS economic_events (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  event_id TEXT UNIQUE,
  title TEXT NOT NULL,
  country TEXT NOT NULL,
  date INTEGER NOT NULL,              -- Unix seconds
  impact TEXT NOT NULL,               -- 'high','medium','low'
  previous REAL,
  forecast REAL,
  actual REAL,
  category TEXT,                      -- 'CPI','NFP','FOMC','PPI','PCE',etc
  source TEXT DEFAULT 'forexfactory',
  score REAL,                         -- Normalized impact score 0-100
  created_at INTEGER DEFAULT (unixepoch())
);
CREATE INDEX idx_econ_events ON economic_events(date, impact);

CREATE TABLE IF NOT EXISTS fred_series (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  series_id TEXT NOT NULL,            -- 'DFII10','T10YIE',etc
  timestamp INTEGER NOT NULL,
  value REAL NOT NULL,
  created_at INTEGER DEFAULT (unixepoch()),
  UNIQUE(series_id, timestamp)
);
CREATE INDEX idx_fred ON fred_series(series_id, timestamp);

--=============================================================================
-- ANALYTICAL OUTPUTS
--=============================================================================
CREATE TABLE IF NOT EXISTS engine_snapshots (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  symbol TEXT NOT NULL DEFAULT 'XAUUSD',
  timeframe TEXT NOT NULL DEFAULT '5m',
  timestamp INTEGER NOT NULL,         -- Bar timestamp
  engine_name TEXT NOT NULL,          -- 'structure','smc','liquidity','macro','correlation','forecast','probability'
  output_json TEXT NOT NULL,          -- Full engine output (JSON)
  version INTEGER DEFAULT 1,
  created_at INTEGER DEFAULT (unixepoch())
);
CREATE INDEX idx_snapshots ON engine_snapshots(symbol, timeframe, timestamp, engine_name);

CREATE TABLE IF NOT EXISTS bias_snapshots (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  timestamp INTEGER NOT NULL,
  macro_bias TEXT,                    -- 'bullish','bearish','neutral'
  micro_bias TEXT,
  current_bias TEXT,
  long_term_bias TEXT,
  macro_score REAL,
  micro_score REAL,
  current_score REAL,
  long_term_score REAL,
  conviction REAL,                    -- 0-100
  regime TEXT,                        -- 'trending','ranging','volatile'
  created_at INTEGER DEFAULT (unixepoch())
);

CREATE TABLE IF NOT EXISTS probability_snapshots (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  timestamp INTEGER NOT NULL,
  pdh_sweep REAL,                     -- Probability % of sweeping PDH
  pdl_sweep REAL,
  bullish REAL,
  bearish REAL,
  continuation REAL,
  reversal REAL,
  mean_reversion REAL,
  trend_expansion REAL,
  calibrated REAL,                    -- Calibration factor
  created_at INTEGER DEFAULT (unixepoch())
);

CREATE TABLE IF NOT EXISTS forecast_snapshots (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  timestamp INTEGER NOT NULL,
  horizon TEXT NOT NULL,              -- '15m','1h','4h','1d'
  expected_move REAL,
  confidence_interval_lower REAL,
  confidence_interval_upper REAL,
  probability_cone_json TEXT,         -- JSON array of {price,prob}
  bullish_scenario REAL,
  bearish_scenario REAL,
  neutral_scenario REAL,
  ensemble_size INTEGER,
  model_confidence REAL,
  created_at INTEGER DEFAULT (unixepoch())
);

--=============================================================================
-- PERFORMANCE & AUDIT
--=============================================================================
CREATE TABLE IF NOT EXISTS performance_metrics (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  date TEXT NOT NULL,                  -- 'YYYY-MM-DD'
  timeframe TEXT,
  session TEXT,                        -- 'asia','london','ny','all'
  regime TEXT,
  total_signals INTEGER,
  win_count INTEGER,
  loss_count INTEGER,
  success_rate REAL,
  precision REAL,
  recall REAL,
  f1_score REAL,
  brier_score REAL,
  calibration_error REAL,
  sharpe_ratio REAL,
  sortino_ratio REAL,
  max_drawdown REAL,
  profit_factor REAL,
  expectancy REAL,
  created_at INTEGER DEFAULT (unixepoch())
);
CREATE INDEX idx_perf ON performance_metrics(date, timeframe, session, regime);

CREATE TABLE IF NOT EXISTS audit_log (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  timestamp INTEGER NOT NULL,
  audit_type TEXT NOT NULL,            -- 'data_quality','formula','calibration','drift','security'
  status TEXT NOT NULL,                -- 'pass','warn','fail'
  engine TEXT,
  metric TEXT,
  expected REAL,
  actual REAL,
  deviation REAL,
  details TEXT,                        -- JSON details
  created_at INTEGER DEFAULT (unixepoch())
);
CREATE INDEX idx_audit ON audit_log(audit_type, status, timestamp);

CREATE TABLE IF NOT EXISTS model_versions (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  version TEXT NOT NULL,
  engine TEXT NOT NULL,
  deployed_at INTEGER NOT NULL,
  commit_sha TEXT,
  parameters_json TEXT,
  performance_before TEXT,             -- JSON
  performance_after TEXT,
  status TEXT DEFAULT 'active',        -- 'active','rolled_back','superseded'
  created_at INTEGER DEFAULT (unixepoch())
);

--=============================================================================
-- TRACEABILITY (every calculation)
--=============================================================================
CREATE TABLE IF NOT EXISTS calculation_trace (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  trace_id TEXT UNIQUE,                -- UUID
  timestamp INTEGER NOT NULL,
  engine TEXT NOT NULL,
  input_json TEXT NOT NULL,
  formula TEXT NOT NULL,
  intermediate_json TEXT,
  output_json TEXT NOT NULL,
  version INTEGER NOT NULL,
  parent_trace_id TEXT,                -- For chaining calculations
  created_at INTEGER DEFAULT (unixepoch())
);
CREATE INDEX idx_trace ON calculation_trace(engine, timestamp);
