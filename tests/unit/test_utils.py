"""Tests for common utilities."""

import pytest
import math
from backend.common.utils import safe_div, clamp


class TestSafeDiv:
    def test_normal_division(self):
        assert safe_div(10, 2) == 5.0

    def test_zero_denominator(self):
        assert safe_div(10, 0) == 0.0

    def test_none_numerator(self):
        assert safe_div(None, 2) == 0.0

    def test_none_denominator(self):
        assert safe_div(10, None) == 0.0

    def test_both_none(self):
        assert safe_div(None, None) == 0.0

    def test_nan_numerator(self):
        assert safe_div(float("nan"), 2) == 0.0

    def test_inf_numerator(self):
        assert safe_div(float("inf"), 2) == 0.0

    def test_negative_values(self):
        assert safe_div(-10, 2) == -5.0

    def test_float_precision(self):
        result = safe_div(1, 3)
        assert abs(result - 0.333333) < 1e-5


class TestClamp:
    def test_within_range(self):
        assert clamp(5, 0, 10) == 5

    def test_below_minimum(self):
        assert clamp(-5, 0, 10) == 0

    def test_above_maximum(self):
        assert clamp(15, 0, 10) == 10

    def test_at_boundary(self):
        assert clamp(0, 0, 10) == 0
        assert clamp(10, 0, 10) == 10

    def test_float_values(self):
        assert clamp(5.5, 0.0, 10.0) == 5.5
