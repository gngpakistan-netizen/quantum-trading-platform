"""Regression Engine — Compare every commit against RI-1 baseline and previous runs.

Rejects changes that degrade validated behavior without explicit approval.
"""

from dataclasses import dataclass, field

from backend.validation_engine.validator import ValidationReport


@dataclass
class RegressionBound:
    """Defines acceptable degradation thresholds."""
    max_pass_rate_decrease: float = 0.05      # Max 5% pass rate decrease
    max_new_failures: int = 3                  # Max 3 new failing checks
    max_timing_degradation_ms: float = 200.0   # Max 200ms additional latency
    max_score_decrease: float = 5.0            # Max 5 point audit score decrease


DEFAULT_BOUNDS = RegressionBound()


@dataclass
class RegressionResult:
    passed: bool
    baseline: ValidationReport
    current: ValidationReport
    new_failures: list[str] = field(default_factory=list)
    regressions: list[str] = field(default_factory=list)


class RegressionEngine:
    """Automated regression detection against RI-1 and previous baselines."""

    def __init__(self, bounds: RegressionBound = DEFAULT_BOUNDS):
        self.bounds = bounds
        self.baselines: dict[str, ValidationReport] = {}

    def register_baseline(self, stream: str, report: ValidationReport) -> None:
        """Store a validation report as baseline for future comparison."""
        self.baselines[stream] = report

    def compare(self, stream: str, current: ValidationReport) -> RegressionResult:
        """Compare current results against baseline for a validation stream."""
        baseline = self.baselines.get(stream)
        if not baseline:
            return RegressionResult(
                passed=True,
                baseline=ValidationReport(stream=stream, date_from=current.date_from, date_to=current.date_to),
                current=current,
                new_failures=[],
                regressions=["No baseline registered — first run"],
            )

        new_failures = []
        regressions = []

        # Check pass rate degradation
        pass_decrease = baseline.pass_rate - current.pass_rate
        if pass_decrease > self.bounds.max_pass_rate_decrease * 100:
            msg = (
                f"Pass rate decreased {pass_decrease:.1f}% "
                f"(was {baseline.pass_rate:.1f}%, now {current.pass_rate:.1f}%)"
            )
            regressions.append(msg)

        # Check for new failures
        baseline_failed = {c.check_name for c in baseline.checks if not c.passed}
        current_failed = {c.check_name for c in current.checks if not c.passed}
        truly_new = current_failed - baseline_failed

        for name in truly_new:
            new_failures.append(name)

        if len(new_failures) > self.bounds.max_new_failures:
            regressions.append(
                f"{len(new_failures)} new failures exceed limit of {self.bounds.max_new_failures}"
            )

        passed = len(regressions) == 0
        return RegressionResult(
            passed=passed,
            baseline=baseline,
            current=current,
            new_failures=new_failures,
            regressions=regressions,
        )

    def check_commit(self, stream: str, current: ValidationReport) -> tuple[bool, list[str]]:
        """Check if a commit would introduce regressions. Returns (approved, reasons)."""
        result = self.compare(stream, current)
        if result.passed:
            return True, ["No regressions detected"]
        return False, result.regressions
