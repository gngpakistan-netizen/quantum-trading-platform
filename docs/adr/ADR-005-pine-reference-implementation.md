# ADR-005: Pine Script as Reference Implementation Only

## Status

ACCEPTED — 2026-07-01

## Context

The original Pine Script (RI-1) contains validated trading logic. Continuing development in Pine would hit architectural limits. The backend must reimplement the logic in Python.

## Decision

- V3.3 Pine Script is frozen as Reference Implementation RI-1.
- No further feature development in Pine.
- Only critical bugs found during validation will be patched in V3.3.
- All new capabilities built in Python backend.
- Every Python formula must reproduce RI-1 behavior within tolerance.

## Consequences

Positive:
- Removes Pine's architectural constraints (no portfolio risk, no multi-asset correlation, no execution modeling)
- Enables proper testing, versioning, and CI/CD
- TradingView becomes visualization layer only

Negative:
- Requires full reimplementation of all formulas
- Validation overhead to prove Python matches Pine
- Pine updates from TradingView may require bridge updates

## Supersedes

All prior development in Pine.
