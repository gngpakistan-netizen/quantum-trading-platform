# XAUUSD Quantum Platform — Data Schema & Formula Dictionary

## Document Control
| Field | Value |
|-------|-------|
| Document ID | QTP-DSD-001 |
| Version | 1.0 |
| Status | DRAFT |

## 1. Core Data Models

### 1.1 Bar (OHLCV)
```python
@dataclass
class Bar:
    symbol: str          # XAUUSD, DXY, EURUSD, XAG, US10Y, SPX
    timestamp: datetime  # Bar open time (UTC)
    timeframe: str       # 1m, 5m, 15m, 1h, 4h, 1d
    open: float
    high: float
    low: float
    close: float
    volume: float
    tick_volume: Optional[float]
    spread: Optional[float]
    vwap: Optional[float]
    # Metadata
    is_complete: bool    # True if bar is closed
    source: str          # tradingview, oanda, polygon
    quality_score: float # 0-1 data quality indicator
```

### 1.2 Tick
```python
@dataclass
class Tick:
    symbol: str
    timestamp: datetime
    price: float
    volume: float
    bid: Optional[float]
    ask: Optional[float]
    source: str
```

### 1.3 Feature Store Entry
```python
@dataclass
class FeatureSet:
    symbol: str
    timestamp: datetime
    timeframe: str
    bar: Bar                             # Link to source bar
    # Trend Features
    adx: float
    adx_direction: str                   # up / down / ranging
    ema50: float
    ema200: float
    ema_spread: float                    # (ema50 - ema200) / ema200
    htf_trend: str                       # bullish / bearish / neutral
    # Volatility Features
    atr14: float
    atr_pct: float                       # ATR / Close
    volatility_regime: str               # low / normal / high
    # Momentum Features
    roc: float                           # Rate of change
    rsi: float
    macd: float
    macd_signal: float
    macd_histogram: float
    # Structure Features
    swing_high: Optional[float]
    swing_low: Optional[float]
    pivot_high: Optional[float]
    pivot_low: Optional[float]
    order_blocks: List[OrderBlock]
    fvg_list: List[FVG]
    internal_pivots: List[float]
    # Liquidity Features
    volume_ratio: float                  # volume / volume_ma
    volume_ma: float
    liquidity_score: float               # 0-100
    liquidity_zone: str                  # above / below / at_price
    # Session Features
    session: str                         # asian / london / ny
    session_quality: float               # 0-100
    asian_range: float
    london_range: float
    ny_range: float
    # Support / Resistance
    support_levels: List[float]
    resistance_levels: List[float]
    nearest_support: float
    nearest_resistance: float
    # Correlation Features
    correlations: Dict[str, float]       # { "dxy": -0.45, ... }
    correlation_significant: Dict[str, bool]
    # Cross-Asset
    dxy_price: Optional[float]
    eurusd_price: Optional[float]
    xag_price: Optional[float]
    us10y_yield: Optional[float]
    spx_price: Optional[float]
    # Regime
    regime: str                          # trending_up / trending_down / ranging / volatile
    regime_confidence: float             # 0-1
    # Meta
    is_outlier: bool
    quality_scores: Dict[str, float]     # Per-feature quality
```

### 1.4 Signal
```python
@dataclass
class Signal:
    signal_id: uuid
    timestamp: datetime
    symbol: str
    timeframe: str
    direction: str                       # long / short / neutral
    confidence: float                    # 0-100
    entry_price: float
    stop_loss: float
    tp1: float
    tp2: float
    tp1_rr: float
    tp2_rr: float
    # Computed Scores
    bias_scores: Dict[str, float]        # { bull: 72, bear: 18, range: 10 }
    trend_score: float
    liq_score: float
    session_score: float
    analog_score: float
    confidence_components: Dict[str, float]
    # Market State at Signal
    regime: str
    feature_snapshot_id: uuid            # Link to FeatureSet
    # Dashboard Snapshot
    dashboard_snapshot_id: uuid
    # Audit Trail
    source_engine: str
    ri1_requirements: List[str]          # RTM IDs
    created_at: datetime
```

### 1.5 Trade / Position
```python
@dataclass
class Trade:
    trade_id: uuid
    signal_id: uuid                      # Link to originating Signal
    symbol: str
    direction: str
    # Entry
    entry_price: float
    entry_time: datetime
    entry_signal_time: datetime
    entry_bar_time: datetime
    # Stops / Targets
    stop_loss: float
    tp1: float
    tp2: float
    tp1_rr: float
    tp2_rr: float
    # Execution
    size: float                          # Position size in lots
    fill_quality: str                    # perfect / slippage / partial
    spread_cost: float
    commission: float
    slippage: float
    # Exit
    exit_price: Optional[float]
    exit_time: Optional[datetime]
    exit_reason: Optional[str]           # sl_hit / tp1_hit / tp2_hit / manual / signal_reversal
    exit_bar_time: Optional[datetime]
    # P&L
    pnl: Optional[float]
    pnl_pct: Optional[float]
    rr_realized: Optional[float]
    # Risk
    risk_amount: float
    risk_pct: float                      # % of account risked
    # Audit
    execution_timing_ms: float           # Signal → Fill latency
    decision_path: List[str]             # Sequence of rules triggered
    ri1_requirements: List[str]
```

### 1.6 Audit Log Entry
```python
@dataclass
class AuditEntry:
    entry_id: uuid
    timestamp: datetime
    engine: str                          # Which engine generated this
    event_type: str                      # formula_check / validation_run / trade_audit / risk_check
    status: str                          # pass / fail / warning / info
    score: Optional[float]
    details: Dict[str, Any]              # Engine-specific payload
    trace_id: str                        # Correlation ID linking related entries
    ri1_requirement_ids: List[str]
```

## 2. Database Schema (TimescaleDB + PostgreSQL)

### 2.1 Hypertables (TimescaleDB)
- `bars`: OHLCV data, partitioned by timestamp
- `ticks`: Tick data, partitioned by timestamp
- `features`: Feature store, partitioned by timestamp

### 2.2 Regular Tables (PostgreSQL)
- `signals`: Signal history
- `trades`: Trade/position records
- `positions`: Current open positions
- `dashboard_snapshots`: Full dashboard state snapshots
- `validation_runs`: Validation execution records
- `validation_results`: Per-stream validation results
- `audit_log`: Immutable audit log
- `audit_scores`: Aggregate audit scores over time
- `risk_limits`: Risk limit configuration
- `risk_events`: Risk breach events
- `model_registry`: Versioned model metadata
- `feature_definitions`: Feature documentation
- `requirements`: RTM entries

## 3. Formula Dictionary (RI-1 Derived)

### 3.1 Trend Score
```
adxComponent = min(adx / 50, 1.0) * 40
emaComponent = emaSpread > 0 ? clamp((emaSpread / 0.02) * 30, 0, 30) : 0
htfComponent = htfTrend == "bullish" ? 30 : htfTrend == "bearish" ? 0 : 15
trendScore = adxComponent + emaComponent + htfComponent
```

### 3.2 Liquidity Score
```
volumeRatio = volume / volumeMA
liqBase = clamp((volumeRatio - 1) * 50 + 50, 0, 100)
spreadPenalty = spread > 0.5 ? min((spread - 0.5) * 20, 30) : 0
liqScore = max(liqBase - spreadPenalty, 0)
```

### 3.3 Session Quality
```
asianRange = high_asian - low_asian
londonRange = high_london - low_london
nyRange = high_ny - low_ny
sessionScore = session == "london" ? 65 : session == "ny" ? 50 : 35
directionBias = close > open ? 1.15 : 0.85
sessionQuality = sessionScore * directionBias
```

### 3.4 Historical Analog Score
```
// 7-dim continuous scoring with time-weighting
dimensions = [adxDiff, atrDiff, htfDiff, structDiff, liqDiff, mrDiff, corrDiff]
weights = [0.20, 0.15, 0.15, 0.15, 0.10, 0.15, 0.10]
timeWeight = exp(-daysAgo / 30)
analogScore = sum(dim * w for dim, w in zip(dimensions, weights)) * timeWeight * 100
```

### 3.5 Confidence Score
```
trendQual = trendScore * 0.30
liqQual = liqScore * 0.20
sessionQual = sessionQuality * 0.20
analogQual = analogScore * 0.30
confidenceScore = trendQual + liqQual + sessionQual + analogQual
```

### 3.6 safeDiv()
```
safeDiv(a, b) = if not na(a) and b != 0 and not na(b) then a / b else 0.0
```

### 3.7 TP1/TP2 RR
```
// Long
tpRR1 = (tp1Price - entryPrice) / (entryPrice - stopPrice)
tpRR2 = (tp2Price - entryPrice) / (entryPrice - stopPrice)
// Short
tpRR1 = (entryPrice - tp1Price) / (stopPrice - entryPrice)
tpRR2 = (entryPrice - tp2Price) / (stopPrice - entryPrice)
```

### 3.8 Position Sizing (V4.0 New)
```
riskAmount = accountSize * (riskPercent / 100)
positionSize = riskAmount / (stopDistance * pointValue)
maxPosition = accountSize * maxPositionPct
positionSize = min(positionSize, maxPosition)
```

### 3.9 Correlation Significance (Bonferroni-Corrected)
```
// 5 independent assets after window averaging
zThreshold = 2.58  // alpha = 0.05 / 5
isSignificant = abs(zScore) > zThreshold
```

### 3.10 Bias Classification
```
maxScore = max(bullScore, bearScore, rangeScore)
bias = bullScore == maxScore ? "bullish" : bearScore == maxScore ? "bearish" : "ranging"
```

## 4. Constants and Thresholds

| Constant | Value | Source | Notes |
|----------|-------|--------|-------|
| ADX threshold | 25 | RI-1 | Trend vs ranging boundary |
| Forecast bars | 5-30 | RI-1 | Configurable input |
| Internal pivot len | 1-5 | RI-1 | Configurable input |
| Swing pivot len | 3-10 | RI-1 | Configurable input |
| S/R scan bars | 100 | RI-1 | Reduced from 150 |
| Normalization blend | 70/20/10 | RI-1 | Current/medium/long-term |
| Correlation z-score | 2.58 | I-005 | Bonferroni-corrected for 5 assets |
| Risk percent | 1.0 | Default | Configurable for V4.0 |
| Max position % | 0.05 | Default | 5% of account |
| Typical XAUUSD spread | 0.3 pips | Market | Configurable |
| Commission | $5/100k | Industry | Configurable |
