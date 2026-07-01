"""XAUUSD Quantum Trading Platform."""
from setuptools import setup, find_packages

setup(
    name="quantum_trading_platform",
    version="4.0.0",
    packages=find_packages(include=["backend", "backend.*"]),
    install_requires=[
        "fastapi>=0.109",
        "uvicorn[standard]>=0.27",
        "pydantic>=2.5",
        "pydantic-settings>=2.1",
        "pandas>=2.1",
        "numpy>=1.26",
    ],
)
