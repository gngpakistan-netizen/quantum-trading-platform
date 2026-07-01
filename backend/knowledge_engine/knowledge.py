"""Knowledge Engine — Central repository for formulas, requirements, audit history, and decisions.

Provides queryable access to all institutional knowledge. Immutable audit trail.
"""

from dataclasses import dataclass, field
from datetime import datetime
from typing import Optional
from uuid import UUID, uuid4

from backend.pie.models import FormulaEntry, Requirement


@dataclass
class DecisionRecord:
    """An immutable record of a decision made during platform evolution."""
    id: UUID = field(default_factory=uuid4)
    timestamp: datetime = field(default_factory=datetime.utcnow)
    title: str = ""
    description: str = ""
    rationale: str = ""
    alternatives: list[str] = field(default_factory=list)
    chosen_approach: str = ""
    affected_requirements: list[str] = field(default_factory=list)
    affected_formulas: list[str] = field(default_factory=list)
    author: str = ""


@dataclass
class KnowledgeSnapshot:
    """Point-in-time snapshot of all platform knowledge."""
    timestamp: datetime = field(default_factory=datetime.utcnow)
    requirements: dict[str, Requirement] = field(default_factory=dict)
    formulas: dict[str, FormulaEntry] = field(default_factory=dict)
    decisions: list[DecisionRecord] = field(default_factory=list)
    audit_scores: list[dict] = field(default_factory=list)
    version: str = ""


class KnowledgeEngine:
    """Central knowledge repository. All access is read-only; mutations go through audit."""

    def __init__(self):
        self._formulas: dict[str, FormulaEntry] = {}
        self._requirements: dict[str, Requirement] = {}
        self._decisions: list[DecisionRecord] = []
        self._audit_history: list[dict[str, object]] = []

    # ============================================================
    # Formulas
    # ============================================================
    def register_formula(self, entry: FormulaEntry):
        """Register or update a formula (creates new version)."""
        existing = self._formulas.get(entry.id)
        if existing:
            entry.version = existing.version + 1
        self._formulas[entry.id] = entry

    def get_formula(self, formula_id: str) -> Optional[FormulaEntry]:
        return self._formulas.get(formula_id)

    def get_all_formulas(self) -> list[FormulaEntry]:
        return list(self._formulas.values())

    def get_formulas_by_status(self, status: str) -> list[FormulaEntry]:
        return [f for f in self._formulas.values() if f.validation_status == status]

    # ============================================================
    # Requirements (RTM)
    # ============================================================
    def register_requirement(self, req: Requirement):
        self._requirements[req.id] = req

    def get_requirement(self, req_id: str) -> Optional[Requirement]:
        return self._requirements.get(req_id)

    def get_all_requirements(self) -> list[Requirement]:
        return list(self._requirements.values())

    def get_requirements_by_status(self, status: str) -> list[Requirement]:
        return [r for r in self._requirements.values() if r.status == status]

    def get_requirements_by_module(self, module: str) -> list[Requirement]:
        return [r for r in self._requirements.values() if r.backend_module == module]

    # ============================================================
    # Decisions
    # ============================================================
    def record_decision(self, decision: DecisionRecord):
        """Record a decision. Immutable once added."""
        self._decisions.append(decision)

    def get_decisions(self, limit: int = 20) -> list[DecisionRecord]:
        return sorted(self._decisions, key=lambda d: d.timestamp, reverse=True)[:limit]

    def get_decisions_affecting(self, requirement_id: str) -> list[DecisionRecord]:
        return [d for d in self._decisions if requirement_id in d.affected_requirements]

    # ============================================================
    # Audit History
    # ============================================================
    def record_audit(self, audit_result: dict[str, object]):
        """Store an audit result with timestamp."""
        entry = {
            "timestamp": datetime.utcnow().isoformat(),
            **audit_result,
        }
        self._audit_history.append(entry)

    def get_audit_history(self, limit: int = 10) -> list[dict[str, object]]:
        return self._audit_history[-limit:]

    def get_audit_trend(self, dimension: str = "overall") -> list[float]:
        """Get scores over time for a dimension to see trend."""
        return [float(a.get(dimension, 0)) for a in self._audit_history]

    # ============================================================
    # Reporting
    # ============================================================
    def generate_knowledge_report(self) -> dict[str, object]:
        """Generate a summary of all knowledge."""
        return {
            "formulas": {
                "total": len(self._formulas),
                "verified": len([f for f in self._formulas.values() if f.validation_status == "verified"]),
                "unverified": len([f for f in self._formulas.values() if f.validation_status != "verified"]),
            },
            "requirements": {
                "total": len(self._requirements),
                "implemented": len([r for r in self._requirements.values() if r.status == "implemented"]),
                "validated": len([r for r in self._requirements.values() if r.status == "validated"]),
            },
            "decisions": len(self._decisions),
            "audit_runs": len(self._audit_history),
            "last_audit": self._audit_history[-1] if self._audit_history else None,
        }
