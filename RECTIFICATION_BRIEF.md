# XAUUSD Quantum 3.3 — Technical Rectification Brief

## Overview

This document specifies mandatory fixes for `XAUUSD_Quantum_3.3.pine` (4095 lines, Pine Script v6) based on an expert audit. Fixes are organized by priority. **P0 items are non-negotiable.** P1/P2 items address functional bugs and performance risks.

---

## PHASE 1 — P0: Semantic & Safety Fixes (Immediate)

### Fix 1.1: Global Rename "Prob" → "Score"

Every variable, label, and comment containing `Prob` or `probability` (referring to the 7-factor engine output) must be renamed to `Score` or `evidence`. These are ordinal evidence-weighted composites, not calibrated probabilities.

| Current Name | New Name | Location |
|---|---|---|
| `bullProb` | `bullScore` | 7-factor engine, forecast engine, dashboard |
| `bearProb` | `bearScore` | same |
| `rangeProb` | `rangeScore` | same |
| `rawBullProb` | `rawBullScore` | lines ~1657–1660 |
| `rawBearProb` | `rawBearScore` | same |
| `totalRaw` | `totalRawScore` | same |
| `histBullRate` | `histBullScore` | lines ~1703–1770 |
| `histBearRate` | `histBearScore` | same |
| `histRangeRate` | `histRangeScore` | same |
| `fBull` | `fBullScore` | forecast engine |
| `fBear` | `fBearScore` | forecast engine |
| `fRng` | `fRngScore` | forecast engine |
| `pdhReachProb` | `pdhReachScore` | scenario analysis |
| `pdlReachProb` | `pdlReachScore` | scenario analysis |
| `liqReachProb` | `liqReachScore` | liquidity destination |
| `probTrendBull` | `evTrendBull` | 7-factor evidence inputs |
| `probStructBull` | `evStructBull` | same |
| `probFlowBull` | `evFlowBull` | same |
| `probMacroBull` | `evMacroBull` | same |
| `probLiqBull` | `evLiqBull` | same |
| `probSessionBull` | `evSessionBull` | same |
| `probCorrBull` | `evCorrBull` | same |
| `probMrBull` | `evMrBull` | same |
| `probLabel` | `evidenceLabel` | display |
| `probColor` | `evidenceColor` | display |

Dashboard labels:
- Old: `"Bull X / Bear Y / Range Z (score)"` → New: `"Bull X / Bear Y / Range Z (evidence)"`

**Use whole-word find-and-replace. Verify no "probably" or "problem" is caught.**

---

### Fix 1.2: Remove All Kelly Criterion Code

Delete entirely from `runStatsEngines()`:

```
float oKelly = 0.0
if mt >= 30 and avgL > 0 and avgW > 0
    float rRatio = avgW / avgL
    float kellyFrac = mt >= 300 ? 0.75 : mt >= 100 ? 0.5 : 0.25
    oKelly := (wr - ((1.0 - wr) / rRatio)) * kellyFrac * 0.70
    oKelly := math.min(math.max(oKelly, -0.25), 0.25)
oKellyPct_ := math.round(oKelly * 100.0)
```

Remove:
- `oKellyPct` / `__kp` from the return tuple and destructuring
- `var float oKellyPct = na`
- `string kellyStr = "—"` and `color kellyCol` from dashboard
- All `table.cell()` calls referencing `kellyStr`

Rationale: Kelly requires calibrated probabilities and known payoff distributions. Neither exists here.

---

### Fix 1.3: Update Disclaimer Header

Replace lines 20–25 with:

```
// DISCLAIMER: All "Score" values are ordinal evidence-weighted composites,
// not statistically validated probabilities. Use for directional bias only,
// not for position sizing. Historical analog is typically underpowered
// (30-50+ matches, SE ~8-14%). Past outcomes do not guarantee future results.
```

---

## PHASE 2 — P1: Functional Bugs (Short-term)

### Fix 2.1: Adaptive ATR Smooth Transition

Replace lines 265–279 (hysteresis switch) with a continuous blend:

```pinescript
// ATR Regime with smooth transition
float atrTrending = ta.atr(7)
float atrRanging  = ta.atr(21)

// Regime strength: 0 = full ranging, 1 = full trending
// ADX <= 20 → 0.0  |  ADX >= 30 → 1.0  |  20-30 → linear blend
float regimeStrength = math.min(math.max((adxVal - (adxThreshold - 5)) / 10.0, 0.0), 1.0)

_rawAdaptiveATR = showAdaptiveATR ? (atrRanging * (1.0 - regimeStrength) + atrTrending * regimeStrength) : atr
adaptiveATR     = math.max(nz(_rawAdaptiveATR, ta.tr), close * 0.0001)

// Hysteresis flag retained only for display
var bool _atrInTrendingRegime = false
_atrInTrendingRegime := regimeStrength > 0.5
```

---

### Fix 2.2: SMT Divergence Logic

Replace lines 1091–1104. Current code incorrectly uses DXY direction (normal inverse correlation) instead of comparing gold vs silver RSI divergence.

New logic:

```pinescript
// Pivot-based SMT: Gold vs Silver (correlated pair)
var float goldRsiAtLastLow   = na
var float silverRsiAtLastLow = na
var float goldRsiAtLastHigh   = na
var float silverRsiAtLastHigh = na

if not na(pivotLow)
    goldRsiAtLastLow   := rsiVal
    silverRsiAtLastLow := _silverRSI
if not na(pivotHigh)
    goldRsiAtLastHigh  := rsiVal
    silverRsiAtLastHigh := _silverRSI

// Bull SMT: gold price lower low, gold RSI > prior gold RSI AND > silver RSI
bool smtBullRaw = smtDataValid and not na(pivotLow) and not na(pivotLow[1])
 and pivotLow < pivotLow[1]
 and goldRsiAtLastLow > goldRsiAtLastLow[1]
 and goldRsiAtLastLow > silverRsiAtLastLow

// Bear SMT: gold price higher high, gold RSI < prior gold RSI AND < silver RSI
bool smtBearRaw = smtDataValid and not na(pivotHigh) and not na(pivotHigh[1])
 and pivotHigh > pivotHigh[1]
 and goldRsiAtLastHigh < goldRsiAtLastHigh[1]
 and goldRsiAtLastHigh < silverRsiAtLastHigh

smtBullPair = smtBullRaw and (goldBarDir[1] < 0 or rsiVal[1] > _silverRSI[1])
smtBearPair = smtBearRaw and (goldBarDir[1] > 0 or rsiVal[1] < _silverRSI[1])
```

---

### Fix 2.3: Label Array Hard Caps

For ALL 8 label arrays (bos, choch, mss, disp, eqh, eql, manip, smt), replace the push pattern with FIFO-cap enforcement:

```pinescript
// Before push, enforce hard FIFO cap (example: cap=3)
if array.size(bosLabels) >= 3
    label.delete(array.get(bosLabels, 0))
    array.remove(bosLabels, 0)
    array.remove(bosLabelBars, 0)
array.push(bosLabels, nl)
array.push(bosLabelBars, bar_index)
```

Caps: BOS/CHOCH/MSS/DISP/SMT=3, EQH/EQL=5, MANIP=2.

Keep `cleanLabels()` for age-based cleanup. Hard cap prevents burst accumulation.

---

## PHASE 3 — P2: Methodological Improvements (Medium-term)

### Fix 3.1: Order Block Failed Displacement

Replace reversal OB detection (lines ~1000–1022) with:

```pinescript
// Reversal OB: failed bullish displacement
// Current bar negates prior displacement bar's body
bool failedBullDisp = displacementUp[1] and not displacementUp
 and close < open[1] and close < (high[1] + low[1]) / 2

if showOB and not obBearActive and failedBullDisp
    obBearHigh    := high[1]
    obBearLow     := low[1]
    obBearBar     := bar_index
    obBearVolPct  := volPercentile[1]
    obBearAtr     := adaptiveATR[1]
    obBearActive  := true

// Reversal OB: failed bearish displacement
bool failedBearDisp = displacementDown[1] and not displacementDown
 and close > open[1] and close > (high[1] + low[1]) / 2

if showOB and not obBullActive and failedBearDisp
    obBullHigh    := high[1]
    obBullLow     := low[1]
    obBullBar     := bar_index
    obBullVolPct  := volPercentile[1]
    obBullAtr     := adaptiveATR[1]
    obBullActive  := true
```

---

### Fix 3.2: Volume Climax — Session-Normalized

Replace lines 894–900. Raw percentile volume triggers false signals at session opens.

```pinescript
// Relative Volume: current vs 20-day same-hour average
var float[] volByHour = array.new_float(24, 0.0)
var int[] volHourCount = array.new_int(24, 0)

int currHour = hour(time, "UTC")
if barstate.isconfirmed
    float prevAvg = array.get(volByHour, currHour)
    int prevCount = array.get(volHourCount, currHour)
    float newAvg = (prevAvg * prevCount + volume) / (prevCount + 1)
    array.set(volByHour, currHour, newAvg)
    array.set(volHourCount, currHour, prevCount + 1)

float relVol = volume / math.max(array.get(volByHour, currHour), 1.0)
float relVolPercentile = ta.percentrank(relVol, volLookback)

climaxUp   = showClimax and recentBars and relVolPercentile >= volClimaxPerc and close > open
climaxDown = showClimax and recentBars and relVolPercentile >= volClimaxPerc and close < open
```

---

### Fix 3.3: Timeframe-Adaptive HIST_MAX

Replace line 1984:

```pinescript
int tfSec = timeframe.in_seconds()
int HIST_MAX = tfSec <= 60 ? 1500 : tfSec <= 300 ? 2500 : tfSec <= 900 ? 3500 : 4500
```

Prevents timeout on lower timeframes.

---

### Fix 3.4: Correlation Breakdown — Weighted Measure

Replace lines 532–539. Current consecutive-bar counter ignores move magnitudes.

```pinescript
float goldMove = math.abs(close - close[1])
float dxyMove  = dxyValid ? math.abs(dxyClose - dxyClose[1]) : 0.0
float minMove  = adaptiveATR * 0.1

float corrBreakStrength = 0.0
if goldBarDir == dxyBarDir_raw and goldBarDir != 0
 and goldMove > minMove and dxyMove > minMove * 0.5
    corrBreakStrength := (goldMove / adaptiveATR) * (dxyMove / math.max(dxyEMAFast, 1) * 1000)

var float corrBreakCum = 0.0
corrBreakCum := corrBreakCum * 0.7 + corrBreakStrength
corrBreakdown = showCorrHealth and corrBreakCum >= corrHealthBars * 2.0
```

---

### Fix 3.5: Table Init — Prevent First-Bar Flicker

Fix `_prevDashMode` default:

```pinescript
var string _prevDashMode = "Desktop"   // was ""
```

---

## PHASE 4 — Validation Checklist

| # | Test | How | Pass Criteria |
|---|---|---|---|
| 1 | ATR smoothness | Plot `regimeStrength` + `adaptiveATR` during ADX 20→30 crossing | No visible jumps; 5-bar smooth transition |
| 2 | SMT signals | Compare gold vs silver charts during SMT alert | Gold/silver show actual RSI divergence at pivots |
| 3 | Label caps | Run on 1-min chart 1 hour; check `array.size()` | No array exceeds its hard cap |
| 4 | OB reversal | Manually verify 10 reversal OBs | Prior displacement bar is fully negated (close beyond midpoint) |
| 5 | Volume climax | Compare signals at 08:00 vs 14:00 UTC | Fewer false signals at session opens |
| 6 | Table stability | Toggle `dashMode` input | No flicker; table persists correctly |
| 7 | Score semantics | Grep for `\bProb\b` in codebase | Zero occurrences (comments exempt) |
| 8 | Compile | Paste into TradingView Pine Editor | No syntax errors |

---

## Deployment Sequence

1. **Phase 1** (today): Renames + Kelly removal + disclaimer.
2. **Phase 2** (this week): ATR smooth + SMT fix + label caps.
3. **Phase 3** (next week): OB fix + volume normalization + HIST_MAX + corr breakdown.
4. **Phase 4**: Validation checklist on 1H, 15M, 5M, 1M timeframes. Compare pre/post outputs.
