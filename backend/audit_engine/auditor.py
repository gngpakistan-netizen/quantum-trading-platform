"""Audit Engine — Central Quality Gate with 10 Weighted Dimensions.

Each dimension scored 0-100. Evidence links every score to specific findings.
Overall score is weighted composite. Scores below threshold trigger alerts.
"""

from dataclasses import dataclass, field
from datetime import datetime
from typing import Optional

from backend.validation_engine.validator import ValidationReport


# ============================================================
# Audit Dimension Configuration
# ============================================================
@dataclass
class AuditDimension:
    name: str
    weight: float                    # Contribution to overall score (0-1)
    score: float = 0.0               # Current score (0-100)
    evidence: list[str] = field(default_factory=list)
    findings: list[str] = field(default_factory=list)
    recommendations: list[str] = field(default_factory=list)

    def add_evidence(self, evidence: str) -> None:
        self.evidence.append(evidence)

    def add_finding(self, finding: str) -> None:
        self.findings.append(finding)

    def add_recommendation(self, rec: str) -> None:
        self.recommendations.append(rec)


# ============================================================
# Audit Engine
# ============================================================
class AuditEngine:
    """10-dimension weighted audit scoring with evidence linking."""

    def __init__(self) -> None:
        self.dimensions: dict[str, AuditDimension] = {
            "architecture": AuditDimension(name="Architecture", weight=0.10),
            "mathematics": AuditDimension(name="Mathematics", weight=0.15),
            "statistics": AuditDimension(name="Statistics", weight=0.10),
            "risk": AuditDimension(name="Risk", weight=0.10),
            "performance": AuditDimension(name="Performance", weight=0.10),
            "security": AuditDimension(name="Security", weight=0.10),
            "reliability": AuditDimension(name="Reliability", weight=0.10),
            "documentation": AuditDimension(name="Documentation", weight=0.10),
            "traceability": AuditDimension(name="Traceability", weight=0.10),
            "testing": AuditDimension(name="Testing", weight=0.05),
        }
        self.run_timestamp: Optional[datetime] = None

    def ingest_validation_report(self, report: ValidationReport) -> None:
        """Incorporate validation results into relevant audit dimensions."""
        # Mathematics dimension
        if report.stream == "mathematical":
            self.dimensions["mathematics"].score = report.pass_rate
            self.dimensions["mathematics"].add_evidence(
                f"Mathematical validation: {report.passed}/{report.total_checks} passed ({report.pass_rate:.1f}%)"
            )
            if report.failed > 0:
                for check in report.checks:
                    if not check.passed:
                        self.dimensions["mathematics"].add_finding(
                            f"Formula {check.formula_id}: expected {check.expected}, got {check.actual}"
                        )

        # Statistics dimension
        elif report.stream == "statistical":
            self.dimensions["statistics"].score = report.pass_rate
            self.dimensions["statistics"].add_evidence(
                f"Statistical validation: {report.passed}/{report.total_checks} passed"
            )

        # Reliability dimension (timing + strategy streams)
        elif report.stream == "timing":
            self.dimensions["reliability"].score = min(
                self.dimensions["reliability"].score + report.pass_rate * 0.5, 100
            ) if self.dimensions["reliability"].score > 0 else report.pass_rate
            self.dimensions["reliability"].add_evidence(
                f"Timing validation: {report.passed}/{report.total_checks} within limits"
            )

        elif report.stream == "strategy":
            self.dimensions["reliability"].score = min(
                self.dimensions["reliability"].score + report.pass_rate * 0.5, 100
            ) if self.dimensions["reliability"].score > 0 else report.pass_rate
            self.dimensions["reliability"].add_evidence(
                f"Strategy validation: {report.passed}/{report.total_checks} trades match RI-1"
            )

        # Traceability dimension
        elif report.stream == "dashboard":
            self.dimensions["traceability"].score = report.pass_rate
            self.dimensions["traceability"].add_evidence(
                f"Dashboard sync: {report.passed}/{report.total_checks} values match"
            )

    def score_architecture(self, has_adrs: bool, has_docs: bool, has_contracts: bool, module_count: int) -> None:
        """Score architecture dimension based on documentation and modularity."""
        score = 0.0
        if has_adrs:
            score += 30.0
        if has_docs:
            score += 25.0
        if has_contracts:
            score += 25.0
        score += min(module_count * 5, 20.0)  # Up to 20 points for modularity
        self.dimensions["architecture"].score = min(score, 100.0)
        self.dimensions["architecture"].add_evidence(
            f"ADRs: {has_adrs}, Docs: {has_docs}, Contracts: {has_contracts}, Modules: {module_count}"
        )

    def score_security(self, has_auth: bool, has_rate_limit: bool, has_validation: bool,
                       has_secrets: bool, has_rls: bool, has_cors: bool) -> None:
        """Score security dimension based on controls present."""
        score = 0.0
        for check, points in [(has_auth, 20), (has_rate_limit, 20), (has_validation, 20),
                               (has_secrets, 15), (has_rls, 15), (has_cors, 10)]:
            if check:
                score += points
        self.dimensions["security"].score = min(score, 100.0)
        self.dimensions["security"].add_evidence(
            f"Auth: {has_auth}, Rate limit: {has_rate_limit}, Validation: {has_validation}, "
            f"Secrets mgmt: {has_secrets}, RLS: {has_rls}, CORS: {has_cors}"
        )

    def score_documentation(self, doc_count: int, has_changelog: bool, has_contributing: bool,
                             has_readme: bool) -> None:
        """Score documentation dimension."""
        score = 0.0
        score += min(doc_count * 5, 50.0)
        for check, points in [(has_changelog, 20), (has_contributing, 15), (has_readme, 15)]:
            if check:
                score += points
        self.dimensions["documentation"].score = min(score, 100.0)
        self.dimensions["documentation"].add_evidence(
            f"Docs: {doc_count}, Changelog: {has_changelog}, Contributing: {has_contributing}, README: {has_readme}"
        )

    def score_traceability(self, rtm_count: int, validated_reqs: int, formula_count: int) -> None:
        """Score traceability based on RTM coverage."""
        coverage = validated_reqs / rtm_count * 100 if rtm_count > 0 else 0
        self.dimensions["traceability"].score = coverage
        self.dimensions["traceability"].add_evidence(
            f"RTM: {rtm_count} requirements, {validated_reqs} validated, {formula_count} formulas"
        )

    def score_testing(self, unit_count: int, integration_count: int, has_regression: bool, has_ci: bool) -> None:
        """Score testing dimension."""
        score = 0.0
        score += min(unit_count, 40)
        score += min(integration_count, 30)
        if has_regression:
            score += 15
        if has_ci:
            score += 15
        self.dimensions["testing"].score = min(score, 100.0)
        self.dimensions["testing"].add_evidence(
            f"Unit: {unit_count}, Integration: {integration_count}, Regression: {has_regression}, CI: {has_ci}"
        )

    def compute_overall(self) -> dict[str, object]:
        """Compute weighted overall audit score and generate recommendations."""
        overall = 0.0
        all_recommendations = []
        all_findings = []

        for dim in self.dimensions.values():
            overall += dim.score * dim.weight
            all_recommendations.extend(dim.recommendations)
            all_findings.extend(dim.findings)

        # Generate recommendations for weak dimensions
        weak_dims = [d for d in self.dimensions.values() if d.score < 70]
        for dim in weak_dims:
            all_recommendations.append(
                f"Improve {dim.name} (current: {dim.score:.0f}/100): review evidence and findings"
            )

        self.run_timestamp = datetime.utcnow()

        return {
            "overall": round(overall, 1),
            "dimensions": {k: {"score": round(v.score, 1), "weight": v.weight}
                          for k, v in self.dimensions.items()},
            "findings_count": len(all_findings),
            "recommendations": list(set(all_recommendations)),
            "run_timestamp": self.run_timestamp.isoformat(),
            "rating": self._rating(overall),
        }

    def _rating(self, score: float) -> str:
        if score >= 95:
            return "EXCELLENT"
        elif score >= 85:
            return "GOOD"
        elif score >= 70:
            return "ACCEPTABLE"
        elif score >= 50:
            return "CONCERN"
        return "CRITICAL"

    def generate_report(self) -> str:
        """Generate a formatted audit report."""
        result = self.compute_overall()

        lines = []
        lines.append("=" * 60)
        ts = self.run_timestamp
        if ts is not None:
            lines.append(f"  AUDIT REPORT — {ts.strftime('%Y-%m-%d %H:%M:%S UTC')}")
        else:
            lines.append(f"  AUDIT REPORT — (no timestamp)")
        lines.append(f"  Overall Score: {result['overall']}/100 [{result['rating']}]")
        lines.append("=" * 60)
        lines.append("")
        lines.append("  Dimensions:")
        for name, info in sorted(result['dimensions'].items()):
            bar = "█" * int(info['score'] / 5) + "░" * (20 - int(info['score'] / 5))
            lines.append(f"    {name:<16s} {info['score']:6.1f} {bar}  (w={info['weight']:.0%})")
        lines.append("")
        lines.append(f"  Findings: {result['findings_count']}")
        lines.append("")
        if result['recommendations']:
            lines.append("  Recommendations:")
            for rec in result['recommendations'][:5]:  # Top 5
                lines.append(f"    → {rec}")
        lines.append("")
        lines.append("=" * 60)

        return "\n".join(lines)
