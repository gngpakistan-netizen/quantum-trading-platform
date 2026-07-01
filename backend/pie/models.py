"""Project Intelligence Engine (PIE) — Core data models."""

from dataclasses import dataclass, field
from datetime import date, datetime
from typing import Optional
from uuid import UUID, uuid4


@dataclass
class Requirement:
    id: str
    origin: str
    description: str
    status: str = "draft"              # draft, specified, implemented, validated, audited, operational
    backend_module: Optional[str] = None
    test_status: str = "pending"
    notes: Optional[str] = None
    dependencies: list[str] = field(default_factory=list)
    created_at: datetime = field(default_factory=datetime.utcnow)
    updated_at: datetime = field(default_factory=datetime.utcnow)


@dataclass
class TodoItem:
    id: UUID = field(default_factory=uuid4)
    content: str = ""
    status: str = "pending"            # pending, in_progress, completed, cancelled
    priority: str = "medium"           # high, medium, low
    owner: Optional[str] = None
    requirement_ids: list[str] = field(default_factory=list)
    created_at: datetime = field(default_factory=datetime.utcnow)
    completed_at: Optional[datetime] = None


@dataclass
class AuditScore:
    overall: float = 0.0
    mathematical_correctness: float = 0.0
    statistical_validity: float = 0.0
    dashboard_consistency: float = 0.0
    timing_integrity: float = 0.0
    risk_management: float = 0.0
    code_quality: float = 0.0
    recommendations: list[str] = field(default_factory=list)
    run_timestamp: datetime = field(default_factory=datetime.utcnow)


@dataclass
class ValidationRun:
    stream: str                        # mathematical, strategy, dashboard, statistical, timing
    date_from: date
    date_to: date
    bars_processed: int = 0
    status: str = "running"            # running, completed, failed
    overall_pass_rate: Optional[float] = None
    started_at: datetime = field(default_factory=datetime.utcnow)
    completed_at: Optional[datetime] = None


@dataclass
class FormulaEntry:
    id: str
    name: str
    expression: str
    source: Optional[str] = None       # RI-1 line reference
    validation_status: str = "unverified"
    dependencies: list[str] = field(default_factory=list)
    version: int = 1


@dataclass
class Release:
    version: str
    release_date: date = field(default_factory=date.today)
    requirement_ids: list[str] = field(default_factory=list)
    notes: Optional[str] = None


# RTM data (in-memory for CLI, persisted to DB in production)
RTM_REQUIREMENTS: dict[str, Requirement] = {}
PIE_TODOS: list[TodoItem] = []
