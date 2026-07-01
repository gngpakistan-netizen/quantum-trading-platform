# ADR-002: Supabase as Primary Database

## Status

ACCEPTED — 2026-07-01

## Context

The platform needs a relational database with time-series support, real-time subscriptions, and a free tier.

## Decision

Use Supabase (managed PostgreSQL with TimescaleDB extension). All state — market data, features, signals, trades, audit logs, project intelligence — lives in Supabase.

## Consequences

Positive:
- PostgreSQL compatibility (migrate to any Postgres provider later)
- Built-in auth, RLS, real-time, storage
- Free tier covers development and early production

Negative:
- 500MB storage limit on free tier
- No native TimescaleDB on Supabase free tier (requires manual partitioning for time-series)
- Query performance degrades beyond certain data volumes without indexing

## Supersedes

None (initial decision).
