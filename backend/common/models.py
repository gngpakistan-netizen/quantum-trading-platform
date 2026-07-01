from datetime import datetime
from typing import Optional

from pydantic import BaseModel


class Bar(BaseModel):
    symbol: str = "XAUUSD"
    timestamp: datetime = datetime.now()
    timeframe: str = "5m"
    open: float = 0.0
    high: float = 0.0
    low: float = 0.0
    close: float = 0.0
    volume: float = 0.0


class FeatureSet(BaseModel):
    symbol: str = "XAUUSD"
    timestamp: datetime = datetime.now()
    timeframe: str = "5m"
    bar: Optional[Bar] = None
    adx: float = 0.0
    adx_direction: str = "neutral"
    ema50: float = 0.0
    ema200: float = 0.0
    ema_spread: float = 0.0
    atr14: float = 0.0
    volume_ratio: float = 1.0
    rsi: float = 50.0
    htf_trend: str = "neutral"
    quality_scores: dict[str, float] = {}
    regime: str = "ranging"
    correlations: dict[str, float] = {}


class TradeSignal(BaseModel):
    signal_type: str = "market"
    entry_price: float = 0.0
    stop: float = 0.0
    tp1: float = 0.0
    tp2: float = 0.0
    confidence: float = 0.0


class Signal(BaseModel):
    signal_id: str = ""
    timestamp: datetime = datetime.now()
    symbol: str = "XAUUSD"
    timeframe: str = "5m"
    direction: str = "neutral"
    confidence: float = 0.0
    entry_price: float = 0.0
    stop_loss: float = 0.0
    tp1: float = 0.0
    tp2: float = 0.0
    tp1_rr: float = 0.0
    tp2_rr: float = 0.0
    bias_scores: dict[str, float] = {"bull": 0, "bear": 0, "range": 0}
    trend_score: float = 0.0
    liq_score: float = 0.0
    session_score: float = 0.0
    regime: str = "ranging"
    source_engine: str = ""
    ri1_requirements: list[str] = []


class Trade(BaseModel):
    trade_id: str = ""
    signal_id: str = ""
    symbol: str = "XAUUSD"
    direction: str = "long"
    entry_price: float = 0.0
    entry_time: datetime = datetime.now()
    entry_signal_time: datetime = datetime.now()
    entry_bar_time: datetime = datetime.now()
    stop_loss: float = 0.0
    tp1: float = 0.0
    tp2: float = 0.0
    tp1_rr: float = 0.0
    tp2_rr: float = 0.0
    size: float = 0.0
    risk_amount: float = 0.0
    risk_pct: float = 0.0
    decision_path: list[str] = []
    ri1_requirements: list[str] = []
    exit_price: Optional[float] = None
    exit_time: Optional[datetime] = None
    pnl: Optional[float] = None
    status: str = "open"
    execution_timing_ms: float = 0.0
