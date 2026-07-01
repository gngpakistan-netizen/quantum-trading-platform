# XAUUSD Quantum Platform — Project Intelligence Engine (PIE)

## Document Control
| Field | Value |
|-------|-------|
| Document ID | QTP-PIE-001 |
| Version | 1.0 |
| Status | DRAFT |

## 1. Purpose

The Project Intelligence Engine (PIE) is the platform's self-awareness layer. It tracks every requirement, bug, audit finding, validation run, formula change, API contract, and deployment. PIE replaces scattered TODO lists, issue trackers, and tribal knowledge with a structured, queryable, automated project memory.

## 2. What PIE Tracks

### 2.1 Requirements
- Every RTM entry (ID, origin, description, status, backend module, test status)
- Requirement dependencies
- Change history for each requirement
- Links to implementation files and test files

### 2.2 TODO Items
- Every todo with status (pending/in_progress/completed/cancelled)
- Priority, owner, deadline, dependencies
- Links to related requirements and audit findings

### 2.3 Audit Findings
- Every audit run: score per dimension, recommendations
- Every finding: severity, description, affected modules
- Finding lifecycle: open → investigating → resolved → verified

### 2.4 Validation Runs
- Every validation run: stream, date range, bars processed
- Results: pass/fail per check, discrepancy details
- Trend: is validation getting better or worse over time?

### 2.5 Formula Registry
- Every formula: ID, name, expression, source (RI-1 line), validation status
- Formula dependency graph
- Change history with before/after diff

### 2.6 API Contracts
- Every endpoint: method, path, request schema, response schema
- Contract version history
- Breaking change detection

### 2.7 Database Schema
- Every table and column with documentation
- Migration history
- Schema drift detection

### 2.8 Releases
- Every release: version, date, features, requirements satisfied
- Release notes generated from tracked items

## 3. PIE Implementation

### 3.1 Storage
All PIE data lives in Supabase tables:

```sql
-- Core tables
pie_requirements      -- RTM entries
pie_todos              -- TODO items
pie_audit_findings     -- Audit findings
pie_validation_runs    -- Validation execution records
pie_formulas           -- Formula registry
pie_api_contracts      -- API endpoint definitions
pie_schema_versions    -- Database schema version tracking
pie_releases           -- Release history

-- Link tables
pie_requirement_dependencies
pie_todo_requirements
pie_finding_affected_modules
```

### 3.2 CLI Interface
```bash
# Status queries
pie status                    # Current project health summary
pie requirements              # List all requirements with status
pie requirements --status pending  # Filter by status

# TODO management
pie todo list                 # List all open TODOs
pie todo add "Build X" --priority high
pie todo done <id>

# Audit
pie audit run                 # Run full audit
pie audit latest              # Show latest audit report

# Validation
pie validation run --stream mathematical
pie validation results --last

# Release
pie release create v4.0.1 --requirements R-001,R-002

# Dashboard
pie dashboard                 # Full project health dashboard
```

### 3.3 API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| GET | `/pie/status` | Project health summary |
| GET | `/pie/requirements` | List requirements with filters |
| PUT | `/pie/requirements/{id}` | Update requirement status |
| GET | `/pie/audit/latest` | Latest audit report |
| POST | `/pie/validation/run` | Trigger validation |
| GET | `/pie/todos` | List open TODOs |
| POST | `/pie/todos` | Create TODO |
| PUT | `/pie/todos/{id}` | Update TODO status |

### 3.4 Automatic Triggers
PIE automatically records:
- Every `git push` → release candidate
- Every PR merge → requirement status update
- Every audit run → finding creation
- Every validation run → result storage
- Every formula change → version bump + diff log

## 4. PIE Commands (Implementation Plan)

PIE is implemented as a Python CLI package (`pie/`) that can run in any environment:

```
backend/pie/
├── __init__.py
├── cli.py                   # Click/typer CLI
├── tracker.py               # Core tracking logic
├── models.py                # Pydantic models for PIE data
├── storage.py               # Supabase/PostgreSQL storage
├── reporter.py              # Report generation
├── requirements.py          # Requirements management
├── todos.py                 # TODO management
├── audit.py                 # Audit integration
└── validation.py            # Validation integration
```

## 5. Project Health Dashboard

PIE generates a single-page health summary:

```
╔══════════════════════════════════════════════════════════╗
║  XAUUSD Quantum Platform — Project Health                ║
║  2026-07-01 12:00:00 UTC                                 ║
╠══════════════════════════════════════════════════════════╣
║  REQUIREMENTS: 29 total  18 done  6 in prog  5 pending  ║
║  TODOS:        12 total   8 done  3 in prog  1 pending  ║
║  AUDIT:        96.8/100  (last: 2026-07-01)             ║
║  VALIDATION:   5/5 streams passing                      ║
║  FORMULAS:     25 total  22 verified  3 pending         ║
║  RELEASES:     v4.0.0 (current)                         ║
║  BUILD:        passing (last: 2026-07-01)                ║
╚══════════════════════════════════════════════════════════╝
```
