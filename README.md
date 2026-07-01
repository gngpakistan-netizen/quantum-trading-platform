# XAUUSD Quantum Platform (QTOS) v4.0

Reference Implementation: v3.3.0-ri1

## Setup
1. pip install -r requirements-dev.txt
2. Set up Supabase database
3. Deploy Cloudflare Worker
4. Run: uvicorn backend.api_gateway.main:app --reload

git tag v3.3.0-ri1

## Quick Start
pip install -r requirements-dev.txt
uvicorn backend.api_gateway.main:app --reload --port 8000
