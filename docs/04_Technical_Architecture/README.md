# XAUUSD Quantum Platform — Technical Architecture

## Document Control
| Field | Value |
|-------|-------|
| Document ID | QTP-TA-001 |
| Version | 1.0 |
| Status | DRAFT |

## 1. Technology Stack

| Layer | Technology | Justification |
|-------|------------|---------------|
| **Backend Language** | Python 3.11+ | Ecosystem (Pandas, NumPy, scikit-learn, statsmodels), quant community standard |
| **API Framework** | FastAPI | Async support, auto-docs, Pydantic validation, performance |
| **Data Processing** | Pandas 2.0+ / Polars | Polars preferred for performance; Pandas for compatibility |
| **Database** | Supabase (PostgreSQL) | Free tier: 500MB DB, real-time subscriptions, auth, storage, REST API |
| **Cache / Config** | Cloudflare KV | Global key-value store, free tier (100k reads/day) |
| **Queue / Events** | Supabase Realtime + pg_net | Built-in PostgreSQL listen/notify, webhook forwarding |
| **Scheduling** | GitHub Actions Cron | Free scheduled job execution (nightly validation, audit) |
| **API Gateway** | Cloudflare Workers | Free tier (100k req/day), edge deployment, zero cold start |
| **Auth** | Cloudflare Workers + Supabase Auth | API key validation + JWT (scales to OAuth later) |
| **Machine Learning** | scikit-learn, XGBoost, LightGBM | Production-tested, interpretable, well-documented |
| **Deep Learning** | PyTorch (if justified) | Reserved for V5.0 if research demonstrates benefit |
| **Timeseries** | statsmodels | ARIMA, VAR, cointegration, stationarity tests |
| **Optimization** | Optuna | Hyperparameter tuning with pruning and visualizations |
| **Config Management** | Pydantic Settings | Environment-aware, validated configuration |
| **Testing** | pytest, pytest-benchmark | Standard Python testing with performance regression |
| **CI/CD** | GitHub Actions | Native GitHub integration, matrix testing |
| **Containerization** | Docker, Docker Compose | Reproducible environments, local development; optional, not required |
| **Logging** | structlog | Structured logging, JSON output, correlation IDs |

## 1b. Deployment Model

```
┌─────────────────────────────────────────────────────┐
│               Zero-Cost Cloud Stack                  │
├─────────────────────────────────────────────────────┤
│                                                      │
│  TradingView (browser)  ───── webhook ────►          │
│                                                    │
│  Cloudflare Workers (API)  ─── REST ────► Supabase  │
│                                  ────► Cloudflare KV │
│                                                    │
│  GitHub Codespaces (dev) ───── sync ────► GitHub    │
│                                                    │
│  Python Engines (Codespaces/Replit) ── SQL ──► DB   │
│                                                    │
│  GitHub Actions Cron (nightly jobs)                 │
│                                                    │
├─────────────────────────────────────────────────────┤
│              Future Migration Path                   │
├─────────────────────────────────────────────────────┤
│  Workers → AWS Lambda / Digital Ocean Functions      │
│  Supabase → RDS / TimescaleDB Cloud                  │
│  Codespaces → ECS Fargate / Kubernetes               │
│  KV → Elasticache / DynamoDB                         │
│  All with zero code changes (env-var driven)         │
└─────────────────────────────────────────────────────┘
```
| **Migration** | Alembic | Database schema versioning |
| **Task Scheduling** | Celery / APScheduler | Background jobs, replay, reporting |

## 2. Module Structure

```
backend/
├── api_gateway/
│   ├── main.py              # FastAPI app
│   ├── routes/               # Endpoint definitions
│   ├── middleware/            # Auth, logging, rate limiting
│   └── schemas/              # Pydantic request/response models
├── common/
│   ├── config.py             # Pydantic Settings
│   ├── database.py           # DB connection management
│   ├── cache.py              # Redis client
│   ├── queue.py              # RabbitMQ client
│   ├── logging.py            # Structured logging setup
│   ├── errors.py             # Custom exception hierarchy
│   ├── metrics.py            # Prometheus metrics
│   └── utils.py              # safeDiv, clamp, etc.
├── market_data_engine/
│   ├── ingestion/            # Data source adapters
│   ├── aggregation/          # Tick→OHLCV, timeframe builder
│   ├── cleaning/             # Outlier detection, gap filling
│   └── models.py             # Bar, Tick, Quote data models
├── validation_engine/
│   ├── mathematical/         # Formula verification
│   ├── strategy/             # Trade replay
│   ├── dashboard/            # Snapshot comparison
│   ├── statistical/          # Metrics computation
│   └── timing/               # Execution timing analysis
├── audit_engine/
│   ├── code_quality/         # Static analysis rules
│   ├── mathematical/         # Formula verification
│   ├── statistical/          # Assumption validation
│   └── reporting/            # Audit report generation
├── strategy_engine/
│   ├── signals/              # Entry/exit logic from RI-1
│   ├── orders/               # Order types and management
│   ├── execution/            # Fill simulation
│   ├── management/           # Trade lifecycle (SL/TP/trailing)
│   └── models.py             # Signal, Order, Trade, Position
├── forecast_engine/
│   ├── ensemble/             # Model combination
│   ├── calibration/          # Probability calibration
│   ├── models/               # Individual predictors
│   └── analog/               # Historical pattern matching
├── risk_engine/
│   ├── sizing/               # Position sizing formulas
│   ├── limits/               # Portfolio limits
│   ├── exposure/             # Correlation exposure
│   └── analytics/            # VaR, CVaR, stress tests
├── learning_engine/
│   ├── analysis/             # Trade outcome analysis
│   ├── features/             # Feature importance
│   └── selection/            # Model selection
├── improvement_engine/
│   ├── candidates/           # Change proposal generation
│   ├── simulation/           # Backtesting
│   └── comparison/           # Performance comparison
├── replay_engine/
│   ├── runner/               # Bar-by-bar replay
│   ├── state/                # Strategy state machine
│   └── capture/              # Snapshot recording
├── knowledge_engine/
│   ├── repository/           # Storage layer
│   ├── versioning/           # Model versioning
│   └── lineage/              # Data provenance
└── reporting_engine/
    ├── templates/            # Report templates
    ├── generators/           # PDF, HTML, CSV export
    └── schedulers/           # Automated reporting
```

## 3. API Design

### Base URL: `/api/v1`

| Endpoint | Method | Description |
|----------|--------|-------------|
| `/health` | GET | Service health check |
| `/signals/current` | GET | Current signal state |
| `/signals/history` | GET | Historical signal log |
| `/dashboard` | GET | Current dashboard state |
| `/positions` | GET | Current open positions |
| `/positions/history` | GET | Closed position log |
| `/risk/limits` | GET | Current risk limit state |
| `/risk/limits` | PUT | Update risk limits |
| `/validation/run` | POST | Trigger validation run |
| `/validation/report/{id}` | GET | Validation report |
| `/audit/score` | GET | Current audit score |
| `/audit/report/{id}` | GET | Audit report |
| `/replay/run` | POST | Trigger replay run |
| `/replay/results/{id}` | GET | Replay results |
| `/market/data/{symbol}/{timeframe}` | GET | Historical market data |
| `/market/features/{symbol}` | GET | Current feature values |

## 4. Data Flow Architecture

### Signal Path (Latency-Critical)
```
Market Data (real-time) → Feature Update → Strategy Check → Risk Gate → Signal
```
Target: < 100ms from data arrival to signal output.

### Validation Path (Batch)
```
Historical Data → Replay Engine → Strategy Engine → Snapshot → Comparator → Report
```
Target: Replay 1000 bars in < 10 seconds.

### Audit Path (On-Demand)
```
All Engine Logs → Audit Engine → Scoring → Report → Knowledge Engine
```
Target: Audit of 1000 bars in < 30 seconds.

## 5. Performance Requirements

| Operation | Target | Degradation Threshold |
|-----------|--------|----------------------|
| Single bar processing | < 10ms | 50ms |
| Full dashboard calculation | < 50ms | 200ms |
| Validation replay (1000 bars) | < 10s | 30s |
| Audit run (1000 bars) | < 30s | 60s |
| API response (p95) | < 200ms | 500ms |
| Database query (typical) | < 100ms | 500ms |

## 6. Security Requirements

- All API endpoints require authentication (API key or JWT)
- Secrets managed via environment variables (never in code)
- Database credentials rotated quarterly
- No secrets or keys in git history
- Rate limiting on all API endpoints
- Input validation via Pydantic models
- SQL injection protection (parameterized queries via SQLAlchemy)
- Audit log: all configuration changes recorded
