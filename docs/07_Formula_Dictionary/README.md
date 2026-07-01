# XAUUSD Quantum Platform — Formula Dictionary (Complete)

## Document Control
| Field | Value |
|-------|-------|
| Document ID | QTP-FD-001 |
| Version | 1.0 |
| Status | DRAFT |

## 1. Overview

This document catalogs every formula, variable, and constant used in the XAUUSD Quantum RI-1 implementation and the V4.0 backend. Each entry includes:
- Formula ID
- Name
- Source (RI-1 line reference where applicable)
- Mathematical expression
- Variable definitions
- Validation status
- Dependencies

## 2. Complete Formula Catalog

### 2.1 Utility Functions

#### F-U001: safeDiv
- **Source**: RI-1 line 60, Audit I-004
- **Expression**: `safeDiv(a, b) = (not na(a) and b != 0 and not na(b)) ? a / b : 0.0`
- **Variables**: `a` (numerator), `b` (denominator)
- **Validation**: PASS (audited)
- **Dependencies**: None

#### F-U002: clamp
- **Expression**: `clamp(val, lo, hi) = max(lo, min(val, hi))`
- **Validation**: PASS (standard utility)

### 2.2 Trend Indicators

#### F-T001: ADX
- **Source**: RI-1 line 380
- **Expression**: Built-in TA function `ta.adx(high, low, close, len)`
- **Variables**: `len = 14`
- **Direction**: `+DI > -DI ? "up" : "down"`
- **Validation**: PASS (standard Pine function)
- **Dependencies**: OHLCV data

#### F-T002: EMA
- **Source**: RI-1 line 390
- **Expression**: `ta.ema(close, len)`
- **Variables**: `len50 = 50`, `len200 = 200`
- **Validation**: PASS (standard Pine function)

#### F-T003: EMA Spread
- **Source**: RI-1 line 410
- **Expression**: `emaSpread = (ema50 - ema200) / ema200`
- **Guard**: `ema200 != 0 and not na(ema200)`
- **Validation**: PASS (audited)

#### F-T004: Trend Composite Score
- **Formula ID**: See F-011 in data schema (Section 3.1)

### 2.3 Volatility

#### F-V001: ATR
- **Source**: RI-1 line 400
- **Expression**: `ta.atr(len)`
- **Variables**: `len = 14`
- **Validation**: PASS (standard Pine function)

#### F-V002: ATR Percentage
- **Expression**: `atrPct = atr14 / close`
- **Guard**: `close != 0`
- **Validation**: PASS

### 2.4 Momentum

#### F-M001: Rate of Change
- **Expression**: `roc = (close - close[len]) / close[len]`
- **Variables**: `len = 10`
- **Validation**: PASS

#### F-M002: RSI
- **Expression**: `ta.rsi(close, len)`
- **Variables**: `len = 14`
- **Validation**: PASS (standard Pine function)

#### F-M003: MACD
- **Expression**: `[macd, signal, hist] = ta.macd(close, 12, 26, 9)`
- **Validation**: PASS (standard Pine function)

### 2.5 Structure

#### F-S001: Swing Pivot Detection
- **Source**: RI-1 line 1200
- **Expression**: `swingHigh = ta.pivothigh(high, left, right)`
- **Variables**: `left = right = swingPivotLen`
- **Validation**: PASS

#### F-S002: Order Block Detection
- **Source**: RI-1 line 2100
- **Expression**: Complex multi-bar pattern detection (see RI-1 lines 2100-2250)
- **Validation**: PASS

#### F-S003: FVG Detection
- **Source**: RI-1 line 2400
- **Expression**: Gap detection between consecutive candle wicks
- **Validation**: PASS (incremental, no loop)

### 2.6 Liquidity

#### F-L001: Volume Ratio
- **Expression**: `volumeRatio = volume / ta.sma(volume, 20)`
- **Validation**: PASS

#### F-L002: Liquidity Score
- **Formula ID**: See Data Schema Section 3.2

### 2.7 Session

#### F-SE001: Session Classification
- **Source**: RI-1 line 3100
- **Expression**: Time-based classification
  - Asian: 00:00-09:00 UTC
  - London: 08:00-17:00 UTC
  - NY: 13:00-22:00 UTC
- **Validation**: PASS

#### F-SE002: Session Quality
- **Formula ID**: See Data Schema Section 3.3

### 2.8 Correlation

#### F-C001: Rolling Correlation
- **Expression**: `ta.correlation(asset1, asset2, len)`
- **Variables**: `len = 50`
- **Assets**: XAUUSD vs [DXY, EURUSD, XAG, US10Y, SPX]

#### F-C002: Significance Test
- **Expression**: `zScore = correlation * sqrt((len - 2) / (1 - correlation^2))`
- **Threshold**: `abs(zScore) > 2.58` (Bonferroni-corrected for 5 assets)
- **Validation**: PASS (Audit I-005)

### 2.9 Historical Analog

#### F-H001: 7-Dim Analog Scoring
- **Formula ID**: See Data Schema Section 3.4

### 2.10 Composite Scores

#### F-CP001: Bias Scores
- **Source**: RI-1 line 4030-4090
- **Components**: trend(40%) + momentum(30%) + structure(30%)
- **Validation**: PASS

#### F-CP002: Confidence Score
- **Formula ID**: See Data Schema Section 3.5

#### F-CP003: Normalization (70/20/10)
- **Expression**: `normalizedScore = currentScore * 0.70 + mediumScore * 0.20 + longScore * 0.10`
- **Validation**: PASS

### 2.11 Risk & Execution (V4.0 New)

#### F-R001: Position Sizing
- **Formula ID**: See Data Schema Section 3.8

#### F-R002: Execution Cost
- **Expression**: `totalCost = spreadCost + commission + slippage`
- **Components**: each modeled separately

#### F-R003: Portfolio Heat
- **Expression**: `heat = sum(positionValue_i) / accountSize`
- **Limit**: Configurable (default 0.50)

#### F-R004: Drawdown
- **Expression**: `dd = (peakEquity - currentEquity) / peakEquity`
- **Limits**: Daily, weekly, maximum

## 3. Variable Reference

| Variable | Type | Range | Source | Description |
|----------|------|-------|--------|-------------|
| adx | float | 0-100 | RI-1 | Average Directional Index |
| atr14 | float | 0+ | RI-1 | Average True Range (14) |
| ema50 | float | price | RI-1 | 50-period EMA |
| ema200 | float | price | RI-1 | 200-period EMA |
| emaSpread | float | -1 to 1 | RI-1 | (ema50-ema200)/ema200 |
| bullScore | float | 0-100 | RI-1 | Bullish evidence score |
| bearScore | float | 0-100 | RI-1 | Bearish evidence score |
| rangeScore | float | 0-100 | RI-1 | Ranging evidence score |
| confidenceScore | float | 0-100 | RI-1 | Signal confidence |
| trendScore | float | 0-100 | RI-1 | Trend quality score |
| liqScore | float | 0-100 | RI-1 | Liquidity score |
| sessionQuality | float | 0-100 | RI-1 | Session quality score |
| analogScore | float | 0-100 | RI-1 | Historical analog score |
| volumeRatio | float | 0+ | RI-1 | Current/avg volume |
| tpRR1 | float | 0+ | RI-1 | TP1 risk:reward |
| tpRR2 | float | 0+ | RI-1 | TP2 risk:reward |
| riskPercent | float | 0-100 | RI-1 | % risk per trade |
| accountSize | float | 0+ | RI-1 | Account equity |

## 4. Formula Dependencies Graph

```
safeDiv → [EMA Spread, ATR%]
EMA Spread → Trend Composite
ADX → Trend Composite, Bias
ATR → Analog, Volatility Regime
Volume Ratio → Liquidity Score
Correlations → Analog Score
Bias + Confidence → Signal Generation
Risk Percent + Account Size → Position Sizing (V4.0)
```

## 5. Version History

| Version | Date | Changes |
|---------|------|---------|
| 1.0 | 2026-07-01 | Initial catalog from RI-1 + V4.0 extensions |
