# ADR-001: Cloud-Native Architecture with Zero-Cost Cloud Stack

## Status

ACCEPTED — 2026-07-01

## Context

The platform requires a persistent, scalable, and auditable infrastructure. Initial development is zero-budget. The architecture must support migration to paid tiers without code changes.

## Decision

Use a three-layer cloud-native stack:

1. **Supabase** (PostgreSQL) — persistent storage, real-time events, auth.
2. **Cloudflare Workers** — API gateway, webhook receiver, edge deployment.
3. **Python engines** — deployment-agnostic, run in Codespaces/Replit during development, container-ready for production.

## Consequences

Positive:
- Zero cost during development
- Each layer can scale independently
- No vendor lock-in at the code level (all config-driven)

Negative:
- Free-tier limits (500MB DB, 100k Workers req/day)
- Workers don't support Python natively (JavaScript for gateway only)
- Replit has compute limits for heavy ML workloads

## Supersedes

None (initial decision).
