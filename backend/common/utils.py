"""Quantum Trading Platform — Common Utilities."""

from math import isfinite


def safe_div(a: float | None, b: float | None) -> float:
    """Safe division guarding against None, NaN, and zero denominator."""
    if a is None or b is None:
        return 0.0
    if not isfinite(a) or not isfinite(b):
        return 0.0
    if b == 0.0:
        return 0.0
    return a / b


def clamp(value: float, lo: float, hi: float) -> float:
    """Clamp value to [lo, hi] range."""
    return max(lo, min(value, hi))
