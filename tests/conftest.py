"""Pytest configuration and fixtures."""

import pytest
from backend.common.models import Bar, FeatureSet
from datetime import datetime


@pytest.fixture
def sample_bar() -> Bar:
    return Bar(
        symbol="XAUUSD",
        timestamp=datetime(2026, 7, 1, 12, 0, 0),
        timeframe="5m",
        open=4120.0,
        high=4130.0,
        low=4115.0,
        close=4126.5,
        volume=12500.0,
    )


@pytest.fixture
def sample_features(sample_bar: Bar) -> FeatureSet:
    return FeatureSet(
        symbol=sample_bar.symbol,
        timestamp=sample_bar.timestamp,
        timeframe=sample_bar.timeframe,
        bar=sample_bar,
        adx=28.0,
        adx_direction="up",
        ema50=4110.0,
        ema200=4080.0,
        ema_spread=0.0073,
        atr14=8.5,
        volume_ratio=1.24,
        rsi=58.0,
    )
