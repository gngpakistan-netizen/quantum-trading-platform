# Changelog

All notable changes to the XAUUSD Quantum Trading Platform are documented here.

The format follows [Keep a Changelog](https://keepachangelog.com/), and the project adheres to [Semantic Versioning](https://semver.org/).

## [4.0.0] — 2026-07-01

### Added
- Cloud-native architecture: Supabase (PostgreSQL), Cloudflare Workers (API gateway), deployment-agnostic Python engines.
- Complete engineering blueprint: 12 documents covering project charter, architecture, specification, API contracts, data schema, formula dictionary, validation framework, test strategy, deployment guide, audit framework, and PIE.
- Supabase database schema: 15 tables (bars, ticks, feature_sets, signals, trades, audit_log, audit_scores, validation_runs, validation_results, requirements, pie_todos, pie_releases, risk_limits, risk_events, dashboard_snapshots, formula_registry).
- Cloudflare Workers API gateway with TradingView webhook receiver, REST API, API key auth, and input validation.
- Project Intelligence Engine (PIE): CLI for project health, requirements tracking, TODO management, audit integration.
- Validation Engine: 5-stream methodology (mathematical, strategy, dashboard, statistical, timing) with automated pass/fail criteria.
- Audit Engine: 10-dimension weighted scoring with evidence linking.
- Regression Engine: RI-1 baseline comparison across all formulas and trade outputs.
- Knowledge Engine: central repository for formulas, requirements, audit history, and decisions.
- Continuous Improvement Workflow: formal pipeline from observation → evidence → validation → finding → proposal → approval → implementation → regression → release.
- Release Gate system: Alpha, Beta, RC, Production with explicit acceptance criteria per gate.
- Changelog, CODEOWNERS, CONTRIBUTING, SECURITY, LICENSE, ADR directory, branching strategy.

### Changed
- Architecture migrated from monolithic Pine + local Python to cloud-native microservices.
- Configuration switched to deployment-agnostic environment-variable-driven design.
- Database schema designed for time-series optimization (indexes, constraints, migration path).

### Fixed
- All 13 audit findings from RI-1 forensic audit (division-by-zero, RR/TP naming, dead code, header corrections).

### Removed
- Dependency on RabbitMQ / Redis (replaced by Supabase Realtime + Cloudflare KV).
- Hardcoded local paths; all config now environment-driven.

### Security
- API key authentication on all endpoints.
- Rate limiting built into Workers gateway.
- Input validation on all webhook payloads.
- Supabase Row-Level Security (RLS) policies in schema.
- Secrets management via Cloudflare Workers secrets (never in code).

## [3.3.0] — 2026-06-27

### Added
- Reference Implementation RI-1 freeze.
- All 13 audit fixes applied.

[3.3.0]: https://github.com/yourorg/quantum-trading-platform/tree/v3.3.0-ri1
[4.0.0]: https://github.com/yourorg/quantum-trading-platform/tree/v4.0.0
