# Engine Documentation — XAUUSD Quantum 3.0

## 1. HTF Trend Stack (5 TF)
**Lines:** 187-252  
**Purpose:** Determine trend direction across 5 timeframes (5m/15m/1h/4h/1D) without repainting  
**Method:** Each TF requests `close[1]`, `ema(close[1], 20/50/200)` via `request.security(..., lookahead=barmerge.lookahead_off)`  
**Score (0-100):** `htfTrendScore()` = `(close>ema200 ? 40:0) + (ema20>ema50 ? 40:0) + (close>ema20 ? 20:0)`  
**Direction:** `≥60 Bull, ≤40 Bear, else Neutral`  
**Gating:** `chartTFSeconds < N` prevents same-TF double-count (FA-001)  
**Uses:** Arrow display (5 arrows), `htfFullLong/Short = sum(Bull) vs sum(Bear)`, signal gates

---

## 2. Adaptive ATR Regime Engine
**Lines:** 254-282  
**Purpose:** Detect trending vs ranging regime using smoothed ATR + ADX with hysteresis  
**Method:**  
- `adaptiveATR = ta.sma(ta.atr(14), 10)` — smoothed ATR  
- `adxVal = ta.sma(ta.adx(high, low, close, 14), 5)` — smoothed ADX  
- Hysteresis band: Trending only above `adxThreshold+3`, ranging only below `adxThreshold-3`  
**Outputs:** `regimeTrending`, `regimeRanging`, `regimeDead`  
**Uses:** Signal filtering, volatility tags, displacement thresholds

---

## 3. Mean Reversion Analysis
**Lines:** 284-300  
**Purpose:** Measure distance from fair value (VWAP, EMAs, Z-score) in ATR terms  
**Weights (M-003):** VWAP=26, EMA20=22, EMA100=22, EMA200=17, Z-score=13 → sum=100  
**Score:** `mrComposite = clamp(weighted sum, -100, 100)`  
**Classification:** `|score|≥60 Overextended, ≥30 Extended, <30 Normal`  
**Uses:** Setup scoring, MR confidence signal, bias modulation

---

## 4. Session Engine (DST-aware)
**Lines:** 316-393  
**Purpose:** Identify current trading session (Asian/London/NY) with accurate DST boundaries  
**US DST:** 2nd Sunday March → 1st Sunday November  
**EU DST:** Last Sunday March → Last Sunday October (progressive `math.max` detection, M-001)  
**Sessions:** London 07:00-open, 16/17-close; NY 13/14-open, 21/22-close; Asian 22-07  
**Killzones:** London-KZ 07-10, NY-KZ 13-16, London-Close-KZ 15-17  
**Output:** `sessionLabel = "LONDON"/"NY"/"ASIAN"/"KZ"/"OFF"`  
**Uses:** Session-weighted bias, quality scoring, entry filters

---

## 5. Macro Engine (5 Assets)
**Lines:** 396-518  
**Purpose:** Track external market influences on XAUUSD  
**Assets:** DXY, US10Y, Silver, EURUSD, SPX  
**Method:** Each via `request.security(..., lookahead=barmerge.lookahead_off)` with `close[1]` + EMAs  
**Direction detection:** EMA crossover (fast/slow) with `barmerge.gaps_off`  
**Silver RSI:** Bundled in same request (confirmed necessary for SMT — no change needed)  
**Voting:** `macroBullVotes >= 3 → macroBull`, `macroBearVotes >= 3 → macroBear`  
**Uses:** Signal gates, bias weighting, confidence scoring

---

## 6. Correlation Engine (5 Assets × 3 Windows)
**Lines:** 522-616  
**Purpose:** Measure how reliably each macro asset correlates with gold  
**Windows:** 30, 60, 120 bars  
**Significance:** Per-window `2/sqrt(n)` thresholds (H-008): 30→0.365, 60→0.258, 120→0.183  
**Scoring:** `corrWinScore()`: r>sig→100, r>0→70, r>-sig→40, else 20  
**Health:** `corrHealthMW()` averages 3 per-window scores per asset  
**Stability:** `avgStability = mean(|corr30 - corr120|)` across all 5 assets  
**Uses:** Macro confidence, weighting macro evidence

---

## 7. Dual Structure Engine (Internal + Swing)
**Lines:** 836-867  
**Purpose:** BOS detection combining fast internal breaks with major swing breaks  
**Internal BOS:** `bodySize > ema(lows)*mult` with sequential bar comparison  
**Swing BOS:** Based on swing high/low pivots (5-bar lookback)  
**Combined:** `bosLabelBull = intBOS or swingBOS`  
**Uses:** Entry signal qualification, structure labels

---

## 8. CHOCH / MSS Detection
**Lines:** 869-896  
**Purpose:** Classify market structure shifts using HH/HL/LH/LL logic  
**CHOCH:** Prior bear sequence + higher low → Bull; prior bull sequence + lower high → Bear, with ATR distance guard  
**MSS:** CHOCH + higher high (bull) or lower low (bear)  
**Priority in display:** MSS > CHOCH > BOS  
**Uses:** Structure label in dashboard, signal confirmation

---

## 9. Displacement (ATR Hysteresis)
**Lines:** 782-807  
**Purpose:** Detect strong directional moves with state persistence  
**H-006:** 2.5× ATR enter / 1.5× ATR hold / 0.5× ATR exit  
**State vars:** `displacementUpActive`, `displacementDownActive` — persist across bars  
**Conditions:** Body > ATR×mult + close > high[1] (bull) / close < low[1] (bear) + volume > SMA×1.3 + body% > 70  
**Uses:** OB/FVG detection, signal qualification, trend bias

---

## 10. Volume Climax
**Lines:** 875-896  
**Purpose:** Identify exhaustion volume (climax)  
**Detection:** Volume > SMA(20)×1.5 + close near high/low of bar + body% > 80  
**Uses:** Reversal signals, OB confirmation

---

## 11. FVG Detection Engine
**Lines:** 897-1007  
**Purpose:** Find fair value gaps (3-candle imbalance)  
**Auto mode only:** No manual override — full algorithmic detection  
**Gap filter:** `gapSize > adaptiveATR * 0.15` (min) and `< adaptiveATR * 2.0` (max)  
**S/R clustering gated behind perfMode** (audit recommendation v3.2)  
**Uses:** Support/resistance levels, liquidity zones

---

## 12. Order Block Detection
**Lines:** 1008-1058  
**Purpose:** ICT-style OB identification (candle before displacement)  
**Quality score:** Combines range/ATR ratio, volume percentile, HTF alignment, displacement strength, climax  
**Reversal OBs:** Displacement after significant opposite move  
**Uses:** Entry zones, invalidation levels

---

## 13. S/R Clustering
**Lines:** 1059-1120  
**Purpose:** Fractal + volume-weighted price clusters  
**Fractal detection:** 5-bar fractal highs/lows  
**Clustering:** `|price - level| < price × 0.001` → merge into zone  
**Volume gating:** Uses `volumeProfileSMA` when not in performance mode  
**Uses:** Key level identification, cluster strength

---

## 14. Liquidity Levels
**Lines:** 1121-1159  
**Purpose:** Identify EQH/EQL (equal highs/lows)  
**Detection:** Within `liqProxMult × ATR` distance, at least 2 touches  
**Uses:** Destination ranking, sweep detection

---

## 15. SMT Divergence
**Lines:** 1040-1058  
**Purpose:** Detect price/RSI divergence between gold and correlated assets  
**Conditions (H-009):** Gold vs DXY direction divergence + goldRSI vs silverRSI divergence + close vs close[3] confirmation + persistence check via `[1]`  
**Uses:** Reversal signals, bias confirmation

---

## 16. Regime Engine
**Lines:** 112 and related  
**Purpose:** Classify market environment  
**Inputs:** ADX (trend strength), ATR percentile (volatility), HTF alignment  
**Outputs:** `strongTrend`, `moderateTrend`, `volRegime` (0-100)  
**Label:** `"Trending"/"Ranging"/"Dead"` + `"Volatile"` when volRegime > 70  
**Uses:** Kelly dampening, EV adjustment, regime-based bias

---

## 17. Volume Delta & Profile
**Lines:** 1062-1098  
**Purpose:** Proxy for directional volume + price-level volume distribution  
**Delta:** `close - open` sign → buying/selling pressure, cumulated over 20 bars  
**Profile:** Price-level volume concentration (100-bar lookback), gated behind `perfMode`  
**Uses:** Volume node identification, pressure divergence

---

## 18. Liquidity Pool Tracking
**Lines:** 1160-1219  
**Purpose:** Track daily/weekly/monthly highs and lows as liquidity targets  
**Source:** `request.security("D"/"W"/"M", [high[1], low[1]])` — last completed candle only  
**Pool vars:** `poolPdH/PdL/PwH/PwL/PmH/PmL` — updated each new period  
**Sweep detection:** Price touches pool level + reversal → `liqSweepBull/Bear`  
**Liquidity runs:** Consecutive pool breaks → `liqRunBull/Bear`  
**Health:** `corrBreakCount >= corrHealthBars` → `corrBreakdown`  
**Uses:** Destination ranking, sweep signals, run detection

---

## 19. Structure Persistence Score (V26 E1)
**Lines:** 1223+  
**Purpose:** Measure directional continuation after BOS/CHOCH  
**Method:** Track consecutive bars maintaining structure direction after signal  
**Decay:** `structScore := nz(structScore[1]) > 0 ? nz(structScore[1]) * 0.5 : 0.0` (L-001)  
**Uses:** Structural confidence in bias computation

---

## 20. Liquidity Destination Ranking (V27 E2)
**Lines:** 1368-1475  
**Purpose:** Rank likely price destination among known liquidity levels  
**Score components:**
- Pool weight: PDH/PDL=1.5, PWH/PWL=1.25, PMH/PML=1.0, EQH/EQL=1.0
- Distance score: linear decay from 100 at 0 ATR to 0 at `liqProxMult × ATR`
- HTF alignment: agrees→1.3, partial→1.15, neutral→1.0, opposes→0.7
- Session weight: killzone→1.3, London/NY→1.15, Asian/OFF→0.8
- Trend weight: `trendMult` (H-005) = 0.60 recentDisp / 0.40 otherwise

**Output:** `liqDestLabel = "PDH"/"PDL"/"PWH"/"PWL"/"PMH"/"PML"/"EQH"/"EQL"/"—"`, `liqDestProb = 0-100`  
**Uses:** Dashboard liquidity field, bias weighting, sweep probability

---

## 21. MTF Confluence Engine (V26 E3)
**Lines:** 1508+  
**Purpose:** Measure alignment across all 5 timeframes  
**Calculation:** `confMTF = max(htfFullLong, htfFullShort) × 100 / 5` (FA-004)  
**Range:** 0 (conflict) to 100 (all 5 aligned)  
**Uses:** Confidence score component, bias confirmation

---

## 22. Macro Intelligence Engine (V26 E4)
**Lines:** ~1550+  
**Purpose:** Combine macro asset signals into directional confidence  
**Calculation:** `macroIntelConf` = weighted combination of DXY/yield/silver/EUR/SPX directions  
**Components:** Each asset contributes up to 20 points based on trend strength  
**Uses:** Confidence score, macro gate for signals

---

## 23. Bias Scores (V27 E5)
**Lines:** 1508-1537  
**Purpose:** Ordinal evidence-weighted directional bias (NOT probabilities)  
**Components:** Structure(20) + MTF(20) + Liquidity(20) + Macro(20) + Session(15) + MR(5)  
**Voting:** Each component contributes to `bullBiasScore`/`bearBiasScore`  
**Output:** Dominant direction + magnitude (0-100)  
**Uses:** Directional label, trade signal qualification

---

## 24. Confidence Score (V27 E6)
**Lines:** 1556-1568  
**Purpose:** Overall confidence in current setup (0-100)  
**Components (equal weight 20% each):** Structure, MTF, Liquidity, Macro, Session  
**Calculation:** `confidenceScore = weighted average / 5`  
**Thresholds:** ≥60 HIGH, ≥40 MED, <40 LOW  
**Setup score:** `confidenceScore / 20` → 0-5 integer  
**Uses:** Dashboard confidence field, label intensity

---

## 25. Signal Quality Engine (V27 E7)
**Lines:** 1568-1593  
**Purpose:** Gate trade signals based on multi-factor quality  
**Inputs:** Trend alignment + macro agreement + regime confidence + session quality + BOS + RSI divergence + recent bars  
**Prefilters:** `buyPreFilters` and `sellPreFilters` (line 1908-1911) — all must pass  
**Uses:** Entry signal generation

---

## 26. Probability Engine (7-factor live)
**Lines:** 1593-1665  
**Purpose:** Compute bull/bear/range probabilities from 7 evidence factors  
**Factors:** Structure strength, confidence, MTF alignment, MR extension, macro, session, liquidity destination  
**Scoring:** Each factor contributes raw points, then normalized to sum 100%  
**Updates:** On each bar with exponential weighting; forced resets on conflicting signals  
**Uses:** Dashboard bias display, signal direction

---

## 27. Outcome Engine (Historical Stats)
**Lines:** 2060-2533  
**Purpose:** Track actual outcomes of past matched setups for calibration  
**Match criteria:** 7-factor encoding (adx, atr, htf, trend, liquidity, mr, corr)  
**Statistics tracked:** Win rate, avg win/loss, profit factor, Sharpe, max DD, EV, Kelly %, calibration  
**Bucketing:** Matches grouped into 5 probability bins for ECE computation  
**Calibration (line 2434-2438):** `oCalPct = 100 - avg_error × 2`, `oCalEce = total_error / total_count`  
**Blending (line 2578-2587):** Live 70% + Historical 30% (`blendHist = 0.30`) when `oMatch >= 3` and `barstate.islast`  
**Minimum bucket:** 30 matches per bin for statistical significance  
**Uses:** Dashboard EV/Kelly/DD/WR, calibration display, forecast blending

---

## 28. Forecast Engine (Module 12)
**Lines:** 2589-2695  
**Purpose:** Multi-factor forward projection combining 4 independent forecasts  
**Weights:** Historical Analog 30% / Trend Persistence 25% / Liquidity Destination 25% / Regime Persistence 20%  
**Output:** `fBull`, `fBear`, `fRng` — forecast probabilities  
**Confidence cone:** Variance across 4 forecasts → cone width (50 CI = 1/3 of range)  
**Uses:** Price projection display, regime-aware bias

---

## 29. Dashboard Rendering
**Lines:** 2794-3028  
**Purpose:** Render all data in a responsive table  
**Shared data:** Lines 2796-2880 — 30+ computed display strings  
**Responsive modes:**  
- Desktop: 9 cols × 3 rows (header + values + sub-values)  
- Tablet: 9 cols × 2 rows (compact)  
- MobileLand: 6 cols × 3 rows (medium)  
- MobilePort: 4 cols × 5 rows (stacked)  
**Text sizing:** Auto→MobilePort="tiny", others="small"; sub-values always "tiny" in Auto  
**Density:** Compact/Standard/Spacious adjust column widths  
**Table lifecycle:** Created on `barstate.islast`, deleted and recreated on mode change  
**Indicator visibility:** OB, FVG, pivot labels hidden on MobileLand and MobilePort via `_dashModeResp`
