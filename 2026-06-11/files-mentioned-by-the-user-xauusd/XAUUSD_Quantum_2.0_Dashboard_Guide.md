# XAUUSD Quantum 2.0 v2.5e — Dashboard Reference Guide

> **Important**: This dashboard provides directional bias, not guaranteed predictions.
> See the in-code disclaimer: ordinal evidence scores, not statistically validated probabilities.
> Multiple factors should align before entering a trade.

---

## Color Palette (from code)

| Color | Hex | Usage |
|-------|-----|-------|
| Bullish | `#00D084` | Bull OB lines/fill, Bull label text, Bull macro, premium zone fill |
| Bearish | `#FF4D4D` | Bear OB lines/fill, Bear label text, Bear macro, discount zone fill |
| Neutral | `#A0A0A0` | EMA20 line, ADX, RSI, correlation health, neutral bias text |
| Forecast / Liq | `#00C2FF` | FVG lines/fill, PMH/PML lines/labels, forecast cone, label text |
| Warning | `#FF8C00` | PDH/PDL lines, Bear MSS label, displacement label, session/warning text |
| Red / Coral | `#FF6B6B` / `#FF4D4D` | Bull BOS label, Bull CHOCH label |
| Amber | `#FFB84D` | Bull MSS label |
| Blue | `#3B82F6` | EMA100 line |
| Gold | `#FFD700` | EMA200 line |
| Purple | `#A855F7` | VWAP line, PWH/PWL lines/labels |
| Slate Blue | `#60A5FA` | CDH/CDL lines |
| Dashboard BG | `#0F172A` 92% opacity | All table backgrounds |

---

## Full-Width Dashboard (7 columns × 8 rows)

### Row 0 — Column Headers

| SIG | PROB | MTF | STRUCT | MACRO | LIQUID | TRADE |
|-----|------|-----|--------|-------|--------|-------|

### Row 1 — Signal Summary
Signal direction + confidence %, bias label, macro bias, regime score, session, liquidity target, symbol + change.

- **Col 0**: `BUY` / `SEL` / `NEU` + confidence % (≥60 green, 40–59 orange, <40 red)
- **Col 1**: Current directional bias (BULL / BEAR / NEUTRAL)
- **Col 2**: Macro bias from 4H + Daily
- **Col 3**: `R` + regime score (0–100, higher = more trending)
- **Col 4**: Session label (`LON-KZ`, `NY-KZ`, etc.) + quality score (0–100)
- **Col 5**: Primary liquidity destination (PDH / PDL / PWH / PWL / PMH / PML / EQH / EQL)
- **Col 6**: Symbol (XAUUSD) + price change %

### Row 2 — Multi-Timeframe Alignment
`5M` `15M` `1H` `4H` `D` each with arrow + alignment count + structure summary

- Arrows: `▲` bullish, `▼` bearish, `—` neutral
- `AN4/5` = 4 of 5 timeframes aligned (≥4 strong, ≤2 weak)
- `B▲ C— M▼` = BOS bullish, CHOCH neutral, MSS bearish

### Row 3 — Probabilities & Forecast
`H B 62` = Historical analog Bull 62%, `F B 58` = Forecast Bull 58%

| Field | Meaning | Action |
|-------|---------|--------|
| `H B 62` | Historical analog match: direction + probability | ≥60% = directional edge |
| `F B 58` | M12 forecast engine: direction + probability | ≥60% = forecast agrees |
| `WR 54%` | Win rate from historical matches | >55% = edge |
| `EV+0.4` | Expected value in ATR units | Positive = favorable |
| `KF 8%` | Kelly fraction (capped 25%) | "NO BET" if ≤0 |
| `N147` | Sample size of historical matches | ≥30 reliable, <10 noisy |
| `Good` | Calibration grade | Excellent / Good / Fair / Poor |

> **⚠ Critical**: If N < 30, ignore WR, EV, KF — insufficient data.

### Row 4 — Macro Context
`DXY▼▼` `10Y▼▼` `XAG▲▲` `EUR▲` `SPX≈`

- `▲▲` / `▼▼` = Strong (>1.5% deviation from EMA), `▲` / `▼` = Moderate, `≈` = Neutral
- `CR` + correlation health score (0–100) — above 60 = strong macro relationships
- `MR` + mean reversion score (+ = overbought, − = oversold)

**Macro alignment for gold:**

| Asset | Bullish for Gold | Bearish for Gold |
|-------|-----------------|-----------------|
| DXY | Falling `▼` | Rising `▲` |
| US 10Y | Falling `▼` | Rising `▲` |
| Silver (XAG) | Rising `▲` | Falling `▼` |
| EUR/USD | Rising `▲` | Falling `▼` |
| S&P 500 | Rising `▲` | Falling `▼` |

Most reliable when ≥3/5 align.

### Row 5 — Liquidity Levels
`PDH○` `PWH○` `PMH○` `PDL↑` `PWL○` `PML○` `LQ OK`

- `↑` = primary target, `↓` = secondary target, `○` = not targeted
- `LQ` + health label (OK / WARN / STALE)
- EQH / EQL shown as active if present

### Row 6 — Volume Profile
`VP4346` `VH4350` `VL4320` `VA35%` `PS●` `FVG— OB—` `VD+2.1k`

| Field | Meaning |
|-------|---------|
| VP + price | Volume Point of Control (most traded price) |
| VH + price | Value Area High (70% volume below) |
| VL + price | Value Area Low (70% volume above) |
| VA + % | Value Area width as % of range (narrow = high conviction) |
| PS + symbol | Position relative to VA: `▲` above, `▼` below, `●` inside |
| FVG + score | Active fair value gap quality (— = none) |
| OB + score | Active order block quality (— = none) |
| VD + delta | Volume delta (+ = buying pressure, − = selling) |

### Row 7 — Trade Plan & Performance
`E4346` `SL4331` `TP4360` `RR1:1.0` `SR 0.82` `DD 12%` `PF 1.2`

| Field | Meaning | Threshold |
|-------|---------|-----------|
| E | Entry price (current close) | — |
| SL | Stop loss (1.5 ATR away) | — |
| TP | Take profit 1 (1.5 ATR reward) | — |
| RR | Risk:Reward ratio | ≥1:1 minimum |
| SR | Sharpe ratio | <1.0 = poor risk-adjusted |
| DD | Max drawdown % | Lower is better |
| PF | Profit factor (gross wins / losses) | >1.5 good, <1.0 negative |
| IS | In-sample win rate % | — |
| OOS | Out-of-sample win rate % | Should be near IS |

---

## Compact Layout (7 columns × 8 rows, abbreviated text)
Same structure as Full-Width but minimal text:
- `5▲` instead of `5M▲`
- `E+0.4` instead of `EV+0.4`
- `KF8%` instead of `KF 8%`

## Mobile Layout (4 columns × 7 rows)
Prioritized for small screens:
- Row 0: Signal + Bias + Macro + Session
- Row 1: 5M/15M/1H + 4H/D + Alignment + Structure
- Row 2: Historical + Forecast + Win Rate + EV
- Rows 3–6: Liquidity, Macro, Health, Volume Profile

---

## Signal Quality Grades

| Grade | Score | Meaning |
|-------|-------|---------|
| A+ | ≥90 | Very strong confluence |
| A | ≥75 | Strong confluence |
| B | ≥55 | Moderate confluence |
| C | ≥35 | Weak confluence |
| D | <35 | Poor / no confluence |

Grades combine BOS/CHOCH/MSS/FVG/OB scores with HTF alignment, macro, confidence, and session quality.

---

## Session Quality Guide

| Session | Raw Score (pre-norm) | Normal Range | Notes |
|---------|---------------------|--------------|-------|
| Asian | 10–15 | 13–19 | Low volume, range trading |
| London Open (killzone) | 55 | 69 | Breakouts, high momentum |
| London Fix | 75 | 94 | Reversals common |
| NY Open (killzone) | 65 | 81 | Momentum, continuation |
| London Close | 50 | 63 | Profit-taking, reduced volume |
| Outside active sessions | 0–10 | 0–13 | Avoid — low liquidity |

Session score ≥ 30 is required for `buyPreFilters` / `sellPreFilters`.

---

## Entry Conditions (from code)

The indicator uses these gates — not a recommendation, but what the code evaluates:

### Long (`shouldBuy`)
```
bullTrend AND htfBullGate AND macroBull AND regimeConfHigh
AND bullRsiDiv AND recentBars AND sessionQuality >= 30
AND (bullBOS OR displacementUp)
```

### Short (`shouldSell`)
```
bearTrend AND htfBearGate AND macroBear AND regimeConfHigh
AND bearRsiDiv AND recentBars AND sessionQuality >= 30
AND (bearBOS OR displacementDown)
```

### Pre-filter breakdown:
- **Trend**: Weighted trend score ≥ 60
- **HTF alignment**: ≥ 2 of 3 (1H + 4H + Daily)
- **Macro**: ≥ 3 of 5 assets aligned
- **Regime confidence**: ≥ 60
- **RSI divergence**: Pivot-based divergence detection (or pass-through if disabled)
- **Recent bars**: Within last 120 bars
- **Session quality**: ≥ 30
- **Signal trigger**: BOS (break of structure) or displacement candle

---

## Structure Labels

| Label | Bull Color | Bear Color | Size | Meaning |
|-------|-----------|-----------|------|---------|
| BOS | `#FF6B6B` bg, white text | `#FF4D4D` bg, white text | normal | Break of Structure — trend confirmation |
| CHOCH | `#FF6B6B` bg, white text | `#FF4D4D` bg, white text | normal | Change of Character — potential reversal |
| MSS | `#FFB84D` bg, white text | `#FF8C00` bg, white text | small | Market Structure Shift — confirmed reversal |
| Disp | `#00D084` bg, white text | `#FF4D4D` bg, white text | small | Displacement — strong momentum |
| Liq Sweep | `#FFB84D` bg | `#FF8C00` bg | small | Liquidity sweep detected |
| Asia Sweep | `#00D084` bg | `#FF4D4D` bg | small | Asian session sweep |
| EQH / EQL | `#FF8C00` bg | `#FF8C00` bg | small | Equal Highs / Equal Lows — reversal bias |
| Manip | `#A0A0A0` bg | `#A0A0A0` bg | small | Manipulation detected |

**Concurrent limits**: BOS/CHOCH max 3, MSS max 3, EQH/EQL max 5 each, Manip max 2.

---

## Chart Visuals Reference

| Element | Bullish | Bearish | Style |
|---------|---------|---------|-------|
| OB lines | `#00D084` 30% opacity | `#FF4D4D` 30% opacity | Solid, span 50 bars |
| OB fill | `#00D084` 90% opacity | `#FF4D4D` 90% opacity | Plot fill |
| FVG lines | `#00C2FF` 20% opacity | `#00C2FF` 20% opacity | Solid, span 50 bars |
| FVG fill | `#00C2FF` 88% opacity | `#00C2FF` 88% opacity | Plot fill |
| CDH/CDL | `#60A5FA` 70% opacity | `#60A5FA` 70% opacity | Dashed, period→current bar |
| PDH/PDL | `#FF8C00` 60% opacity | `#FF8C00` 60% opacity | Dotted, extend right |
| PWH/PWL | `#A855F7` 60% opacity | `#A855F7` 60% opacity | Dotted, extend right |
| PMH/PML | `#00C2FF` 60% opacity | `#00C2FF` 60% opacity | Dotted, extend right |
| Price line | `#F1F5F9` 70% opacity | `#F1F5F9` 70% opacity | Dotted, extend right |
| Premium zone | `#FF4D4D` 95% | — | Box fill above midpoint |
| Discount zone | — | `#00D084` 95% | Box fill below midpoint |
| EMA20 | `#A0A0A0` 70% | `#A0A0A0` 70% | Dotted |
| EMA100 | `#3B82F6` 70% | `#3B82F6` 70% | Dotted |
| EMA200 | `#FFD700` 80% | `#FFD700` 80% | Dotted, 2px width |
| VWAP | `#A855F7` 60% | `#A855F7` 60% | Dotted |
| SR levels | `#00C2FF` 50% | `#00C2FF` 50% | Solid |
| Forecast cone | `#00C2FF` center 70%, bands 30% | same | Solid center, dashed bands |

---

## Using the Dashboard (step-by-step)

1. **Check Row 1 SIG**: ≥60% green = high conviction
2. **Verify MTF alignment (Row 2)**: ≥3/5 arrows same direction
3. **Check macro (Row 4)**: ≥3/5 assets align with bias
4. **Identify liquidity target (Row 5)**: `↑` = where price is drawn
5. **Validate with volume profile (Row 6)**: Price above VAH = strong trend, below VAL = weak, inside VA = chop
6. **Check stats (Row 3)**: N ≥ 30 + WR > 55% + EV > 0
7. **Execute with trade plan (Row 7)**: RR ≥ 1:1, respect SL

### Invalidation triggers:
- Price closes below VWAP (longs) / above VWAP (shorts)
- EMA20 crosses EMA100 counter to position
- New CHOCH or MSS against position direction
- DXY and yields reverse direction simultaneously
- Session quality drops below 30
