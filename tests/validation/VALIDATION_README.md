# XAUUSD Quantum — Institutional Validation Project

## Objective
Verify that every calculation and trading decision in V3.3 (RI-1) is reproducible across 100–200 historical bars spanning multiple market regimes.

## Five Validation Streams

### 1. Mathematical Validation (`01_mathematical_template.csv`)
- Verify every dashboard field independently
- Record: source variables, formula, expected output, actual output
- Goal: 100% of dashboard values reproducible

### 2. Strategy Execution Validation (`02_strategy_execution_template.csv`)
- Replay every historical trade
- Record: signal bar, entry bar, fill bar, prices, stops, TPs, exit, RR
- Goal: every trade independently verifiable

### 3. Dashboard Synchronization (`03_dashboard_sync_template.csv`)
- For each trade, verify dashboard state at entry matches strategy state
- Record: bias, probability, all scores at time of signal
- Goal: no repainting, no future leakage, no timing drift

### 4. Statistical Validation (`04_statistical_template.csv`)
- Evaluate prediction quality beyond win rate
- Metrics: precision, recall, confusion matrix, calibration, Brier, AUC
- Goal: statistical profile of the strategy's predictions

### 5. Execution Timing (`05_timing_template.csv`)
- Measure timing divergence across signal→confirm→entry→fill
- Record every timestamp and price along the chain
- Goal: quantify execution timing assumptions for backend spec

## Market Regimes to Cover
- Trending (strong ADX > 30)
- Ranging (low ADX < 20)
- High volatility (NFP, FOMC, news events)
- Low volatility (Asian session)
- Bullish and bearish phases
- Transition periods (regime changes)

## Output
Each stream produces a completed CSV and a one-page summary report. Aggregate findings into the Issue Register at `issue_register.md`.
