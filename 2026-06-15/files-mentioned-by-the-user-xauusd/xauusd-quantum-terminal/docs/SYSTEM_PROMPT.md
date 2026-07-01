# XAUUSD Quantum 3.0 — System Prompt / Context Resume

> Paste this into any AI coding assistant to resume work on this project with full context.

---

## Project Identity

**Name:** XAUUSD Quantum 3.0 — Institutional Intelligence Terminal
**Path:** `C:\Users\PC\Documents\Codex\2026-06-15\files-mentioned-by-the-user-xauusd\xauusd-quantum-terminal\`
**Total:** 71 files, ~3,900 lines, 28 API endpoints, 12 packages, 17 DB tables
**Stack:** pnpm monorepo | Cloudflare Workers + Pages + D1 + R2 + KV | Next.js 15 + Tailwind + Zustand | TypeScript
**Deployment:** GitHub Actions → Cloudflare edge (push to main)

---

## Architecture Overview

```
apps/
  api/     Cloudflare Workers — 28 REST endpoints + WebSocket /ws
  web/     Next.js 15 — 5-panel dashboard (grid: 280px/1fr/320px × 48px/1fr/280px)
packages/
  analytics/     Logistic regression, walk-forward, bin calibration, ROC-AUC
  audit/         Brier decomposition, formula verification, confusion matrix
  backtesting/   Monte Carlo, Box-Muller, expected value calculator
  common/        Shared types (Candle, Tick, SMCOutput, etc.)
  correlation/   Pearson correlation, beta, cointegration, lead-lag detection
  data-ingestion/Economic calendar, synthetic price generation
  forecasting/   Bayesian Beta-Binomial conjugate, feature importance
  probability/   Bayesian softmax (10 evidence modules, online calibration)
  smc/           SMC engine (FVG, OB, liquidity, BOS/CHOCH/MSS, premium/discount)
  charts/        (scaffold — empty src/)
  explainability/(scaffold — empty src/)
  ui/            (scaffold — empty src/)
infrastructure/
  terraform/     D1 + R2 + KV + Workers + Pages as code
  cloudflare/    wrangler.pages.toml
.github/workflows/deploy.yml  4 jobs: quality → deploy-api → deploy-web → audit-daily
```

---

## Engines (Core Logic)

### 1. SMC Engine (`packages/smc/src/index.ts`)
- FVG detection (bullish/bearish gaps, ATR-buffered, max-age tracked)
- Order block detection (engulfing candles, >40% body ratio)
- Liquidity levels (PDH/PDL/PWH/PWL, sweep-able, sweptAbove/sweptBelow flags)
- Premium/Discount arrays (20-bar lookback, ATR-scaled)
- Pivot detection + swing pivot + BOS/CHOCH/MSS classification

### 2. Probability Engine (`packages/probability/src/index.ts`)
- Bayesian softmax: `P(i) = exp(e_i / tau) / sum(exp(e_j / tau))`
- 10 evidence modules: HTF, trend, MR, regime, structure, divergence, FVG, OB, session, characteristic
- Online calibration: adjusts tau based on prediction errors
- Outputs 7 probabilities: bullish, bearish, pdhSweep, pdlSweep, continuation, reversal, meanReversion

### 3. Correlation Engine (`packages/correlation/src/index.ts`)
- Pearson correlation at 30/60/120 windows
- Dynamic beta (regression slope)
- Lead-lag detection (max corr over ±10 lags)
- Cointegration (ADF on spread, critical -2.89)
- Regime detection (ADX-based)

### 4. Analytics/ML (`packages/analytics/src/index.ts`)
- Logistic regression via gradient descent (epochs=1200, lr=0.8, split=3500)
- 4 features: liquidity ratio, trend bias, structure shock, volatility regime
- Target: PDH sweep within 10 bars (classification)
- Walk-forward validation with full metrics per split
- Calibration curves (5 decile buckets)

### 5. Audit (`packages/audit/src/index.ts`)
- Brier decomposition: `Calibration - Refinement + OutcomeVariance`
- Box-Muller normality test (1000 samples, mean≈0, stdev≈1)
- Confusion matrix with precision/recall/F1
- Formula verification suite

### 6. Forecasting (`packages/forecasting/src/index.ts`)
- Bayesian Beta-Binomial: `Beta(alpha+W, beta+L)`
- Lanczos lnGamma approximation
- 50-point density map
- Feature importance (normalized coefficient magnitudes)

### 7. Backtesting (`packages/backtesting/src/index.ts`)
- Monte Carlo: 100 paths, log-normal compounding, ruin detection (<70% drawdown)
- Expected value: `EV = P(win) * RR - P(loss)`, threshold at 0.2

---

## API Routes (28 total)

```
GET  /health
WS   /ws

Market:    GET  /api/v1/market/candles, /quote, /symbols, /economic-calendar
Analysis:  GET  /api/v1/analysis/bias, /probability, /forecast, /correlation, /structure, /smc
Analytics: POST /api/v1/analytics/train
           GET  /api/v1/analytics/calibration, /ev, /forecast, /tests/verify
           POST /api/v1/analytics/bayesian, /montecarlo
Engines:   GET  /api/v1/engines/snapshot, /status
           POST /api/v1/engines/run
Audit:     GET  /api/v1/audit/report, /metrics, /trace
Perf:      GET  /api/v1/performance/metrics, /history
```

---

## DB Tables (17 tables, D1/SQLite)

```
candles, tick_data                          — Market data
asset_prices                                — Macro asset prices
economic_events, fred_series, economic_calendar_reactions — Economics
engine_snapshots, bias_snapshots            — Engine outputs
probability_snapshots, forecast_snapshots   — Probability + forecast
model_training_metrics, calibration_snapshots — ML training records
performance_metrics, audit_log, model_versions, calculation_trace, webhook_alerts — Ops
```

Key: all engine outputs stored as JSON in `engine_snapshots.output_json`. Traceability via `calculation_trace` with `trace_id` + `parent_trace_id` DAG.

---

## Key Design Decisions

1. **Zero lookahead** — All computations timestamp-gated. No forward-looking data.
2. **Full traceability** — Every calc stored with inputs, formula, intermediates, output.
3. **Bayesian over ad-hoc** — Softmax replaces weighted averages. Online calibration.
4. **Multi-timeframe** — Per-TF engine instances. Pivot windows scale with TF.
5. **Audit-first** — 6h cron verification + continuous formula checks.
6. **Edge deployment** — Cloudflare Workers/Pages/D1. $0-$5/mo. No server management.

---

## GitHub + Cloudflare Setup

```bash
# One-time Cloudflare setup
npx wrangler d1 create xauusd-quantum-db
npx wrangler r2 bucket create xauusd-market-data
npx wrangler kv:namespace create xauusd-quantum-realtime
cd apps/api && npx wrangler d1 migrations apply xauusd-quantum-db

# GitHub Actions secrets needed
# CF_API_TOKEN  — Cloudflare API token with Workers/Pages/D1/R2/KV permissions
# API_URL       — Workers URL (set as GitHub variable)
# WS_URL        — WebSocket URL (set as GitHub variable)
```

---

## What's Done vs What's Left

### ✅ Implemented (71 files, ~3,900 lines)
- [x] 12 monorepo packages with proper tsconfig + package.json
- [x] 28 API endpoints on Cloudflare Workers
- [x] Full Next.js 15 dashboard (5 panels: TopBar, Left, Chart, Right, Bottom)
- [x] SMC engine (FVG, OB, liquidity, BOS/CHOCH/MSS)
- [x] Probability engine (Bayesian softmax, 10 evidence modules)
- [x] Correlation engine (Pearson, beta, cointegration, lead-lag)
- [x] ML training pipeline (logistic regression GD)
- [x] Bayesian calibration framework
- [x] Monte Carlo simulation
- [x] Economic calendar + webhook alerts
- [x] Formula verification suite
- [x] 17-table D1 database schema
- [x] Terraform infrastructure
- [x] GitHub Actions CI/CD pipeline
- [x] .gitignore, pnpm-workspace, tsconfig

### 🔲 Scaffold (empty src/) — Needs Implementation
- [ ] `packages/charts/` — chart utility functions
- [ ] `packages/explainability/` — trace visualization helpers  
- [ ] `packages/ui/` — shared UI primitives
- [ ] `tests/unit/` — unit tests
- [ ] `tests/integration/` — integration tests
- [ ] `tests/e2e/` — end-to-end tests
- [ ] `packages/data-ingestion/src/` — real broker API ingestion (cTrader, Oanda)

### 🔜 Phase 2 — Production
- [ ] Live broker API integration (Oanda, cTrader, or similar)
- [ ] Order execution with risk checks
- [ ] Multi-user auth
- [ ] Notification system (Telegram, email)

### 🔜 Phase 3 — Advanced ML
- [ ] Online learning (streaming gradient descent)
- [ ] XGBoost/LightGBM via Workers AI
- [ ] RL for position sizing

---

## Prompt for an AI to Continue This Work

```
You have full context of the XAUUSD Quantum 3.0 terminal at:
C:\Users\PC\Documents\Codex\2026-06-15\files-mentioned-by-the-user-xauusd\xauusd-quantum-terminal\

This is a pnpm monorepo targeting Cloudflare Workers + Pages.
Read WHITEPAPER.md and SYSTEM_PROMPT.md for architecture context.
The root package.json scripts use "turbo dev" to run both apps/api and apps/web.

Key files to read first:
- apps/api/src/index.ts (all routes)
- packages/smc/src/index.ts (SMC engine)
- packages/probability/src/index.ts (probability engine)
- apps/web/lib/store.ts (Zustand state)
- apps/web/components/ (all dashboard panels)

Current state: 71 files, ~3,900 lines, fully functional scaffold with ML training.
Priority: implement the empty packages (charts, explainability, ui),
write tests, and add real broker API integration.
```

---

*Save this file alongside the project. Paste the entire contents into any AI tool to resume work.*
