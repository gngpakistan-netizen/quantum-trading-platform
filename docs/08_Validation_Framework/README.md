# XAUUSD Quantum Platform — Validation Framework Specification

## Document Control
| Field | Value |
|-------|-------|
| Document ID | QTP-VF-001 |
| Version | 1.0 |
| Status | DRAFT |

## 1. Validation Philosophy

Validation is not testing. Testing finds bugs. Validation proves the system does what it claims to do.

Every claim the system makes—every score, every signal, every risk calculation—must be independently reproducible. The Validation Framework exists to:

1. Prove that V4.0 reproduces RI-1 behavior where specified
2. Quantify discrepancies between Pine and Python implementations
3. Verify new V4.0 features against their specifications
4. Provide regression protection as the platform evolves

## 2. Five Validation Streams

### 2.1 Stream 1: Mathematical Validation

**Purpose**: Verify every formula produces identical output in Python vs RI-1.

**Input**: Feature set from a specific bar (timestamp + symbol + timeframe).
**Process**:
1. Extract source variables from feature store
2. Apply formula (Python implementation)
3. Compare result to RI-1 recorded output
4. Record pass/fail with tolerance

**Tolerance**: Float comparison to 6 decimal places (1e-6 relative error).

**Coverage**: Every formula in the Formula Dictionary (Section 07).

**Output**: `MathematicalValidationReport` with:
- Formula ID, Name
- Input variables
- Expected (RI-1) output
- Actual (Python) output
- Difference
- Pass/Fail
- Timestamp

### 2.2 Stream 2: Strategy Execution Validation

**Purpose**: Verify the strategy engine produces identical trades to RI-1.

**Input**: Historical bar range.
**Process**:
1. Run Replay Engine over N bars
2. Replay Engine feeds each bar to Strategy Engine
3. Strategy Engine generates signals and manages positions
4. Record every trade: entry bar, entry price, SL, TP1, TP2, exit bar, exit price, exit reason
5. Compare to RI-1 recorded trade log

**Matching Criteria**: Two trades match if:
- Same direction
- Entry price within 0.1% 
- Stop loss within 0.1%
- Entry bar matches

**Output**: `StrategyValidationReport` with:
- Total trades compared
- Matching trades (exact match)
- Matching trades (within tolerance)
- Non-matching trades (with discrepancy analysis)
- False positives (trades in V4.0 but not RI-1)
- False negatives (trades in RI-1 but not V4.0)
- Precision and recall of signal detection

### 2.3 Stream 3: Dashboard Synchronization

**Purpose**: Verify the dashboard state at signal time exactly matches the values the strategy used.

**Input**: Trade record with signal timestamp.
**Process**:
1. For each trade, capture full dashboard snapshot at signal time
2. Compare to dashboard snapshot stored with the trade
3. Verify every score matches

**Fields Checked**:
- Bias (bull/bear/range)
- Probability / confidence
- Trend score
- Liquidity score
- Session quality
- Analog prediction
- All composite scores

**Output**: `DashboardSyncReport` with per-field match/mismatch.

### 2.4 Stream 4: Statistical Validation

**Purpose**: Evaluate prediction quality beyond win rate.

**Metrics**:
- Win Rate, Profit Factor, Expectancy
- Precision, Recall, F1 Score
- Confusion Matrix (TP, FP, TN, FN)
- Calibration Curve (reliability diagram)
- Brier Score
- Log Loss
- AUC-ROC
- Maximum Drawdown
- Sharpe Ratio, Sortino Ratio
- Average Win / Average Loss
- Win/Loss Ratio

**Output**: `StatisticalValidationReport` with all metrics, charts, and interpretation.

### 2.5 Stream 5: Execution Timing

**Purpose**: Measure and quantify timing divergence across the signal→execution chain.

**Timing Points**:
```
Signal Time → Bar Confirmed → Strategy Entry → Fill → Dashboard Snapshot
```

**Measurements**:
- Signal Time to Bar Confirmation (ms)
- Bar Confirmation to Strategy Entry (ms)
- Strategy Entry to Fill (ms)
- Fill to Dashboard Snapshot (ms)
- Total: Signal to Dashboard (ms)

**Divergence Analysis**:
- Price at Signal vs Price at Fill (ticks)
- Dashboard values at Signal vs Dashboard values at Fill

**Output**: `TimingValidationReport` with per-trade timing log and aggregate statistics.

## 3. Validation Execution

### 3.1 Manual Execution (Phase 1.1)
- User loads RI-1 in TradingView
- User navigates to specific bars
- User records dashboard values and trade events in templates
- User runs Python engine over same bars
- User compares outputs

### 3.2 Automated Execution (Phase 1.2+)
```python
# Validation Engine API
validator = ValidationEngine()
report = validator.run_validation(
    stream="all",
    bars=1000,
    date_from="2026-01-01",
    date_to="2026-06-30"
)
report.to_csv("validation_report.csv")
report.to_html("validation_report.html")
```

### 3.3 CI Integration
- Validation runs triggered on every PR
- Full validation runs nightly
- Regression gate: validation must pass before merge

## 4. Acceptance Criteria

| Stream | Minimum Pass Rate | Action if Failed |
|--------|------------------|------------------|
| Mathematical | 100% | Block release |
| Strategy | 95% trade match | Investigate mismatches |
| Dashboard Sync | 100% cell match | Block release |
| Statistical | N/A (informational) | Document for research phase |
| Timing | N/A (informational) | Document for execution model |

## 5. Validation Artifacts

Each validation run produces:
1. **CSV data file**: Raw per-item comparison data
2. **JSON report**: Structured results for API consumption
3. **HTML report**: Visual summary with charts
4. **Summary**: One-page executive summary
5. **Issue Register**: Discrepancies logged for tracking

## 6. Implementation Plan

### Phase 1.1 (Current): Manual Validation
- Use CSV templates in `tests/validation/`
- Manually capture ~100 trades across regimes
- Populate templates, identify discrepancies
- **Duration**: 1-2 weeks

### Phase 1.2: Semi-Automated Validation
- Build Replay Engine in Python
- Automate trade log generation
- Automated comparison against manually captured data
- **Duration**: 2-3 weeks

### Phase 1.3: Full Automation
- Complete Validation Engine
- CI integration
- Automated dashboard snapshot comparison
- **Duration**: 2-3 weeks
