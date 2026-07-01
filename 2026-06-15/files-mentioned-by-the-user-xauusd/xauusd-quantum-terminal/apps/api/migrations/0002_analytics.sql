-- Analytics Tables — ML Training, Calibration, Webhook Alerts

CREATE TABLE IF NOT EXISTS model_training_metrics (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  trained_at INTEGER NOT NULL DEFAULT (unixepoch()),
  engine_name TEXT NOT NULL DEFAULT 'logistic_regression',
  epoch INTEGER NOT NULL DEFAULT 1200,
  learning_rate REAL NOT NULL DEFAULT 0.8,
  train_size INTEGER NOT NULL,
  test_size INTEGER NOT NULL,
  accuracy REAL,
  brier_score REAL,
  precision REAL,
  recall REAL,
  f1_score REAL,
  roc_auc REAL,
  calibration_brier REAL,
  refinement_brier REAL,
  coefficients_json TEXT,
  feature_importance_json TEXT,
  loss_history_json TEXT,
  calibration_curves_json TEXT,
  created_at INTEGER DEFAULT (unixepoch())
);

CREATE TABLE IF NOT EXISTS calibration_snapshots (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  timestamp INTEGER NOT NULL DEFAULT (unixepoch()),
  dataset_size INTEGER,
  brier_score REAL,
  calibration_brier REAL,
  refinement_brier REAL,
  outcome_variance REAL,
  reliability_json TEXT,
  overall_confidence TEXT,
  created_at INTEGER DEFAULT (unixepoch())
);

CREATE TABLE IF NOT EXISTS webhook_alerts (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  alert_id TEXT UNIQUE,
  event TEXT NOT NULL,
  price REAL NOT NULL,
  symbol TEXT NOT NULL DEFAULT 'XAUUSD',
  timestamp INTEGER NOT NULL,
  raw_payload TEXT,
  processed BOOLEAN DEFAULT 0,
  response_json TEXT,
  created_at INTEGER DEFAULT (unixepoch())
);
CREATE INDEX idx_webhook_alerts ON webhook_alerts(timestamp);

CREATE TABLE IF NOT EXISTS economic_calendar_reactions (
  id INTEGER PRIMARY KEY AUTOINCREMENT,
  event_id TEXT,
  event_name TEXT NOT NULL,
  date TEXT NOT NULL,
  deviation TEXT,
  surprise TEXT,
  dx_reaction TEXT,
  yield_reaction TEXT,
  gold_reaction TEXT,
  surprise_index REAL,
  created_at INTEGER DEFAULT (unixepoch())
);
