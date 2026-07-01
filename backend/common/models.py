from datetime import datetime
from typing import Optional
from uuid import UUID, uuid4

from pydantic import BaseModel


class Bar(BaseModel):
    timestamp: datetime
    open: float
    high: float
    low: float
    close: float
    volume: float


class FeatureSet(BaseModel):
    adx: float
    ema_spread: float
    htf_trend: str
    volume_ratio: float
    quality_scores: dict[str, float]


class TradeSignal(BaseModel):
    signal_type: str
    entry_price: float
    stop: float
    tp1: float
    tp2: float
    confidence: float


class Signal(BaseModel):
    signal_id: str = ""
    timestamp: datetime
    symbol: str
    timeframe: str
    direction: str
    confidence: float
    entry_price: float
    stop_loss: float
    tp1: float
    tp2: float
    tp1_rr: float
    tp2_rr: float
    bias_scores: dict[str, float]
    trend_score: float
    liq_score: float
    session_score: float
    regime: str
    source_engine: str
    ri1_requirements: list[str]


class Trade(BaseModel):
    trade_id: str = ""
    signal_id: str
    symbol: str
    direction: str
    entry_price: float
    entry_time: datetime
    entry_signal_time: datetime
    entry_bar_time: datetime
    stop_loss: float
    tp1: float
    tp2: float
    tp1_rr: float
    tp2_rr: float
    size: float
    risk_amount: float
    risk_pct: float
    decision_path: list[str]
    ri1_requirements: list[str]
    exit_price: Optional[float] = None
    exit_time: Optional[datetime] = None
    pnl: Optional[float] = None
    status: str = "open"
    execution_timing_ms: float = 0.0
