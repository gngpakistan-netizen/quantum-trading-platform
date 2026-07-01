# XAUUSD Quantum Platform — Test Strategy

## Document Control
| Field | Value |
|-------|-------|
| Document ID | QTP-TS-001 |
| Version | 1.0 |
| Status | DRAFT |

## 1. Test Pyramid

```
         ╱╲
        ╱ E2E ╲
       ╱────────╲
      ╱Integration╲
     ╱──────────────╲
    ╱   Unit Tests   ╲
   ╱──────────────────╲
  ╱  Static Analysis  ╲
 ╱──────────────────────╲
```

## 2. Test Levels

### 2.1 Static Analysis
- **Tools**: `ruff`, `mypy`, `bandit`
- **Scope**: All Python code
- **Gate**: Must pass on every commit
- **Rules**: PEP 8, type annotations required, no unsafe eval/exec, no hardcoded secrets

### 2.2 Unit Tests
- **Framework**: `pytest`
- **Coverage Target**: 90%+ for computation paths, 80%+ overall
- **Scope**: Individual functions, formula implementations, utility methods
- **Property-based testing**: `hypothesis` for edge cases (division by zero, NaN, extreme values)
- **Run**: Every commit

### 2.3 Integration Tests
- **Scope**: Engine-to-engine communication, API endpoints, database operations
- **Database**: Test PostgreSQL instance (Docker)
- **Run**: Every PR

### 2.4 Regression Tests
- **Scope**: Compare V4.0 output against RI-1 recorded output
- **Data**: Pre-recorded feature sets and expected outputs from RI-1
- **Run**: Every PR, nightly full suite

### 2.5 Performance Tests
- **Tools**: `pytest-benchmark`
- **Metrics**: Bar processing time, API response time, replay speed
- **Thresholds**: Defined in Technical Architecture Section 5
- **Run**: Nightly

### 2.6 E2E Tests
- **Scope**: Full pipeline: data → features → signal → trade → report
- **Run**: Nightly, pre-release

## 3. Test Organization

```
tests/
├── unit/
│   ├── test_safe_div.py
│   ├── test_trend_score.py
│   ├── test_liq_score.py
│   ├── test_session_quality.py
│   ├── test_analog_score.py
│   ├── test_confidence_score.py
│   ├── test_position_sizing.py
│   └── test_execution_costs.py
├── integration/
│   ├── test_market_data_engine.py
│   ├── test_strategy_engine.py
│   ├── test_validation_engine.py
│   ├── test_risk_engine.py
│   └── test_api_endpoints.py
├── regression/
│   ├── fixtures/
│   │   ├── ri1_expected_outputs.csv
│   │   └── ri1_trade_log.csv
│   └── test_ri1_regression.py
├── performance/
│   └── test_benchmarks.py
└── conftest.py
```

## 4. Quality Gates

| Gate | Check | Blocking |
|------|-------|----------|
| Pre-commit | Static analysis, unit tests | Yes |
| PR | All unit + integration tests | Yes |
| PR | Regression tests | Yes |
| PR | Documentation required | Yes |
| Nightly | Full test suite + performance | Warning |
| Pre-release | E2E tests | Yes |
| Pre-release | Audit score >= 80 | Yes |

## 5. Test Fixtures

### 5.1 RI-1 Reference Data
- Pre-computed expected outputs for 1000+ bars
- Stored in `tests/regression/fixtures/ri1_expected_outputs.csv`
- Generated during validation Phase 1

### 5.2 Mock Market Data
- Synthesized OHLCV data for deterministic testing
- Covers all market regimes (trending, ranging, volatile, gap)
- Pre-computed feature values

## 6. Continuous Testing

```
Commit → Pre-commit hooks → Push → GitHub Actions → PR Checks → Merge → Nightly Suite
```
