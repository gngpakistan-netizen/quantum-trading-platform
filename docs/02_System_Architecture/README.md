# XAUUSD Quantum Platform — System Architecture Document

## Document Control
| Field | Value |
|-------|-------|
| Document ID | QTP-SAD-001 |
| Version | 1.0 |
| Status | DRAFT |

## 1. Architecture Overview

```
                    ┌─────────────────────────────────────────────┐
                    │           TradingView (Browser)             │
                    │   Indicator • Dashboard • Alerts           │
                    └──────────────────┬──────────────────────────┘
                                       │ Webhook / REST
                    ┌──────────────────▼──────────────────────────┐
                    │        Cloudflare Workers (API Gateway)     │
                    │─────────────────────────────────────────────│
                    │  Auth • Rate Limit • Webhook Parse • Queue │
                    └──────────────────┬──────────────────────────┘
                                       │
          ┌────────────────────────────┼────────────────────────────┐
          │                            │                            │
          ▼                            ▼                            ▼
 ┌──────────────────┐   ┌──────────────────────────┐   ┌──────────────────┐
 │  Supabase (DB)   │   │  Python Engines (Compute)│   │ Cloudflare KV/R2 │
 │──────────────────│   │──────────────────────────│   │──────────────────│
 │ PostgreSQL       │   │ Codespaces / Replit      │   │ Config cache      │
 │ Real-time        │   │ Docker-ready (agnostic)  │   │ Static assets     │
 │ Storage          │   │ Async event-driven       │   │ Webhook logs      │
 └──────┬───────────┘   └─────────────┬────────────┘   └──────────────────┘
        │                             │
        └─────────────────────────────┘
                                      │
                    ┌─────────────────▼─────────────────────────┐
                    │           Engine Layer (Python)            │
                    │───────────────────────────────────────────│
                    │ ┌─────────┐ ┌────────┐ ┌──────────────┐  │
                    │ │Validate │ │ Audit  │ │   Forecast   │  │
                    │ └─────────┘ └────────┘ └──────────────┘  │
                    │ ┌─────────┐ ┌────────┐ ┌──────────────┐  │
                    │ │  Risk   │ │Strategy│ │   MarketInt  │  │
                    │ └─────────┘ └────────┘ └──────────────┘  │
                    │ ┌─────────┐ ┌────────┐ ┌──────────────┐  │
                    │ │ Learning│ │Improve │ │   Replay     │  │
                    │ └─────────┘ └────────┘ └──────────────┘  │
                    │ ┌──────────────────────────────────────┐  │
                    │ │      Project Intelligence Engine     │  │
                    │ └──────────────────────────────────────┘  │
                    └───────────────────────────────────────────┘
```

## 2. Engine Responsibilities

### 2.1 Market Data Engine
- Multi-source data ingestion (TradingView, OANDA, Polygon, etc.)
- Tick → OHLCV aggregation
- Multi-timeframe builder
- Cross-asset data synchronization
- Economic calendar integration

### 2.2 Data Engineering Layer
- Data cleaning and validation
- Feature calculation (all RI-1 features plus extensions)
- Regime detection
- Feature store management
- Historical data management

### 2.3 Validation Engine
- Mathematical formula verification against RI-1
- Strategy execution replay
- Dashboard synchronization checking
- Statistical performance evaluation
- Execution timing measurement

### 2.4 Audit Engine
- Continuous code quality assessment
- Mathematical correctness verification
- Statistical assumption validation
- Dashboard consistency monitoring
- Runtime performance tracking

### 2.5 Forecast Engine
- Ensemble forecasting (multiple models)
- Probability calibration
- Confidence scoring
- Historical analog matching
- Regime-conditional predictions

### 2.6 Risk Engine
- Position sizing (Kelly, fractional, volatility-adjusted)
- Portfolio heat monitoring
- Drawdown limits (daily, weekly, max)
- Correlation exposure tracking
- VaR / CVaR calculations

### 2.7 Strategy Engine
- Signal generation (entry/exit rules from RI-1)
- Order management
- Execution simulation (spread, slippage, commission, partial fills, latency)
- Trade management (stops, TPs, trailing)

### 2.8 Market Intelligence Engine
- Multi-timeframe trend analysis
- Cross-asset relationship monitoring
- Volatility regime classification
- Liquidity profiling
- Session analysis
- Macro event awareness

### 2.9 Learning Engine
- Trade outcome analysis (wins/losses, false positives/negatives)
- Feature weight optimization
- Model selection (within governed boundaries)
- Pattern discovery

### 2.10 Improvement Engine
- Candidate improvement generation
- Backtesting / simulation
- Regression testing
- Performance comparison
- Upgrade recommendation

### 2.11 Replay Engine
- Historical bar-by-bar replay
- Strategy state machine execution
- Dashboard snapshot capture
- Timing measurement
- Validation data generation

### 2.12 Knowledge Engine
- Institutional memory (formulas, rules, decisions)
- Versioned model storage
- Audit report archival
- Requirements traceability
- Decision log

### 2.13 Reporting Engine
- Performance reports
- Validation reports
- Audit reports
- Risk reports
- Dashboard data export

## 3. Data Flow

### Signal Flow (Trading Path)
```
Market Data → Feature Engineering → Strategy Engine → Risk Engine → Execution
```
### Validation Flow
```
Replay Engine → Strategy Engine → Snapshot Comparator → Validation Report
```
### Learning Flow
```
Trade Log → Learning Engine → Improvement Engine → Simulation → Recommendation
```
### Audit Flow
```
All Engines → Audit Engine → Audit Score → Knowledge Engine
```

## 4. Engine Communication

### Synchronous (Request-Reply)
- API Gateway → all engines
- Strategy Engine → Risk Engine (position sizing check)
- Validation Engine → Knowledge Engine (store results)
- Audit Engine → Knowledge Engine (store results)

### Asynchronous (Event-Driven)
- Market Data → Feature Engineering → all engines (state update)
- Replay Engine → Validation Engine (trade events)
- Improvement Engine → Strategy Engine (parameter updates after approval)

## 5. Design Principles

1. **Single Responsibility**: Each engine does one thing well.
2. **Stateless Compute**: Engines are stateless; state lives in the database and feature store.
3. **Audit Everywhere**: Every engine logs its inputs, outputs, and decisions.
4. **Testability**: Every engine has a test harness.
5. **Replaceability**: Engines communicate through contracts; any engine can be replaced if the contract is honored.
6. **Traceability**: Every output links back to RI-1 requirement IDs.
