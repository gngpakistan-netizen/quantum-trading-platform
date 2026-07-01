"""Replay Engine — Bar-by-bar state machine matching Pine's execution model.

Simulates Pine's sequential execution: for each bar, compute features,
generate signals, manage positions. Captures full state snapshots.
"""

from dataclasses import dataclass, field
from datetime import datetime
from typing import Optional
from uuid import UUID, uuid4

from backend.common.models import Signal, Trade
from backend.strategy_engine.engine import (
    BarData,
    MarketState,
    StrategyConfig,
    StrategyEngine,
    compute_position_size,
)


@dataclass
class ReplaySnapshot:
    """Full system state at a single bar during replay."""
    bar_index: int
    timestamp: datetime
    bar: BarData
    market_state: MarketState
    signal: Optional[Signal] = None
    trade: Optional[Trade] = None


@dataclass
class ReplayResult:
    """Complete replay run output."""
    run_id: UUID = field(default_factory=uuid4)
    total_bars: int = 0
    bars_processed: int = 0
    trades_generated: int = 0
    signals_generated: int = 0
    snapshots: list[ReplaySnapshot] = field(default_factory=list)
    trades: list[Trade] = field(default_factory=list)
    signals: list[Signal] = field(default_factory=list)


class ReplayEngine:
    """Bar-by-bar replay engine with state machine."""

    def __init__(self, config: Optional[StrategyConfig] = None):
        if config is None:
            config = StrategyConfig()
        self.config = config
        self.strategy = StrategyEngine(config)
        self.result = ReplayResult()
        self.prev_state: Optional[MarketState] = None

    def reset(self) -> None:
        """Reset engine state for a new replay run."""
        self.result = ReplayResult()
        self.prev_state = None

    def feed_bar(self, bar_data: BarData, bar_index: int) -> ReplaySnapshot:
        """Process a single bar through the strategy engine."""
        # Compute market state
        market_state = self.strategy.compute(bar_data, self.prev_state)

        # Generate signal if trade conditions met
        signal = None
        if market_state.should_buy or market_state.should_sell:
            signal = Signal(
                timestamp=datetime.fromtimestamp(bar_data.timestamp),
                symbol="XAUUSD",
                timeframe="5m",
                direction="long" if market_state.should_buy else "short",
                confidence=float(market_state.confidence_score),
                entry_price=market_state.tp_entry,
                stop_loss=market_state.tp_sl,
                tp1=market_state.tp1,
                tp2=market_state.tp2,
                tp1_rr=market_state.tp_rr1,
                tp2_rr=market_state.tp_rr2,
                bias_scores={
                    "bull": market_state.bull_score,
                    "bear": market_state.bear_score,
                    "range": market_state.range_score,
                },
                trend_score=market_state.bull_trend_score,
                liq_score=float(market_state.liq_dest_score),
                session_score=market_state.session_quality,
                regime=("trending_up" if market_state.regime_trending else
                        "ranging" if market_state.regime_ranging else "volatile"),
                source_engine="strategy_engine",
                ri1_requirements=["R-009", "R-010", "R-011", "R-012", "R-013"],
            )
            self.result.signals.append(signal)
            self.result.signals_generated += 1

        # Generate trade if signal exists
        trade = None
        if signal:
            position_size = compute_position_size(
                entry_price=signal.entry_price,
                stop_loss=signal.stop_loss,
                account_size=self.config.account_size,
                risk_percent=self.config.risk_percent,
                point_value=self.config.point_value,
            )
            trade = Trade(
                signal_id=signal.signal_id,
                symbol="XAUUSD",
                direction=signal.direction,
                entry_price=signal.entry_price,
                entry_time=signal.timestamp,
                entry_signal_time=signal.timestamp,
                entry_bar_time=datetime.fromtimestamp(bar_data.timestamp),
                stop_loss=signal.stop_loss,
                tp1=signal.tp1,
                tp2=signal.tp2,
                tp1_rr=signal.tp1_rr,
                tp2_rr=signal.tp2_rr,
                size=position_size,
                risk_amount=self.config.account_size * (self.config.risk_percent / 100.0),
                risk_pct=self.config.risk_percent,
                decision_path=["strategy_engine_signal"],
                ri1_requirements=["R-009", "R-010", "R-011"],
            )
            self.result.trades.append(trade)
            self.result.trades_generated += 1

        # Capture snapshot
        snapshot = ReplaySnapshot(
            bar_index=bar_index,
            timestamp=datetime.fromtimestamp(bar_data.timestamp),
            bar=bar_data,
            market_state=market_state,
            signal=signal,
            trade=trade,
        )
        self.result.snapshots.append(snapshot)
        self.result.bars_processed += 1

        # Update state for next bar
        self.prev_state = market_state
        return snapshot

    def run(self, bars: list[BarData]) -> ReplayResult:
        """Run full replay on a list of bars."""
        self.reset()
        self.result.total_bars = len(bars)

        for i, bar in enumerate(bars):
            self.feed_bar(bar, i)

        return self.result

    def generate_validation_data(self) -> dict[str, object]:
        """Generate validation-ready data from replay results."""
        return {
            "trades": self.result.trades,
            "signals": self.result.signals,
            "market_states": [s.market_state for s in self.result.snapshots],
            "total_bars": self.result.total_bars,
            "trades_generated": self.result.trades_generated,
            "signals_generated": self.result.signals_generated,
        }
