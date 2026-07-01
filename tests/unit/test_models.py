"""Tests for core data models."""

from backend.common.models import Signal, Trade, FeatureSet, Bar
from datetime import datetime


class TestSignal:
    def test_default_direction(self):
        s = Signal()
        assert s.direction == "neutral"

    def test_signal_id_is_uuid(self):
        s = Signal()
        assert s.signal_id is not None

    def test_confidence_range(self):
        s = Signal(confidence=75.0)
        assert s.confidence == 75.0

    def test_bias_scores_default(self):
        s = Signal()
        assert s.bias_scores == {"bull": 0, "bear": 0, "range": 0}


class TestTrade:
    def test_default_direction(self):
        t = Trade()
        assert t.direction == "long"

    def test_trade_id_is_uuid(self):
        t = Trade()
        assert t.trade_id is not None

    def test_optional_fields_default_to_none(self):
        t = Trade()
        assert t.exit_price is None
        assert t.exit_time is None
        assert t.pnl is None


class TestFeatureSet:
    def test_default_regime(self, sample_features):
        assert sample_features.regime == "ranging"

    def test_adx_value(self, sample_features):
        assert sample_features.adx == 28.0

    def test_correlation_defaults(self, sample_features):
        assert sample_features.correlations == {}
