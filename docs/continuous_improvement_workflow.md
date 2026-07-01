# Continuous Improvement Workflow

Formal pipeline for proposing, approving, implementing, and verifying changes.

## Workflow

```
Observation
    │ (any source: audit finding, validation failure, trader feedback, research)
    ▼
Evidence Collection
    │ (data, logs, metrics, affected formulas, affected requirements)
    ▼
Validation
    │ (reproduce the issue or opportunity independently)
    ▼
Audit Finding
    │ (formal finding with severity, impact, root cause)
    ▼
Improvement Proposal
    │ (what to change, expected improvement, risk assessment)
    ▼
Approval
    │ (Project Owner or System Architect depending on severity)
    ▼
Implementation
    │ (code change, test addition, documentation update)
    ▼
Regression Tests
    │ (full suite: unit, integration, regression, performance)
    ▼
Release
    │ (Alpha → Beta → RC → Production gates)
```

## Roles in the Workflow

| Role | Responsibility |
|------|---------------|
| **Observer** | Anyone can submit an observation via PIE |
| **Validator** | QA Engineer validates the observation is reproducible |
| **Proposer** | Quant Developer or Python Engineer authors the improvement proposal |
| **Approver** | Project Owner (any change) or System Architect (technical changes) |
| **Implementer** | Engineer assigned to the change |
| **Verifier** | QA Engineer runs regression suite and signs off |

## Proposal Template

Every improvement proposal must include:

```yaml
title: "Short description of the change"
observation: "What was observed"
evidence: "Links to logs, metrics, validation reports"
root_cause: "Why does this happen?"
proposed_change: "Exactly what will change"
expected_improvement: "Quantified benefit (e.g., +5% pass rate, -200ms latency)"
risk_assessment: "What could go wrong and how it's mitigated"
affected_formulas: ["F-001", "F-002"]
affected_requirements: ["R-001", "R-002"]
regression_plan: "Which tests verify no degradation"
```

## Approval Matrix

| Change Type | Approver | Auto-approvable? |
|-------------|----------|------------------|
| Bug fix (PATCH) | Any CODEOWNER | Yes, if test coverage ≥ 90% |
| New feature (MINOR) | System Architect | No |
| Breaking change (MAJOR) | Project Owner | No |
| Documentation | Technical Writer | Yes |
| Dependency update | DevOps Engineer | Yes, if CI passes |
| Security fix | Security Engineer | Yes (emergency override) |

## Prohibited Changes (Require Project Owner)

- Changing validated RI-1 formulas without preserving the original as a versioned option
- Removing or weakening risk controls
- Changing audit scoring weights
- Adding new data sources without data quality assessment
