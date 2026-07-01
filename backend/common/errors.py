"""Quantum Trading Platform — Error Handling."""


class QuantumError(Exception):
    """Base exception for all platform errors."""

    def __init__(self, message: str, code: str = "INTERNAL_ERROR"):
        self.message = message
        self.code = code
        super().__init__(self.message)


class InvalidRequestError(QuantumError):
    """Invalid request parameters."""

    def __init__(self, message: str):
        super().__init__(message, code="INVALID_REQUEST")


class UnauthorizedError(QuantumError):
    """Authentication failure."""

    def __init__(self):
        super().__init__("Invalid or missing API key", code="UNAUTHORIZED")


class RateLimitedError(QuantumError):
    """Rate limit exceeded."""

    def __init__(self):
        super().__init__("Rate limit exceeded", code="RATE_LIMITED")


class EngineUnavailableError(QuantumError):
    """Engine not ready or unavailable."""

    def __init__(self, engine: str):
        super().__init__(f"Engine '{engine}' is unavailable", code="ENGINE_UNAVAILABLE")


class FormulaError(QuantumError):
    """Error during formula computation."""

    def __init__(self, formula_id: str, detail: str):
        super().__init__(f"Formula '{formula_id}' error: {detail}", code="FORMULA_ERROR")


class ValidationError(QuantumError):
    """Validation failure."""

    def __init__(self, stream: str, detail: str):
        super().__init__(f"Validation '{stream}' failed: {detail}", code="VALIDATION_ERROR")
