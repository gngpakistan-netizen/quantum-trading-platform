"""Quantum Trading Platform — Common Configuration.
Deployment-agnostic: works in Codespaces, Replit, Docker, or cloud VM.
All config via environment variables — zero hardcoded paths.
"""

from pydantic_settings import BaseSettings


class Settings(BaseSettings):
    """Application settings loaded from environment variables."""

    # API
    api_key: str = "dev-key"
    api_host: str = "0.0.0.0"
    api_port: int = 8000
    log_level: str = "DEBUG"
    environment: str = "development"

    # Supabase (primary DB for cloud-native)
    supabase_url: str = ""
    supabase_key: str = ""

    # PostgreSQL (fallback / local dev)
    database_url: str = "postgresql://user:pass@localhost:5432/quantum"

    # Cache
    redis_url: str = "redis://localhost:6379/0"

    # Engine toggles
    enable_validation_engine: bool = True
    enable_audit_engine: bool = True
    enable_forecast_engine: bool = False
    enable_learning_engine: bool = False

    # Trading
    account_size: float = 100000.0
    max_position_pct: float = 0.05
    default_risk_percent: float = 1.0
    max_portfolio_heat: float = 0.50
    max_daily_loss: float = -500.0
    max_weekly_loss: float = -1500.0
    max_drawdown: float = -0.15

    # Market
    xauusd_typical_spread: float = 0.3
    commission_per_100k: float = 5.0
    default_slippage_pips: float = 0.5

    model_config = {"env_file": ".env", "env_file_encoding": "utf-8"}

    @property
    def is_cloud(self) -> bool:
        """Returns True if running with Supabase (cloud-native mode)."""
        return bool(self.supabase_url and self.supabase_key)


settings = Settings()
