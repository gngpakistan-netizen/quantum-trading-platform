# XAUUSD Quantum Platform — Functional Specification

## Document Control
| Field | Value |
|-------|-------|
| Document ID | QTP-FS-001 |
| Version | 1.0 |
| Status | DRAFT |

## 1. Feature Inventory

### 1.1 Core Features (RI-1 Derived)

| Feature ID | Name | RTM Link | Priority | Status |
|-----------|------|----------|----------|--------|
| F-001 | TP1 RR Calculation | R-001 | CRITICAL | Specified |
| F-002 | TP2 RR Calculation | R-002 | CRITICAL | Specified |
| F-003 | Division-by-Zero Guard (asianLookbackSec) | R-003 | HIGH | Specified |
| F-004 | Division-by-Zero Guard (PDH=PDL Current) | R-004 | HIGH | Specified |
| F-005 | Division-by-Zero Guard (PDH=PDL Historical) | R-005 | HIGH | Specified |
| F-006 | adaptiveATR Guard (emaSpread) | R-006 | HIGH | Specified |
| F-007 | safeDiv() Utility | R-007 | HIGH | Specified |
| F-008 | Bonferroni-Corrected Correlation | R-008 | HIGH | Specified |
| F-009 | Dashboard Bias Computation | R-009 | CRITICAL | Specified |
| F-010 | Dashboard Confidence Score | R-010 | CRITICAL | Specified |
| F-011 | Trend Score | R-011 | HIGH | Specified |
| F-012 | Liquidity Score | R-012 | HIGH | Specified |
| F-013 | Session Quality Score | R-013 | HIGH | Specified |
| F-014 | Historical Analog Engine | R-014 | HIGH | Specified |
| F-015 | SMC Structure Detection | R-015 | MEDIUM | Specified |
| F-016 | Candle Pattern Scoring | R-016 | MEDIUM | Specified |
| F-017 | Support/Resistance Scan | R-017 | MEDIUM | Specified |
| F-018 | Score Normalization (70/20/10) | R-018 | MEDIUM | Specified |
| F-019 | FVG Tracking | R-019 | MEDIUM | Specified |
| F-020 | Dashboard Label System | R-020 | MEDIUM | Specified |

### 1.2 New Institutional Features

| Feature ID | Name | RTM Link | Priority | Status |
|-----------|------|----------|----------|--------|
| F-021 | Position Sizing Engine | R-021 | CRITICAL | Specified |
| F-022 | Execution Simulation Model | R-022 | CRITICAL | Specified |
| F-023 | Portfolio Risk Management | R-023 | CRITICAL | Specified |
| F-024 | Multi-Asset Correlation Engine | R-024 | HIGH | Specified |
| F-025 | Tick-Level Execution Modeling | R-025 | MEDIUM | Deferred |
| F-026 | Portfolio Analytics Engine | R-026 | HIGH | Specified |
| F-027 | Statistical Forecasting (R² improvement) | R-027 | HIGH | Requires Research |
| F-028 | Justified Outcome Threshold | R-028 | MEDIUM | Requires Research |
| F-029 | Realistic Cost Model | R-029 | HIGH | Specified |

## 2. Feature Specifications

### F-001: TP1 RR Calculation
- **Source**: RI-1 lines 4380-4390
- **Formula**: `tpRR1 = (tp1Price - entryPrice) / (entryPrice - stopPrice)` for longs; inverted for shorts
- **Validation**: Must reproduce RI-1 values within float precision
- **Acceptance**: Automated test comparing 1000+ bars against RI-1 output

### F-002: TP2 RR Calculation
- **Source**: RI-1 lines 4390-4400
- **Formula**: `tpRR2 = (tp2Price - entryPrice) / (entryPrice - stopPrice)` for longs; inverted for shorts
- **Validation**: Independent calculation, not derived from tpRR1

### F-009: Dashboard Bias
- **Output**: bullScore, bearScore, rangeScore (0-100 each)
- **Source**: RI-1 line 4030-4090
- **Components**: trend(40%) + momentum(30%) + structure(30%)
- **Validation**: Mathematical stream template 01.

### F-010: Confidence Score
- **Output**: 0-100
- **Components**: trendQuality + liqQuality + sessionQuality + analogScore
- **Weighting**: Per RI-1 specification

### F-021: Position Sizing (New)
- **Inputs**: `riskPercent`, `accountSize`, `pointValue`, `stopDistance`
- **Formula**: `positionSize = (accountSize * riskPercent/100) / (stopDistance * pointValue)`
- **Constraints**: Max position % of account, max leverage
- **Note**: RI-1 defined these inputs but never used them; this is the first implementation.

### F-022: Execution Simulation (New)
- **Components**: spread cost (fixed or variable), slippage model (normal distribution, market impact), commission (per trade or per unit), partial fill probability, latency randomization
- **Default parameters**: XAUUSD typical spread = 0.2-0.5 pips, commission = $5/100k, slippage = 0.5-1.0 pips
- **Purpose**: Replace Pine's zero-cost assumption with realistic execution

### F-023: Portfolio Risk (New)
- **Limits**: Max drawdown (daily, weekly, overall), daily loss limit, weekly loss limit, portfolio heat limit, correlation exposure limit
- **Behavior**: When any limit is breached, new signals are rejected

## 3. Acceptance Criteria

All features must satisfy:
1. Unit tests pass (100% coverage of computation paths)
2. RI-1 regression tests pass (where applicable)
3. Validation stream passes (where applicable)
4. Audit engine score ≥ 90/100
5. Documentation complete

## 4. Feature Lifecycle

```
Proposed → Specified → Implemented → Validated → Audited → Operational
```
Each transition requires a quality gate review.
