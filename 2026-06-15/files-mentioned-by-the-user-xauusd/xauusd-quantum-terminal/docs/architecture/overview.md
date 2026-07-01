# XAUUSD Quantum Terminal — Architecture Overview

## System Architecture

```
┌─────────────────────────────────────────────────────────────────────┐
│                        CLOUDFLARE PAGES                              │
│  ┌───────────────────────────────────────────────────────────────┐  │
│  │  Next.js 15 App (Bloomberg-style Terminal Dashboard)          │  │
│  │  - Market Overview     - Bias Panel     - Probability Panel   │  │
│  │  - Chart (Lightweight) - SMC Panel      - Correlation Panel  │  │
│  │  - Forecast Panel      - Performance    - Audit Trail        │  │
│  │  - Explainability Modal (click any value)                     │  │
│  └───────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
                                │
                                │ HTTP/WS
                                ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      CLOUDFLARE WORKERS (API)                        │
│  ┌─────────┐ ┌──────────┐ ┌──────────┐ ┌─────────┐ ┌──────────┐  │
│  │ Market  │ │ Analysis │ │ Engines  │ │ Audit   │ │ WebSocket│  │
│  │ Routes  │ │ Routes   │ │ Routes   │ │ Routes  │ │ Handler  │  │
│  └────┬────┘ └────┬─────┘ └────┬─────┘ └────┬────┘ └────┬─────┘  │
│       │           │             │            │           │         │
│  ┌────▼───────────▼─────────────▼────────────▼───────────▼─────┐  │
│  │                    ENGINE LAYER                               │  │
│  │  ┌─────────┐ ┌────────────┐ ┌──────────┐ ┌──────────────┐   │  │
│  │  │ SMC     │ │ Correlation│ │Probability│ │ Forecasting  │   │  │
│  │  │ Engine  │ │ Engine     │ │ Engine    │ │ Engine       │   │  │
│  │  └─────────┘ └────────────┘ └──────────┘ └──────────────┘   │  │
│  │  ┌─────────┐ ┌────────────┐ ┌──────────┐ ┌──────────────┐   │  │
│  │  │Structure│ │ Liquidity  │ │Regime    │ │ Backtesting  │   │  │
│  │  │ Engine  │ │ Engine     │ │Engine    │ │ Engine       │   │  │
│  │  └─────────┘ └────────────┘ └──────────┘ └──────────────┘   │  │
│  └───────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
                                │
                                ▼
┌─────────────────────────────────────────────────────────────────────┐
│                   DATA INFRASTRUCTURE                                │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────────┐  │
│  │ Cloudflare D1 │  │ Cloudflare R2│  │ Cloudflare KV            │  │
│  │ (SQLite)     │  │ (Object     │  │ (Real-time cache)         │  │
│  │ - Candles    │  │  Storage)   │  │ - Latest engine state     │  │
│  │ - Ticks      │  │ - Market    │  │ - WebSocket connections   │  │
│  │ - Snapshots  │  │   Data Parq │  │ - API keys                │  │
│  │ - Audit Log  │  │ - Backups   │  │ - Session tokens          │  │
│  │ - Performance│  │             │  │                          │  │
│  └──────────────┘  └──────────────┘  └──────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────┘
```

## Key Design Decisions

### 1. Zero Lookahead Bias
Every engine operates only on data available at the computation timestamp. 
The `Calculate(index)` pattern in cTrader is replaced by explicit timestamp-gated
computations in the cloud version.

### 2. Full Traceability
Every calculation stores:
- Input data (with checksums)
- Formula (human-readable)
- Intermediate values (every step)
- Output
- Parent trace ID (for chaining)

This enables click-to-trace from any dashboard value.

### 3. Bayesian Calibration
Replaced Pine Script's cascade of ad-hoc normalizations with:
- Evidence-weighted composite scores
- Bayesian softmax: P(i) = exp(e_i/tau) / sum(exp(e_j/tau))
- Continuous calibration via reliability diagrams
- Walk-forward validation

### 4. Multi-Timeframe Architecture
Each timeframe has its own engine instance, running independently.
Results are combined in the bias aggregation step.

### 5. Audit-First Design
The audit engine runs continuously, checking:
- Data quality (gaps, outliers, timestamp ordering)
- Formula accuracy (regression tests against known outputs)
- Probability calibration (reliability diagrams)
- Model drift (performance degradation alerts)
- Version tracking (every model change is versioned)

## Data Flow

```
External Feed → Ingestion Worker → D1 / R2
                                      │
                                      ▼
                              Engine Layer
                                      │
                                      ▼
                          Snapshot Storage (D1)
                                      │
                                      ▼
                              API Routes
                                      │
                              ┌───────┴───────┐
                              ▼               ▼
                        WebSocket        REST Response
                              │               │
                              └───────┬───────┘
                                      ▼
                              Frontend Dashboard
```

## Deployment Pipeline

```
Git Push → GitHub Actions
    │
    ├── Lint + TypeCheck + Test
    │
    ├── Deploy API (Workers)
    │   └── Run DB Migrations (D1)
    │
    ├── Deploy Frontend (Pages)
    │
    └── Daily Audit (scheduled)
        └── Generate Report → Slack/Webhook
```
