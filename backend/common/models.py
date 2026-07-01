from datetime import datetime

from pydantic import BaseModel


class Bar(BaseModel):
    timestamp: datetime
    open: float
    high: float
    low: float
    close: float
    volume: float

class TradeSignal(BaseModel):
    signal_type: str
    entry_price: float
    stop: float
    tp1: float
    tp2: float
    confidence: float
