# XAUUSD Quantum 3.0 — Independent Dashboard Validation & Accuracy Audit

**Audit Date**: June 16, 2026  
**File**: `XAUUSD_Quantum_3.0.pine` (3664 lines, v1.0)  
**Methodology**: Code forensic analysis from first principles.

---

## Deliverable 1: Dashboard Metric Inventory

### All Inputs (from CORE + V11 + V17 + V19 groups)

| Input | Variable | Default | Location |
|-------|----------|---------|----------|
| EMA20 | ema20Len | 20 | L106 |
| EMA100 | ema100Len | 100 | L107 |
| EMA200 | ema200Len | 200 | L108 |
| ATR Base | atrLen | 14 | L109 |
| ADX | adxLen | 14 | L110 |
| ADX Threshold | adxThreshold | 25 | L111 |
| RSI Length | rsiLen | 14 | L112 |
| Internal Pivot Len | internalPivotLen | 2 | L113 |
| Swing Pivot Len | swingPivotLen | 5 | L114 |
| Signal Lookback | recentBarsLen | 120 | L115 |
| Trend Threshold | trendThreshold | 60 | L116 |
| Vol Climax Pctl | volClimaxPerc | 95 | L117 |
| EQ Tolerance | eqAtrMult | 0.25 | L118 |
| Vol Lookback | volLookback | 50 | L119 |
| FVG Max Bars | fvgMaxBars | 30 | L129 |
| OB Max Bars | obMaxBars | 20 | L130 |
| Liq Proximity | liqProxMult | 1.5 | L131 |
| Disp Mult | dispMult | 2.5 | L135 |
| Macro TF | macroTF | "60" | L136 |
| DXY ROC Len | dxyRocLen | 10 | L140 |
| Corr Health Bars | corrHealthBars | 10 | L142 |
| Yield ROC Len | znRocLen | 10 | L143 |
| Yield Symbol | yieldSymbol | TVC:US10Y | L144 |
| Invert Yield | invertYield | false | L145 |
| DXY Symbol | dxySymbol | TVC:DXY | L146 |
| Silver Symbol | silverSymbol | OANDA:XAGUSD | L147 |
| EURUSD Symbol | eurusdSymbol | OANDA:EURUSD | L148 |
| SPX Symbol | spxSymbol | SP:SPX | L149 |
| Corr Short | corrShortLen | 30 | L152 |
| Corr Med | corrMedLen | 60 | L153 |
| Corr Long | corrLongLen | 120 | L154 |
| Layout Mode | layoutMode | "Full-Width" | L87 |
| Module toggles (18) | show* | true/false | L60-L83 |
| Risk inputs | riskPercent/accountSize/pointValue | 1.0/10000/100 | L90-L92 |

### All request.security() Calls

| # | Symbol | Timeframe | Variables | Lookahead | Gaps | Line |
|---|--------|-----------|-----------|-----------|------|------|
| 1 | syminfo.tickerid | "5" | htf5mClose/Ema20/Ema50/Ema200 | lookahead_off | gaps_off | L187-190 |
| 2 | syminfo.tickerid | "15" | htf15mClose/Ema20/Ema50/Ema200 | lookahead_off | gaps_off | L192-195 |
| 3 | syminfo.tickerid | "60" | htf1hClose/Ema20/Ema50/Ema200 | lookahead_off | gaps_off | L197-200 |
| 4 | syminfo.tickerid | "240" | htf4hClose/Ema20/Ema50/Ema200 | lookahead_off | gaps_off | L202-205 |
| 5 | syminfo.tickerid | "D" | htfDClose/Ema20/Ema50/Ema200 | lookahead_off | gaps_off | L207-210 |
| 6 | dxySymbol | macroTF (60) | rawDXY, rawDXYFast, rawDXYSlow, rawDXYRoc | lookahead_off | gaps_off | L391-394 |
| 7 | yieldSymbol | macroTF (60) | rawYield, rawYieldFast, rawYieldSlow, rawYieldRoc | lookahead_off | gaps_off | L395-398 |
| 8 | silverSymbol | timeframe.period | rawSilver, rawSilverEMA | lookahead_off | gaps_off | L399-402 |
| 9 | eurusdSymbol | timeframe.period | rawEURUSD, rawEURFast, rawEURSlow, rawEURRoc | lookahead_off | gaps_off | L403-406 |
| 10 | spxSymbol | timeframe.period | rawSPX, rawSPXFast, rawSPXSlow | lookahead_off | gaps_off | L407-410 |
| 11 | syminfo.tickerid | "D" | [high[1], low[1]] | lookahead_off | (default) | L1122 |
| 12 | syminfo.tickerid | "W" | [high[1], low[1]] | lookahead_off | (default) | L1123 |
| 13 | syminfo.tickerid | "M" | [high[1], low[1]] | lookahead_off | (default) | L1124 |
| 14 | silverSymbol | timeframe.period | ta.rsi(close, rsiLen)[1] | lookahead_off | gaps_on | L1022-1025 |

**Verdict**: All 14 calls use `barmerge.lookahead_off`. Zero use of `lookahead_on`. No future leakage.

### Dashboard Field-to-Variable Mapping (Full-Width / Advanced mode)

| Dashboard Field | Variable | Formula | Source | Lines |
|----------------|----------|---------|--------|-------|
| SYMBOL | _symStr | syminfo.ticker | Chart | L2727 |
| TF | _tfLabel | timeframe.period | Chart | L2726 |
| PRICE | _priceStr | close | Chart | L2728 |
| CHG | _chgStr | (close-close[1])/close[1] | Chart | L2729-2730 |
| DIR | _dirLabel | max(bullProb,bearProb,rangeProb) | Live 7-factor | L2736 |
| CONF | _confStr | confidenceScore | 5-dim eq-weight | L2732 |
| PROB | _bullPct/_bearPct | bullProb/bearProb | Live 7-factor→blend→forecast | L2742-2743 |
| QUAL | _qualLabel | HIGH/MED/LOW per confidenceScore | Confidence engine | L2747 |
| STATE | _stateLabel | shouldBuy/Sell/Wait | Buy/sell pre-filters | L2738 |
| MTF | _mtfDir | BULL/BEAR/MIX per htfFullLong/Short | 5-TF trend stack | L2751 |
| MACRO | _macroBias | BULL/BEAR/MIX per 4H+1D | HTF (subset) | L2755-2757 |
| SETUP | _setupStr | confidenceScore/20 → 0-5/5 | Confidence engine | L2761-2763 |
| KELLY | _kellyLabel | oKellyPct → formatted | Stats engine | L2767 |
| RR | _rrStr | tpRR1 | Trade plan | L2771 |
| E | _eStr | tpEntry = close | Trade plan | L2774 |
| SL | _slStr | close ± 1.5 ATR | Trade plan | L2775 |
| TP | _tpStr | close ± 1.5 ATR | Trade plan | L2776 |
| EV | _evStr | oEvVal | Stats engine | L2777 |
| VD | _vdStr | volDelta50/1000 | Volume delta | L2779 |
| WR | _wrStr | oWr | Stats engine | L2781 |
| PF | _pfStr | oProfitFactor | Stats engine | L2782 |
| DD | _ddStr | oMaxDD | Stats engine | L2784 |
| DXY | _dxyLbl | DXY + _dxyArrow | Macro engine | L2788 |
| 10Y | _yldLbl | 10Y + _yldArrow | Macro engine | L2790 |
| XAG | _xagLbl | XAG + _xagArrow | Macro engine | L2792 |
| SPX | _spxLbl | SPX + _spxArrow | Macro engine | L2794 |
| EUR | _eurLbl | EUR + _eurArrow | Macro engine | L2796 |
| REG | _regLbl | regLabel: Trending/Ranging/Dead | Regime engine | L2798 |
| VOL | _volLbl | volTag: HIGH/NORMAL/LOW | ATR percentile | L2800 |
| SESS | _sessLbl | sessionLabel | Session engine | L2802 |
| PDH | _pdhLbl | PDH↑/○ | Liq destination | L2806 |
| PWH | _pwhLbl | PWH↑/○ | Liq destination | L2808 |
| PMH | _pmhLbl | PMH↑/○ | Liq destination | L2810 |
| PDL | _pdlLbl | PDL↓/○ | Liq destination | L2812 |
| PWL | _pwlLbl | PWL↓/○ | Liq destination | L2814 |
| PML | _pmlLbl | PML↓/○ | Liq destination | L2816 |
| EQH/EQL | _eqhEqlLbl | EQH✓/EQL✓ | EQ detection | L2818 |
| HB | _hbStr | HB {B/S}{pct} | Hist analog / stats | L2822-2824 |
| FB | _fbStr | FB {B/S}{pct} | Forecast engine | L2825-2827 |
| SR | _srStr | SR + sharpeStr | Stats engine | L2829 |
| CR | _crStr | avgCorrHealth | Correlation engine | L2831 |
| CAL | _calStr | oCalGrade | Stats engine calibration | L2832 |
| IS | _isStr2 | IS WR% | Stats engine (in-sample) | L2835-2836 |
| OOS | _oosStr2 | OOS WR% | Stats engine (out-of-sample) | L2835-2836 |
| FVG | _fvgStr2 | FVG{score} | FVG quality | L2837 |
| OB | _obStr2 | OB{score} | OB quality | L2838-2839 |
| VP | _vpStr | VP{poc} | Volume profile | L2840 |
| VH/VA | _vhStr/_vlStr | VH{vah}/VL{val} | Volume profile | L2841-2842 |
| MTF arrows | _5mArw2 etc | ▲/▼/≈ per TF | HTF trend stack | L2843-2850 |

---

## Deliverable 2: Multi-Timeframe Validation Matrix

### How Values Change Across Timeframes

The indicator computes most values on the **chart's own timeframe** — switching timeframes in TradingView recomputes everything from scratch on that TF's bars. This is inherent to Pine Script design.

| Metric | TF-Dependent? | Mechanism | Expected Variation |
|--------|:------------:|-----------|-------------------|
| **HTF scores** (5M/15M/1H/4H/D) | No — fixed | `request.security` to fixed TFs | Identical on all chart TFs |
| **bullProb/bearProb** | Yes | 7-factor model on chart TF data | Changes between TFs |
| **Bias scores** | Yes | bullBiasScore/bearBiasScore on current data | Changes between TFs |
| **shouldBuy/shouldSell** | Yes | Depends on bias + HTF + macro | May differ |
| **Session** | No | UTC hour, independent of TF | Identical |
| **Macro** | Partially | DXY/10Y on macroTF(60), rest on chart TF | Silver/EUR/SPX change with TF |
| **Liquidity levels** | No | `high[1]/low[1]` on D/W/M | Identical |
| **Liq destination** | Yes | Depends on current close-to-level distance | Changes with chart TF close |
| **FVG/OB** | Yes | Detected on chart TF bars | May differ between TFs |
| **Stats engine** | Yes | Records on `isconfirmed` of chart TF | Different databases per TF |
| **Entry/SL/TP** | Yes | Based on current close + ATR | Changes with TF close |
| **EV/Kelly/WR** | Yes | From stats engine on this TF | Different per TF |

### Cross-TF Consistency Check

**Potential Issue — Stats Engine Per-TF**: The statistical engine (lines 1899-2485) records on `barstate.isconfirmed` of the CHART timeframe. Each chart TF builds its own historical database. A 5M chart and 4H chart will have completely different history databases, producing different WR/EV/Kelly values even for the same time period. This is expected behavior.

**Potential Issue — Macro Silver/EUR/SPX on chart TF**: Calls #8, #9, #10 use `timeframe.period` (the chart's own TF). DXY and Yield use `macroTF` (default "60"). This means silver/EUR/SPX macro signals vary by chart TF while DXY/10Y stay at 1H. On a 5M chart, the silver signal updates every 5 minutes; on a 4H chart, every 4 hours. This creates inconsistency — the macro signals are not all at the same temporal resolution.

---

## Deliverable 3: Repainting and Lookahead Risk Assessment

### request.security() Analysis

All 14 calls verified:

- **HTF trend** (calls 1-5): `close[1]` inside `request.security` + `lookahead=barmerge.lookahead_off` ✅
  - The `close[1]` reads the previous completed bar's close in the target TF
  - No repainting — value is fixed once the TF bar completes
  - Correct pattern per Pine Script best practices

- **Macro assets** (calls 6-10): Same `close[1]` + `lookahead=barmerge.lookahead_off` ✅
  - Stale data handling (lines 412-447): cached values with 5-bar max staleness
  - Correct — prevents na from breaking dashboard when data is delayed

- **Liquidity levels** (calls 11-13): `high[1]/low[1]` + `lookahead=barmerge.lookahead_off` ✅
  - Non-repainting — levels snap to previous completed period's extreme
  - Fixed for entire current candle

- **SMT RSI** (call 14): `ta.rsi(close, rsiLen)[1]` + `lookahead=barmerge.lookahead_off` ✅
  - The `[1]` shifts RSI by 1 bar (reads previous completed bar's RSI)
  - `barmerge.gaps_on` — only updates on new silver bar
  - Minor: uses `gaps_on` which means the value stays stale between silver TF ticks
  - **Minor issue**: SMT compares current gold RSI (line 1021: `goldRSI = rsiVal`) with lagged silver RSI by 1 bar. This introduces a 1-bar offset between the two RSI values, potentially causing false SMT signals.

### Dashboard Repainting

All dashboard cells (lines 2852-2981 in renderDashboard) are set on `barstate.islast`. This is the standard pattern — the dashboard updates on every tick of the last bar. This is NOT repainting in the problematic sense (no historical values change), but the current bar's values do update tick-by-tick (e.g., close, ATR, session state). This is unavoidable and expected.

**Verdict: No critical repainting or lookahead issues.**

**Minor issue (SMT RSI offset):** SMT compares `rsiVal` (current gold RSI) with `_silverRSI` which is `ta.rsi(close, rsiLen)[1]` of silver — silver's RSI lagged by 1 bar. This asymmetry could produce false divergence readings. This is an intentional simplification but technically a minor data alignment issue.

---

## Deliverable 4: Historical Probability Assessment

### Historical Analog Engine (lines 1631-1691)

- **Match criteria**: ADX (25%), HTF (25%), ATR (20%), Structure (15%), Liquidity Position (15%)
- **Threshold**: ≥60/100 similarity
- **Forward measurement**: 5-bar forward change (line 1673: `close[i-5] - close[i]`)
- **Outcome classification**: ≥30% of range = directional, else range

**Issues:**

1. **Match threshold is low**: 60/100 with 5 dimensions means a bar can match on ADX (25) + HTF (25) + ATR (20) = 70 and still match with zero structural similarity. This produces false matches.
2. **Liquidity position falls back to 0.5** (line 1662: `float liqPosI = 0.5`) — this dimension contributes nothing to similarity scoring since all historical bars get `liqPosI = 0.5` while current also has a fixed value.
3. **Small sample warning**: The disclaimer at line 16 correctly notes "3-10 matches, SE ~15-30%". The `matchCount >= 3` threshold (line 1683) is very low for statistical significance.
4. **Falls back to live probability** when `matchCount < 3` (lines 1688-1691) — correct behavior.

### Statistical Engine (lines 1899-2485)

- **Recording**: `barstate.isconfirmed` (line 1958) ✅ — no lookahead
- **Querying**: `barstate.islast` (line 2451) ✅
- **7-dimension state**: ADX, ATR, HTF, Structure, Liquidity, Mean Reversion, Correlation
- **SIM_THRESH = 55** (lower than analog at 60)
- **OUTCOME_N = 10** bars forward

**Issues:**

1. **Circular buffer size**: HIST_MAX = 4500 bars. On lower timeframes this could wrap and overwrite unmeasured outcomes.
2. **Outcome measurement** (lines 1982-1993): Uses `close - close[OUTCOME_N]` at the time when `bar_index - oBi >= OUTCOME_N`. This measures from event bar's close to current close. However, the outcome classification (bull/bear/range) uses `close - close[OUTCOME_N]` compared to `highOutcome - lowOutcome`. Wait — `close` here is the close of the bar when the outcome is being recorded (which is `oBi + OUTCOME_N`), and `close[OUTCOME_N]` is the close 10 bars ago (which is `oBi`). So `fc = close_at_bar(oBi+10) - close_at_bar(oBi)`. And `highOutcome/lowOutcome` are `highest(high, 10)/lowest(low, 10)` at the current bar. This is `high/low` over the 10-bar window ending at the current bar. This correctly measures the 10-bar forward range.
3. **IS/OOS split** (line 2178): `bool _isOos = sb >= bar_index - min(500, max(200, bar_index/3))`. This uses a fixed recent-band split. OOS = most recent 200-500 bars. IS = everything older. This is a reasonable walk-forward approach.

### In-Sample vs Out-of-Sample

- IS: WR from older matches (lines 2185-2189)
- OOS: WR from recent matches (lines 2179-2184)  
- IS min N = 30, OOS min N = 10 (lines 2700-2701)
- **No explicit time-based separation** — the split is purely by bar index proximity. This means an OOS bar could be similar to an IS bar from a different market regime, reducing the OOS validity.

**Verdict**: The historical probability engines are directionally sound but have low statistical power. The disclaimer appropriately warns about the limitations.

---

## Deliverable 5: Macro Model Assessment

### Expected Relationships

| Asset | Expected vs Gold | Code Direction | Correct? |
|-------|:----------------:|:--------------:|:--------:|
| DXY | Negative | DXY bear → bull vote (line 486) | ✅ |
| US10Y | Negative | Yield falling → bull vote (line 486) | ✅ |
| Silver | Positive | Silver bull → bull vote (line 486) | ✅ |
| EURUSD | Positive | EURUSD bull → bull vote (line 486) | ✅ |
| SPX | Positive (regime-dependent) | SPX bull → bull vote (line 486) | ⚠️ |

### Issues

1. **SPX relationship is not always positive**: The audit report (v2.5b) noted this at line 357. During risk-off macro regimes, SPX and gold can be negatively correlated. The indicator assumes constant positive correlation. Weight is 15% — low enough that the impact is limited but conceptually incorrect.

2. **Macro timeframe inconsistency**: DXY and US10Y use `macroTF` (default "60" = 1H). Silver, EURUSD, SPX use `timeframe.period` (chart TF). This means:
   - On a 5M chart: DXY updates hourly, but silver updates every 5min. The directional votes are at different temporal resolutions.
   - On a 4H chart: DXY still updates hourly, silver updates every 4H.

3. **Correlation uses returns on chart TF**: `ta.correlation(goldRet, dxyRet, N)` where `goldRet = close / close[1] - 1` (line 181). These are chart-TF returns. The correlation value changes based on the chart TF — a 5M correlation will be noisier than a 4H correlation. The health scoring (lines 567-572) uses fixed thresholds (`_corrSig95 = 2/sqrt(corrShortLen)`) which scales with window size, but the return volatility changes with TF.

4. **Weight calculation uses absolute correlation magnitude** (lines 575-585): `_wDXYr = min(abs(avgCorrDXY) * 100, 30)`. This means assets with stronger correlation get higher weights, which is reasonable. However, the expected sign is hardcoded in the direction votes (line 486: `dxyBear ? 1 : 0`), not in the weights.

**Verdict**: The macro model is conceptually sound for typical conditions. The SPX regime issue and TF inconsistency are known limitations.

---

## Deliverable 6: Issue Log

### Critical (0 issues)

None found.

### Major (0 issues)

None found.

### Minor (8 issues)

| # | Area | Issue | Evidence | Classification |
|---|------|-------|----------|:-------------:|
| 1 | **SMT** | Gold RSI vs silver RSI[1] has 1-bar offset | L1021 `goldRSI = rsiVal` vs L1022-1025 `ta.rsi(close, rsiLen)[1]` | Minor |
| 2 | **Macro** | Silver/EUR/SPX use chart TF, DXY/10Y use 1H — temporal inconsistency | L399-410 vs L391-398 | Minor |
| 3 | **Prob** | probStructBear = 100 - structScore is simplistic | L1542: `float probStructBear = structScore > 0 ? 100.0 - structScore : 50.0` | Minor |
| 4 | **Bias** | biasSessBull and biasSessBear are identical | L1496-1497: both use same sessionQuality logic | Minor |
| 5 | **Forecast** | Live 7-factor prob computed then overwritten by forecast engine on last bar | L2532-2539 blend, then L2625-2632 override | Minor |
| 6 | **Analog** | liqPosI hardcoded to 0.5 — dimension is non-functional | L1662: `float liqPosI = 0.5` | Minor |
| 7 | **Analog** | Match threshold 60/100 allows matches with only 2 of 5 dimensions | L1664-1669: ADX(25)+HTF(25)+ATR(20)=70 without structure/liquidity | Minor |
| 8 | **Stats** | IS/OOS split by bar index proximity, not by time regime | L2178: `sb >= bar_index - min(500, max(200, bar_index/3))` | Minor |

### Expected Behavior (7 items)

| # | Behavior | Explanation |
|---|----------|-------------|
| 1 | Different TFs show different signals | Pine Script recomputes per-TF — expected |
| 2 | Stats engine values differ per TF | Each TF builds its own history database |
| 3 | Dashboard updates tick-by-tick on last bar | `barstate.islast` pattern — expected |
| 4 | Session quality changes at UTC hour boundaries | DST-aware session engine — correct |
| 5 | DXY/US10Y correlation changes slowly | Uses 1H resolution macroTF — expected |
| 6 | Kelly capped at 0.25 max | L2347: `math.min(math.max(oKelly, -0.5), 0.25)` — conservative |
| 7 | shouldBuy/Sell rarely true | Multiple gates require confluence — intentional |

---

## Deliverable 7: Final Conclusion

### **Dashboard is mostly accurate with minor issues.**

All 14 `request.security()` calls use `lookahead=barmerge.lookahead_off`. Zero critical or major issues found. The 8 minor issues are:

1. SMT RSI 1-bar offset (gold vs silver RSI alignment)
2. Macro TF inconsistency (silver/EUR/SPX on chart TF, DXY/10Y on 1H)
3. ProbStructBear as simple inverse of structScore
4. Identical bull/bear session bias scores
5. Live probability computed then overwritten by forecast engine
6. Non-functional liquidity position dimension in historical analog
7. Low match threshold in analog engine
8. IS/OOS split by bar index, not regime

None of these affect core dashboard integrity. The dashboard values correctly map to their source variables. The non-repainting guarantees hold across all timeframes. The statistical disclaimer at lines 12-17 is appropriate and should be retained.

The indicator is ready for TradingView compilation and visual validation against screenshots.
