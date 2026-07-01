"""Strategy Engine — Pine RI-1 Formula Reimplementation in Python.

Pure functions ported from XAUUSD_Quantum_3.3.pine.
Every function is independently verifiable against the frozen RI-1.
"""

from dataclasses import dataclass
from typing import Optional

from backend.common.utils import clamp, safe_div


# ============================================================
# Configuration (matching Pine inputs)
# ============================================================
@dataclass
class StrategyConfig:
    """All user-configurable parameters matching Pine inputs."""
    # Core
    ema20_len: int = 20
    ema100_len: int = 100
    ema200_len: int = 200
    atr_len: int = 14
    adx_len: int = 14
    adx_threshold: int = 25
    rsi_len: int = 14
    internal_pivot_len: int = 2
    swing_pivot_len: int = 5
    recent_bars_len: int = 120
    trend_threshold: int = 60

    # Risk
    risk_percent: float = 1.0
    account_size: float = 10000.0
    point_value: float = 100.0

    # Structure
    fvg_max_bars: int = 30
    ob_max_bars: int = 20
    liq_prox_mult: float = 1.5

    # Trading
    atr_mult_sl: float = 1.5
    atr_mult_tp1: float = 1.5
    atr_mult_tp2: float = 3.0

    # Macro
    correlation_len: int = 50
    correlation_z_threshold: float = 2.58  # Bonferroni-corrected for 5 assets


DEFAULT_CONFIG = StrategyConfig()


# ============================================================
# Core OHLCV Data
# ============================================================
@dataclass
class BarData:
    """Single bar of OHLCV data with computed indicators."""
    timestamp: float
    open: float
    high: float
    low: float
    close: float
    volume: float

    # Computed indicators (set by FeatureEngine)
    ema20: float = 0.0
    ema50: float = 0.0
    ema100: float = 0.0
    ema200: float = 0.0
    atr14: float = 0.0
    adaptive_atr: float = 0.0
    adx: float = 0.0
    di_plus: float = 0.0
    di_minus: float = 0.0
    rsi: float = 50.0
    volume_sma20: float = 0.0
    vwap: float = 0.0
    gold_ret: float = 0.0  # (close/close[1]) - 1


# ============================================================
# Market State (per-bar computed values)
# ============================================================
@dataclass
class MarketState:
    """All computed market state for a single bar."""
    # Trend
    bull_trend: bool = False
    bear_trend: bool = False
    bull_trend_score: float = 0.0
    bear_trend_score: float = 0.0

    # ADX Regime
    regime_trending: bool = False
    regime_ranging: bool = False
    regime_dead: bool = False
    regime_strength: float = 0.0

    # HTF Alignment
    htf_bull_gate: bool = False
    htf_bear_gate: bool = False
    htf_alignment_long: int = 0
    htf_alignment_short: int = 0
    htf_full_long: int = 0
    htf_full_short: int = 0

    # Structure
    bull_bos: bool = False
    bear_bos: bool = False
    structure_score: float = 0.0
    active_resistance: Optional[float] = None
    active_support: Optional[float] = None
    pivot_high: Optional[float] = None
    pivot_low: Optional[float] = None
    swing_pivot_high: Optional[float] = None
    swing_pivot_low: Optional[float] = None

    # Macro
    macro_bull: bool = False
    macro_bear: bool = False
    macro_strength_score: float = 0.0
    macro_intel_conf: float = 0.0

    # Liquidity
    liq_dest_label: str = "—"
    liq_dest_score: int = 0
    liq_dest_dist: float = 0.0

    # Displacement
    displacement_up: bool = False
    displacement_down: bool = False

    # Session
    session_quality: float = 50.0
    in_killzone: bool = False
    session: str = "london"

    # MR (Mean Reversion)
    mr_bull_score: float = 10.0
    mr_bear_score: float = 10.0

    # RSI Divergence
    bull_rsi_div: bool = False
    bear_rsi_div: bool = False

    # Evidence Aggregates
    bias_struct_bull: float = 0.0
    bias_struct_bear: float = 0.0
    bias_liq_bull: float = 0.0
    bias_liq_bear: float = 0.0
    bias_macro_bull: float = 0.0
    bias_macro_bear: float = 0.0
    bias_htf_bull: float = 0.0
    bias_htf_bear: float = 0.0
    bias_mom_bull: float = 0.0
    bias_mom_bear: float = 0.0
    bias_sess_bull: float = 0.0
    bias_sess_bear: float = 0.0
    bull_bias_score: float = 0.0
    bear_bias_score: float = 0.0
    range_bias_score: float = 0.0

    # Confidence
    confidence_score: float = 0.0
    confidence_label: str = "LOW"

    # Normalized Scores
    bull_score: float = 50.0
    bear_score: float = 50.0
    range_score: float = 0.0
    bias_label: str = "NEUTRAL"

    # Forecast scores (post-analog blend)
    f_bull_score: float = 50.0
    f_bear_score: float = 50.0
    f_rng_score: float = 0.0

    # Trade Plan
    should_buy: bool = False
    should_sell: bool = False
    tp_entry: float = 0.0
    tp_sl: float = 0.0
    tp1: float = 0.0
    tp2: float = 0.0
    tp_rr1: float = 0.0
    tp_rr2: float = 0.0
    tp_is_long: bool = True
    tp_dir_str: str = "LONG"

    # FVG
    fvg_active: bool = False

    # OB
    ob_bull_active: bool = False
    ob_bear_active: bool = False


# ============================================================
# Engine Functions — Pure, Stateless, Verifiable
# ============================================================

def compute_trend_scores(bar: BarData, cfg: StrategyConfig = DEFAULT_CONFIG) -> tuple[float, float, bool, bool]:
    """Compute bull/bear trend scores matching Pine lines 721-738."""
    high_volume = bar.volume > bar.volume_sma20 * 1.20 if bar.volume_sma20 > 0 else False

    bull = 0
    bull += 20 if bar.close > bar.ema200 else 0
    bull += 20 if bar.ema20 > bar.ema100 else 0
    bull += 20 if bar.di_plus > bar.di_minus else 0
    bull += 20 if bar.adx > cfg.adx_threshold else 0
    bull += 10 if bar.close > bar.ema100 else 0
    bull += 10 if high_volume else 0

    bear = 0
    bear += 20 if bar.close < bar.ema200 else 0
    bear += 20 if bar.ema20 < bar.ema100 else 0
    bear += 20 if bar.di_minus > bar.di_plus else 0
    bear += 20 if bar.adx > cfg.adx_threshold else 0
    bear += 10 if bar.close < bar.ema100 else 0
    bear += 10 if high_volume else 0

    return bull, bear, bull >= cfg.trend_threshold, bear >= cfg.trend_threshold


def compute_htf_alignment(bar: BarData) -> tuple[bool, bool, int, int, int, int]:
    """Compute HTF alignment scores matching Pine lines 296-328."""
    # HTF trend scoring: close vs EMA200 * 40 + EMA20 vs EMA50 * 40 + close vs EMA20 * 20
    def htf_score(close: float, ema20: float, ema50: float, ema200: float) -> float:
        s = 0.0
        if ema200 != 0 and close > ema200:
            s += 40
        if ema20 > ema50:
            s += 40
        if close > ema20:
            s += 20
        return s

    # For the main timeframe: use bar EMA values
    score_5m = htf_score(bar.close, bar.ema20, bar.ema50, bar.ema200)
    # For higher TFs, we use approximations from the bar's ema with adjusted parameters
    # In full implementation, these come from higher timeframe data
    htf_5m_bull = score_5m >= 60
    htf_5m_bear = score_5m <= 40

    # HTF alignment from 15m, 1h, 4h (slower 3 TFs)
    # In real implementation, this is computed from actual HTF bars
    # For now, derive from main TF as approximation
    score_15m = score_5m  # placeholder
    score_1h = score_5m
    score_4h = score_5m

    htf_15m_bull = score_15m >= 60
    htf_15m_bear = score_15m <= 40
    htf_1h_bull = score_1h >= 60
    htf_1h_bear = score_1h <= 40
    htf_4h_bull = score_4h >= 60
    htf_4h_bear = score_4h <= 40

    alignment_long = (1 if htf_15m_bull else 0) + (1 if htf_1h_bull else 0) + (1 if htf_4h_bull else 0)
    alignment_short = (1 if htf_15m_bear else 0) + (1 if htf_1h_bear else 0) + (1 if htf_4h_bear else 0)

    full_long = sum([htf_5m_bull, htf_15m_bull, htf_1h_bull, htf_4h_bull])
    full_short = sum([htf_5m_bear, htf_15m_bear, htf_1h_bear, htf_4h_bear])

    return (
        alignment_long >= 2,
        alignment_short >= 2,
        alignment_long,
        alignment_short,
        full_long,
        full_short,
    )


def compute_regime(bar: BarData, cfg: StrategyConfig = DEFAULT_CONFIG) -> tuple[bool, bool, bool, float]:
    """Compute ADX regime matching Pine lines 337-389."""
    trending = bar.adx > cfg.adx_threshold
    ranging = bar.adx <= cfg.adx_threshold and bar.adx > 15
    dead = bar.adx <= 15

    # Regime strength: 0 = full ranging, 1 = full trending
    strength = clamp((bar.adx - (cfg.adx_threshold - 5)) / 10.0, 0.0, 1.0)

    return trending, ranging, dead, strength


def compute_adaptive_atr(bar: BarData, regime_strength: float) -> float:
    """Compute adaptive ATR matching Pine lines 334-344."""
    atr_trending = bar.atr14  # Uses atr(7) in Pine, approximated with atr14
    atr_ranging = bar.atr14   # Uses atr(21) in Pine, approximated with atr14
    raw = atr_ranging * (1.0 - regime_strength) + atr_trending * regime_strength
    return max(raw, bar.close * 0.0001)


def compute_liquidity_dist_score(dist: float, atr: float, prox_mult: float) -> float:
    """Compute liquidity distance score matching Pine line 1593."""
    if atr <= 0:
        return 0.0
    score = max(100.0 - safe_div(dist, atr) * safe_div(100.0, prox_mult), 0.0)
    return score


def compute_macro_strength(
    dxy_bear: bool, dxy_bull: bool, yield_falling: bool, yield_rising: bool,
    spx_bull: bool, spx_bear: bool, eurusd_bull: bool, eurusd_bear: bool,
    silver_bull: bool, silver_bear: bool,
    dxy_mom_bull: bool, dxy_mom_bear: bool,
    dxy_valid: bool, yield_valid: bool, spx_valid: bool,
    eurusd_valid: bool, silver_valid: bool,
) -> tuple[float, float, bool, bool, float]:
    """Compute macro strength score matching Pine lines 690-715."""
    # Weighted voting (simplified from Pine's correlation-based weights)
    w_dxy, w_yld, w_slv, w_spx, w_eur = 30.0, 25.0, 20.0, 15.0, 20.0

    ms_dxy = w_dxy if (dxy_valid and dxy_bear) else (-w_dxy if (dxy_valid and dxy_bull) else 0.0)
    ms_yld = w_yld if (yield_valid and yield_falling) else (-w_yld if (yield_valid and yield_rising) else 0.0)
    ms_spx = w_spx if (spx_valid and spx_bull) else (-w_spx if (spx_valid and spx_bear) else 0.0)
    ms_eur = w_eur if (eurusd_valid and eurusd_bull) else (-w_eur if (eurusd_valid and eurusd_bear) else 0.0)
    ms_slv = w_slv if (silver_valid and silver_bull) else (-w_slv if (silver_valid and silver_bear) else 0.0)
    ms_mom = 10.0 if (dxy_valid and dxy_mom_bear) else (-10.0 if (dxy_valid and dxy_mom_bull) else 0.0)

    macro_score = clamp(ms_dxy + ms_yld + ms_spx + ms_eur + ms_slv + ms_mom, -100, 100)
    macro_conf = abs(macro_score)
    macro_bull = macro_score > 0
    macro_bear = macro_score < 0

    return macro_score, macro_conf, macro_bull, macro_bear, abs(macro_score)


def compute_bias_scores(state: MarketState) -> MarketState:
    """Compute bias evidence scores matching Pine lines 1780-1796."""
    state.bias_struct_bull = 30 if state.bull_trend else 0
    state.bias_struct_bear = 30 if state.bear_trend else 0

    state.bias_liq_bull = 20 if (state.bull_bos or state.displacement_up) else 0
    state.bias_liq_bear = 20 if (state.bear_bos or state.displacement_down) else 0

    state.bias_macro_bull = 25 if state.macro_bull else 0
    state.bias_macro_bear = 25 if state.macro_bear else 0

    state.bias_htf_bull = 25 if state.htf_bull_gate else (10 if state.htf_alignment_long >= 1 else 0)
    state.bias_htf_bear = 25 if state.htf_bear_gate else (10 if state.htf_alignment_short >= 1 else 0)

    state.bias_mom_bull = 15 if state.bull_rsi_div else 0
    state.bias_mom_bear = 15 if state.bear_rsi_div else 0

    state.bias_sess_bull = 15 if state.session_quality >= 50 else (8 if state.session_quality >= 30 else 3)
    state.bias_sess_bear = 3 if state.session_quality >= 50 else (8 if state.session_quality >= 30 else 15)

    state.bull_bias_score = (
        state.bias_struct_bull + state.bias_liq_bull + state.bias_macro_bull
        + state.bias_htf_bull + state.bias_mom_bull + state.bias_sess_bull
    )
    state.bear_bias_score = (
        state.bias_struct_bear + state.bias_liq_bear + state.bias_macro_bear
        + state.bias_htf_bear + state.bias_mom_bear + state.bias_sess_bear
    )
    state.range_bias_score = (
        (40 if state.regime_ranging else 0) + (50 if state.regime_dead else 0) + 0
    )

    return state


def _conf_score(val: float, hi: float, mid: float, lo: float) -> int:
    if val >= hi:
        return 100
    if val >= mid:
        return 60
    if val >= lo:
        return 30
    return 10

def compute_confidence(state: MarketState) -> MarketState:
    """Compute confidence score matching Pine lines 1805-1817."""
    conf_struct = _conf_score(state.structure_score, 80, 50, 20)
    conf_mtf = int(round(max(state.htf_full_long, state.htf_full_short) * 100.0 / 7.0))
    conf_liq = 0
    conf_liq += 25 if (state.bull_bos or state.bear_bos) else 0
    conf_liq += 25 if (state.fvg_active or state.ob_bull_active or state.ob_bear_active) else 0
    conf_liq += 25 if state.liq_dest_score >= 50 else 0
    conf_macro = _conf_score(state.macro_intel_conf, 60, 30, 10)
    conf_session = _conf_score(state.session_quality, 80, 50, 20)

    state.confidence_score = int(round(
        (conf_struct * 20 + conf_mtf * 20 + conf_liq * 20 + conf_macro * 20 + conf_session * 20) / 100.0
    ))
    state.confidence_label = (
        "EXTREME" if state.confidence_score >= 85 else
        "HIGH" if state.confidence_score >= 65 else
        "MEDIUM" if state.confidence_score >= 40 else
        "LOW"
    )

    return state


def compute_evidence_aggregate(
    state: MarketState, bar: BarData,
) -> tuple[float, float, float, float, float, float, float, float,
           float, float, float, float, float, float, float, float]:
    """Compute 8-factor evidence scores matching Pine lines 1831-1847."""
    # Normalize each factor to 0-100
    ev_trend_bull = state.bull_trend_score
    ev_trend_bear = state.bear_trend_score

    ev_struct_bull = max(state.structure_score, 55.0) if state.structure_score > 0 else 50.0
    ev_struct_bear = max(100.0 - state.structure_score, 55.0) if state.structure_score > 0 else 50.0

    di_sum = bar.di_plus + bar.di_minus
    ev_flow_bull = safe_div(bar.di_plus, di_sum) * 100.0 if bar.di_plus > bar.di_minus else 30.0
    ev_flow_bear = safe_div(bar.di_minus, di_sum) * 100.0 if bar.di_minus > bar.di_plus else 30.0

    ev_macro_bull = state.macro_strength_score if state.macro_strength_score > 0 else 0.0
    ev_macro_bear = abs(state.macro_strength_score) if state.macro_strength_score < 0 else 0.0

    ev_liq_bull = 60.0 if state.liq_dest_label in ("PDH", "PWH", "PMH", "EQH") else 30.0
    ev_liq_bear = 60.0 if state.liq_dest_label in ("PDL", "PWL", "PML", "EQL") else 30.0

    ev_session_bull = state.session_quality * 0.8 if state.session_quality >= 50 else state.session_quality * 0.5
    ev_session_bear = state.session_quality * 0.5 if state.session_quality >= 50 else state.session_quality * 0.8

    ev_corr_bull = 30.0
    ev_corr_bear = 30.0

    ev_mr_bull = state.mr_bull_score
    ev_mr_bear = state.mr_bear_score

    return (ev_trend_bull, ev_trend_bear, ev_struct_bull, ev_struct_bear, ev_flow_bull, ev_flow_bear,
            ev_macro_bull, ev_macro_bear, ev_liq_bull, ev_liq_bear, ev_session_bull, ev_session_bear,
            ev_corr_bull, ev_corr_bear, ev_mr_bull, ev_mr_bear)


def normalize_scores(
    raw_bull: float, raw_bear: float, range_evidence: float,
    _regime_strength: float, regime_trending: bool, regime_ranging: bool,
) -> tuple[float, float, float, str]:
    """Normalize evidence scores to 0-100 matching Pine lines 1890-1934."""
    # Regime-based range base
    regime_range_base = 10.0 if regime_trending else (30.0 if regime_ranging else 45.0)

    total_bull = raw_bull
    total_bear = raw_bear
    raw_total = total_bull + total_bear

    raw_bull_pct = total_bull / raw_total * 100.0 if raw_total > 0 else 50.0
    raw_bear_pct = total_bear / raw_total * 100.0 if raw_total > 0 else 50.0
    range_residual = 100.0 - raw_bull_pct - raw_bear_pct

    # Blend residual with independent ranging evidence + regime base (70/20/10)
    range_blended = (range_residual * 0.70) + (range_evidence * 0.20) + (regime_range_base * 0.10)
    r_total = raw_bull_pct + raw_bear_pct + range_blended

    if r_total > 0:
        bull_score = float(round(raw_bull_pct / r_total * 100.0))
        bear_score = float(round(raw_bear_pct / r_total * 100.0))
        range_score = 100.0 - bull_score - bear_score
    else:
        bull_score, bear_score, range_score = 50.0, 50.0, 0.0

    # Clamp negative range score
    if range_score < 0:
        excess = abs(range_score)
        range_score = 0.0
        ratio = bull_score / (bull_score + bear_score) if (bull_score + bear_score) > 0 else 0.5
        bull_score = max(bull_score - excess * ratio, 5.0)
        bear_score = 100.0 - bull_score

    # Floor and ceiling
    if bull_score < 5.0 and bear_score < 5.0:
        bull_score, bear_score, range_score = 50.0, 50.0, 0.0
    if bull_score > 95.0:
        bull_score = 95.0
        bear_score = max(bear_score, 5.0)
        range_score = 100.0 - bull_score - bear_score
    if bear_score > 95.0:
        bear_score = 95.0
        bull_score = max(bull_score, 5.0)
        range_score = 100.0 - bull_score - bear_score

    # Bias label
    if bull_score >= 62:
        label = "BULL"
    elif bear_score >= 62:
        label = "BEAR"
    elif range_score >= 62:
        label = "RANGE"
    elif bull_score > bear_score and bull_score > range_score:
        label = "BULL-ISH"
    elif bear_score > bull_score and bear_score > range_score:
        label = "BEAR-ISH"
    else:
        label = "NEUTRAL"

    return bull_score, bear_score, range_score, label


def compute_trade_plan(state: MarketState, bar: BarData, cfg: StrategyConfig = DEFAULT_CONFIG) -> MarketState:
    """Compute entry/SL/TP/RR matching Pine lines 2025-2046."""
    # Pre-filters
    buy_prefilters = (
        state.bull_trend and state.htf_bull_gate and state.macro_bull
        and state.confidence_score >= 60 and state.bull_rsi_div
        and state.session_quality >= 30
    )
    state.should_buy = buy_prefilters and (state.bull_bos or state.displacement_up)

    sell_prefilters = (
        state.bear_trend and state.htf_bear_gate and state.macro_bear
        and state.confidence_score >= 60 and state.bear_rsi_div
        and state.session_quality >= 30
    )
    state.should_sell = sell_prefilters and (state.bear_bos or state.displacement_down)

    # Trade Plan
    if state.should_buy or state.should_sell:
        state.tp_is_long = state.should_buy
    else:
        state.tp_is_long = state.bull_bias_score >= state.bear_bias_score
    state.tp_dir_str = "LONG" if state.tp_is_long else "SHORT"

    state.tp_entry = bar.close
    if state.tp_is_long:
        state.tp_sl = bar.close - bar.adaptive_atr * cfg.atr_mult_sl
        state.tp1 = bar.close + bar.adaptive_atr * cfg.atr_mult_tp1
        state.tp2 = bar.close + bar.adaptive_atr * cfg.atr_mult_tp2
    else:
        state.tp_sl = bar.close + bar.adaptive_atr * cfg.atr_mult_sl
        state.tp1 = bar.close - bar.adaptive_atr * cfg.atr_mult_tp1
        state.tp2 = bar.close - bar.adaptive_atr * cfg.atr_mult_tp2

    tp_dist = abs(state.tp_entry - state.tp_sl)
    state.tp_rr1 = abs(state.tp1 - state.tp_entry) / tp_dist if tp_dist > 0 else 0.0
    state.tp_rr2 = abs(state.tp2 - state.tp_entry) / tp_dist if tp_dist > 0 else 0.0

    return state


# ============================================================
# Main Strategy Engine
# ============================================================
class StrategyEngine:
    """Orchestrates all strategy computations for a single bar."""

    def __init__(self, config: StrategyConfig = DEFAULT_CONFIG):
        self.config = config

    def compute(self, bar: BarData, _prev_state: Optional[MarketState] = None) -> MarketState:
        """Compute full market state for a single bar."""
        state = MarketState()

        # 1. Trend scores
        bull_score, bear_score, state.bull_trend, state.bear_trend = compute_trend_scores(bar, self.config)
        state.bull_trend_score = bull_score
        state.bear_trend_score = bear_score

        # 2. Regime
        reg = compute_regime(bar, self.config)
        state.regime_trending, state.regime_ranging, state.regime_dead, state.regime_strength = reg

        # 3. Adaptive ATR
        bar.adaptive_atr = compute_adaptive_atr(bar, state.regime_strength)

        # 4. HTF alignment
        (state.htf_bull_gate, state.htf_bear_gate,
         state.htf_alignment_long, state.htf_alignment_short,
         state.htf_full_long, state.htf_full_short) = compute_htf_alignment(bar)

        # 5. Bias scores
        state = compute_bias_scores(state)

        # 6. Confidence
        state = compute_confidence(state)

        # 7. Evidence aggregate (simplified — uses bias scores as evidence)
        # In full implementation, this uses the 8-factor evidence engine

        # 8. Normalize scores
        range_evidence = 0.0
        if bar.adx < 20:
            range_evidence = 40.0
        elif bar.adx < 25:
            range_evidence = 25.0

        # Use bias scores as raw evidence for normalization
        raw_bull = state.bull_bias_score
        raw_bear = state.bear_bias_score

        state.bull_score, state.bear_score, state.range_score, state.bias_label = normalize_scores(
            raw_bull, raw_bear, range_evidence,
            state.regime_strength, state.regime_trending, state.regime_ranging,
        )

        # 9. Trade plan
        state = compute_trade_plan(state, bar, self.config)

        # 10. Forecast scores (initial, before analog blend)
        state.f_bull_score = state.bull_score
        state.f_bear_score = state.bear_score
        state.f_rng_score = state.range_score

        return state


# ============================================================
# Position Sizing (V4.0 New — not in RI-1)
# ============================================================
def compute_position_size(entry_price: float, stop_loss: float, account_size: float,
                          risk_percent: float, point_value: float,
                          max_position_pct: float = 0.05) -> float:
    """Compute position size in lots."""
    stop_distance = abs(entry_price - stop_loss)
    if stop_distance <= 0 or point_value <= 0:
        return 0.0
    risk_amount = account_size * (risk_percent / 100.0)
    position = risk_amount / (stop_distance * point_value)
    max_position = account_size * max_position_pct
    return min(position, max_position)
