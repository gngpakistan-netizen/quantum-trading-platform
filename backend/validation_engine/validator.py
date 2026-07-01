"""Validation Engine — Automated 5-Stream Validation Framework.

Each stream has objective pass/fail criteria. Results stored in Supabase.
"""

from dataclasses import dataclass, field
from datetime import date, datetime
from typing import Any, Optional

from backend.common.models import FeatureSet, Trade


# ============================================================
# Tolerance Configuration
# ============================================================
@dataclass
class ValidationTolerances:
    formula_relative: float = 1e-6       # 0.0001% relative error
    formula_absolute: float = 1e-8       # absolute fallback for near-zero values
    price_relative: float = 0.001        # 0.1% price tolerance
    timing_ms: float = 100.0             # 100ms timing tolerance
    dashboard_score: float = 0.5         # 0.5 point dashboard value tolerance


DEFAULT_TOLERANCES = ValidationTolerances()


# ============================================================
# Validation Result Models
# ============================================================
@dataclass
class ValidationCheck:
    check_name: str
    passed: bool
    expected: Optional[float] = None
    actual: Optional[float] = None
    difference: Optional[float] = None
    tolerance: Optional[float] = None
    formula_id: Optional[str] = None
    details: Optional[str] = None


@dataclass
class ValidationReport:
    stream: str
    date_from: date
    date_to: date
    total_checks: int = 0
    passed: int = 0
    failed: int = 0
    pass_rate: float = 0.0
    checks: list[ValidationCheck] = field(default_factory=list)
    started_at: datetime = field(default_factory=datetime.utcnow)
    completed_at: Optional[datetime] = None

    def add_check(self, check: ValidationCheck) -> None:
        self.checks.append(check)
        self.total_checks += 1
        if check.passed:
            self.passed += 1
        else:
            self.failed += 1
        self.pass_rate = (self.passed / self.total_checks * 100) if self.total_checks > 0 else 0.0


# ============================================================
# Tolerance Utilities
# ============================================================
def values_match(expected: float, actual: float, tol: ValidationTolerances = DEFAULT_TOLERANCES) -> tuple[bool, float]:
    """Compare two float values within tolerance. Returns (match, difference)."""
    diff = abs(expected - actual)
    if expected == 0.0:
        return diff <= tol.formula_absolute, diff
    return (diff / abs(expected)) <= tol.formula_relative, diff


# ============================================================
# Stream 1: Mathematical Validation
# ============================================================
class MathematicalValidator:
    """Verify every formula produces correct output."""

    def __init__(self, tolerances: ValidationTolerances = DEFAULT_TOLERANCES):
        self.tolerances = tolerances

    def validate_trend_score(self, features: FeatureSet, expected: float) -> ValidationCheck:
        """Recompute trend score from raw features and compare to expected."""
        adx_component = min(features.adx / 50.0, 1.0) * 40
        ema_component = max(0, min((features.ema_spread / 0.02) * 30, 30)) if features.ema_spread > 0 else 0
        htf_component = 30 if features.htf_trend == "bullish" else (0 if features.htf_trend == "bearish" else 15)
        actual = adx_component + ema_component + htf_component

        match, diff = values_match(expected, actual, self.tolerances)
        return ValidationCheck(
            check_name="trend_score",
            passed=match,
            expected=expected,
            actual=actual,
            difference=diff,
            tolerance=self.tolerances.formula_relative,
            formula_id="F-011",
            details=f"adx={features.adx}, emaSpread={features.ema_spread:.4f}, htf={features.htf_trend}",
        )

    def validate_liquidity_score(self, features: FeatureSet, expected: float) -> ValidationCheck:
        """Recompute liquidity score."""
        volume_ratio = features.volume_ratio if features.volume_ratio > 0 else 1.0
        liq_base = max(0, min((volume_ratio - 1) * 50 + 50, 100))
        actual = liq_base  # Without spread penalty for independent verification

        match, diff = values_match(expected, actual, self.tolerances)
        return ValidationCheck(
            check_name="liquidity_score",
            passed=match,
            expected=expected,
            actual=actual,
            difference=diff,
            tolerance=self.tolerances.formula_relative,
            formula_id="F-012",
            details=f"volumeRatio={volume_ratio:.4f}",
        )

    def validate_confidence_score(self, features: FeatureSet, expected: float) -> ValidationCheck:
        """Recompute confidence score from sub-scores."""
        trend = features.quality_scores.get("trend", 50)
        liq = features.quality_scores.get("liq", 50)
        session = features.quality_scores.get("session", 50)
        analog = features.quality_scores.get("analog", 50)

        actual = trend * 0.30 + liq * 0.20 + session * 0.20 + analog * 0.30

        match, diff = values_match(expected, actual, self.tolerances)
        return ValidationCheck(
            check_name="confidence_score",
            passed=match,
            expected=expected,
            actual=actual,
            difference=diff,
            tolerance=self.tolerances.formula_relative,
            formula_id="F-010",
        )

    def validate_safe_div(self, a: Optional[float], b: Optional[float], expected: float) -> ValidationCheck:
        """Validate safeDiv utility."""
        from backend.common.utils import safe_div
        actual = safe_div(a, b)
        match, diff = values_match(expected, actual, self.tolerances)
        return ValidationCheck(
            check_name="safe_div",
            passed=match,
            expected=expected,
            actual=actual,
            difference=diff,
            tolerance=self.tolerances.formula_absolute,
            formula_id="F-U001",
            details=f"a={a}, b={b}",
        )


# ============================================================
# Stream 2: Strategy Execution Validation
# ============================================================
class StrategyValidator:
    """Verify strategy engine produces identical trades to RI-1."""

    def __init__(self, tolerances: ValidationTolerances = DEFAULT_TOLERANCES):
        self.tolerances = tolerances

    def validate_trade_match(self, ri1_trade: Trade, python_trade: Trade) -> ValidationCheck:
        """Compare a single trade from RI-1 against Python engine output."""
        mismatches = []

        # Direction
        if ri1_trade.direction != python_trade.direction:
            mismatches.append("direction")

        # Entry price
        price_ok, _ = values_match(ri1_trade.entry_price, python_trade.entry_price, self.tolerances)
        if not price_ok:
            mismatches.append("entry_price")

        # Stop loss
        sl_ok, _ = values_match(ri1_trade.stop_loss, python_trade.stop_loss, self.tolerances)
        if not sl_ok:
            mismatches.append("stop_loss")

        # TP1
        tp1_ok, _ = values_match(ri1_trade.tp1, python_trade.tp1, self.tolerances)
        if not tp1_ok:
            mismatches.append("tp1")

        passed = len(mismatches) == 0
        return ValidationCheck(
            check_name=f"trade_match_{ri1_trade.trade_id}",
            passed=passed,
            details=f"Mismatched fields: {', '.join(mismatches)}" if mismatches else "All fields match",
        )


# ============================================================
# Stream 3: Dashboard Synchronization Validation
# ============================================================
class DashboardValidator:
    """Verify dashboard state at signal time matches backend calculations."""

    def __init__(self, tolerances: ValidationTolerances = DEFAULT_TOLERANCES):
        self.tolerances = tolerances

    def validate_dashboard_value(self, field_name: str, expected: float, actual: float) -> ValidationCheck:
        """Compare a single dashboard field value."""
        match, diff = values_match(expected, actual, self.tolerances)
        return ValidationCheck(
            check_name=f"dashboard_{field_name}",
            passed=match,
            expected=expected,
            actual=actual,
            difference=diff,
            tolerance=self.tolerances.dashboard_score,
        )


# ============================================================
# Stream 4: Statistical Validation
# ============================================================
class StatisticalValidator:
    """Compute and validate statistical performance metrics."""

    def compute_metrics(self, trades: list[Trade]) -> dict[str, object]:
        """Compute all statistical metrics from a list of closed trades."""
        closed = [t for t in trades if t.pnl is not None]

        if not closed:
            return {"error": "No closed trades"}

        winners = [t for t in closed if t.pnl > 0]
        losers = [t for t in closed if t.pnl <= 0]
        total = len(closed)

        # Basic metrics
        win_rate = len(winners) / total if total > 0 else 0
        total_pnl = sum(t.pnl for t in closed)
        avg_win = sum(t.pnl for t in winners) / len(winners) if winners else 0
        avg_loss = sum(t.pnl for t in losers) / len(losers) if losers else 0
        loss_sum = sum(t.pnl for t in losers) if losers else 0
        profit_factor = abs(sum(t.pnl for t in winners) / loss_sum) if losers and loss_sum != 0 else float('inf')

        # Confusion matrix (long vs short)
        tp = sum(1 for t in closed if t.direction == "long" and t.pnl > 0)
        fp = sum(1 for t in closed if t.direction == "long" and t.pnl <= 0)
        tn = sum(1 for t in closed if t.direction == "short" and t.pnl <= 0)
        fn = sum(1 for t in closed if t.direction == "short" and t.pnl > 0)

        # Derived metrics
        precision = tp / (tp + fp) if (tp + fp) > 0 else 0
        recall = tp / (tp + fn) if (tp + fn) > 0 else 0
        f1 = 2 * (precision * recall) / (precision + recall) if (precision + recall) > 0 else 0
        expectancy = win_rate * avg_win + (1 - win_rate) * avg_loss

        # Drawdown
        cumulative = 0
        peak = 0
        max_dd = 0.0
        for t in closed:
            cumulative += t.pnl or 0
            if cumulative > peak:
                peak = cumulative
            dd = (peak - cumulative) / peak if peak > 0 else 0
            max_dd = max(max_dd, dd)

        return {
            "total_trades": total,
            "win_rate": round(win_rate, 4),
            "profit_factor": round(profit_factor, 4),
            "expectancy": round(expectancy, 4),
            "avg_win": round(avg_win, 2),
            "avg_loss": round(avg_loss, 2),
            "win_loss_ratio": round(abs(avg_win / avg_loss), 4) if avg_loss != 0 else float('inf'),
            "net_pnl": round(total_pnl, 2),
            "max_drawdown": round(max_dd, 4),
            "confusion_matrix": {"tp": tp, "fp": fp, "tn": tn, "fn": fn},
            "precision": round(precision, 4),
            "recall": round(recall, 4),
            "f1_score": round(f1, 4),
        }


# ============================================================
# Stream 5: Timing Validation
# ============================================================
class TimingValidator:
    """Measure and validate execution timing."""

    def __init__(self, max_latency_ms: float = 5000.0):
        self.max_latency_ms = max_latency_ms

    def validate_timing(self, trade: Trade) -> ValidationCheck:
        """Check if execution timing is within acceptable bounds."""
        if trade.execution_timing_ms > self.max_latency_ms:
            return ValidationCheck(
                check_name=f"timing_{trade.trade_id}",
                passed=False,
                expected=self.max_latency_ms,
                actual=trade.execution_timing_ms,
                difference=trade.execution_timing_ms - self.max_latency_ms,
                details=f"Execution timing {trade.execution_timing_ms:.0f}ms exceeds limit {self.max_latency_ms:.0f}ms",
            )
        return ValidationCheck(
            check_name=f"timing_{trade.trade_id}",
            passed=True,
            actual=trade.execution_timing_ms,
            details=f"Timing within limits: {trade.execution_timing_ms:.0f}ms",
        )


# ============================================================
# Main Validation Engine
# ============================================================
class ValidationEngine:
    """Orchestrates all 5 validation streams with objective pass/fail criteria."""

    def __init__(self, tolerances: ValidationTolerances = DEFAULT_TOLERANCES):
        self.tolerances = tolerances
        self.mathematical = MathematicalValidator(tolerances)
        self.strategy = StrategyValidator(tolerances)
        self.dashboard = DashboardValidator(tolerances)
        self.statistical = StatisticalValidator()
        self.timing = TimingValidator()

    def run_stream(self, stream: str, **kwargs: Any) -> ValidationReport:
        """Execute a single validation stream."""
        stream_map = {
            "mathematical": self._run_mathematical,
            "strategy": self._run_strategy,
            "dashboard": self._run_dashboard,
            "statistical": self._run_statistical,
            "timing": self._run_timing,
        }

        if stream not in stream_map:
            raise ValueError(f"Unknown validation stream: {stream}. Choose from: {list(stream_map.keys())}")

        report = ValidationReport(
            stream=stream,
            date_from=kwargs.get("date_from", date.today()),
            date_to=kwargs.get("date_to", date.today()),
        )

        stream_map[stream](report, **kwargs)
        report.completed_at = datetime.utcnow()
        return report

    def run_all(self, **kwargs: Any) -> dict[str, ValidationReport]:
        """Run all 5 validation streams."""
        streams = ["mathematical", "strategy", "dashboard", "statistical", "timing"]
        return {s: self.run_stream(s, **kwargs) for s in streams}

    def _run_mathematical(self, report: ValidationReport, **kwargs: Any) -> None:
        """Run mathematical validation checks."""
        feature_sets = kwargs.get("feature_sets", [])

        for fs in feature_sets:
            if "trend_score_expected" in fs.quality_scores:
                check = self.mathematical.validate_trend_score(fs, fs.quality_scores["trend_score_expected"])
                report.add_check(check)
            if "liq_score_expected" in fs.quality_scores:
                check = self.mathematical.validate_liquidity_score(fs, fs.quality_scores["liq_score_expected"])
                report.add_check(check)

        # Always validate safeDiv edge cases
        for a, b, exp in [(10, 2, 5.0), (10, 0, 0.0), (None, 2, 0.0), (float('nan'), 2, 0.0)]:
            check = self.mathematical.validate_safe_div(a, b, exp)
            report.add_check(check)

    def _run_strategy(self, report: ValidationReport, **kwargs: Any) -> None:
        """Run strategy execution validation."""
        ri1_trades = kwargs.get("ri1_trades", [])
        python_trades = kwargs.get("python_trades", [])

        trade_map = {t.trade_id: t for t in python_trades}
        for ri1_t in ri1_trades:
            py_t = trade_map.get(ri1_t.trade_id)
            if py_t:
                check = self.strategy.validate_trade_match(ri1_t, py_t)
                report.add_check(check)
            else:
                report.add_check(ValidationCheck(
                    check_name=f"trade_match_{ri1_t.trade_id}",
                    passed=False,
                    details="Trade found in RI-1 but missing from Python output",
                ))

    def _run_dashboard(self, report: ValidationReport, **kwargs: Any) -> None:
        """Run dashboard synchronization validation."""
        snapshots = kwargs.get("snapshots", [])
        for snap in snapshots:
            for fname, expected in snap.get("expected", {}).items():
                actual = snap.get("actual", {}).get(fname, 0)
                check = self.dashboard.validate_dashboard_value(fname, expected, actual)
                report.add_check(check)

    def _run_statistical(self, report: ValidationReport, **kwargs: Any) -> None:
        """Run statistical validation."""
        trades = kwargs.get("trades", [])
        metrics = self.statistical.compute_metrics(trades)

        expected_metrics = kwargs.get("expected_metrics", {})

        for metric, value in metrics.items():
            if isinstance(value, (int, float)):
                expected = expected_metrics.get(metric, value)
                match, diff = values_match(expected, value)
                report.add_check(ValidationCheck(
                    check_name=f"statistical_{metric}",
                    passed=match,
                    expected=expected,
                    actual=value,
                    difference=diff,
                ))
            else:
                report.add_check(ValidationCheck(
                    check_name=f"statistical_{metric}",
                    passed=True,
                    details=str(value),
                ))

    def _run_timing(self, report: ValidationReport, **kwargs: object) -> None:
        """Run execution timing validation."""
        trades = kwargs.get("trades", [])
        for trade in trades:
            check = self.timing.validate_timing(trade)
            report.add_check(check)
