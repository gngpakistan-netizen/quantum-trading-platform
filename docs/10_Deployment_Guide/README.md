# XAUUSD Quantum Platform — Deployment Guide

## Document Control
| Field | Value |
|-------|-------|
| Document ID | QTP-DG-001 |
| Version | 1.0 |
| Status | DRAFT |

## 1. Development Environment

### 1.1 Prerequisites
- Python 3.11+
- PostgreSQL 15+ with TimescaleDB extension
- Redis 7+
- Docker Desktop
- Git

### 1.2 Setup
```bash
# Clone
git clone https://github.com/yourorg/quantum-trading-platform.git
cd quantum-trading-platform

# Virtual environment
python -m venv .venv
source .venv/bin/activate  # Windows: .venv\Scripts\Activate.ps1

# Install dependencies
pip install -r requirements.txt

# Environment config
cp .env.example .env
# Edit .env with your settings

# Database
docker compose up -d db redis
alembic upgrade head

# Run
uvicorn backend.api_gateway.main:app --reload
```

### 1.3 Docker Compose (Development)
```yaml
services:
  api:
    build: .
    ports: ["8000:8000"]
    depends_on: [db, redis]
    environment:
      - DATABASE_URL=postgresql://user:pass@db:5432/quantum
      - REDIS_URL=redis://redis:6379
  db:
    image: timescale/timescaledb:latest-pg15
    environment:
      - POSTGRES_USER=user
      - POSTGRES_PASSWORD=pass
      - POSTGRES_DB=quantum
    volumes: [pgdata:/var/lib/postgresql/data]
  redis:
    image: redis:7-alpine
```

## 2. CI/CD Pipeline (GitHub Actions)

### 2.1 Workflows

| Workflow | Trigger | Actions |
|----------|---------|---------|
| `ci.yml` | Push, PR | Lint, type-check, unit tests, integration tests, regression tests |
| `nightly.yml` | Daily 00:00 UTC | Full test suite + performance benchmarks + validation run |
| `release.yml` | Tag v*.*.* | Build Docker image, push to registry, deploy to staging |

### 2.2 CI Steps
```yaml
jobs:
  test:
    runs-on: ubuntu-latest
    services:
      postgres:
        image: timescale/timescaledb:latest-pg15
        env: { POSTGRES_PASSWORD: testpass }
      redis:
        image: redis:7-alpine
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-python@v5
        with: { python-version: "3.11" }
      - run: pip install -r requirements-dev.txt
      - run: ruff check .
      - run: mypy backend/
      - run: pytest tests/unit tests/integration
      - run: pytest tests/regression
```

## 3. Production Deployment

### 3.1 Infrastructure
```
Load Balancer → API Servers (×2 min, auto-scale)
                    ↓
            PostgreSQL (Primary + Replica)
            Redis (Cluster)
            RabbitMQ (Cluster)
```

### 3.2 Environment Variables
```
DATABASE_URL=postgresql://user:pass@host:5432/quantum
REDIS_URL=redis://host:6379
RABBITMQ_URL=amqp://user:pass@host:5672
API_KEY=your-api-key
LOG_LEVEL=INFO
ENVIRONMENT=production
```

### 3.3 Health Checks
- `/health`: Returns OK if all engines are healthy
- Database connection pool health
- Redis connectivity
- Queue consumer health

### 3.4 Monitoring (Grafana + Prometheus)
- API request rate, latency (p50, p95, p99), error rate
- Engine processing times
- Database query performance
- Queue depth
- System resources (CPU, memory, disk)

### 3.5 Alerting
| Alert | Threshold | Action |
|-------|-----------|--------|
| API p95 > 500ms | 5 min | Page on-call |
| Error rate > 5% | 1 min | Page on-call |
| Queue depth > 1000 | 5 min | Investigate bottleneck |
| Database connection > 80% | 10 min | Scale connection pool |
| Validation failure | Immediate | Block deployment |

## 4. Rollback Procedure

1. Revert to previous Docker image tag
2. Run database migration rollback: `alembic downgrade -1`
3. Verify health endpoint
4. Run validation smoke test
5. Monitor for 15 minutes

## 5. Backup Strategy

- Database: pg_dump daily, WAL archiving continuous
- Feature store: Parquet export weekly
- Configuration: Git-backed, immutable
- Model registry: Versioned in database
- Audit logs: Append-only, backed up daily

## 6. Release Process

1. Create release branch from `main`
2. Run full test suite + validation
3. Create GitHub release with tag `v4.x.x`
4. CI builds Docker image
5. Deploy to staging, run E2E tests
6. Deploy to production (rolling update)
7. Monitor for 30 minutes
8. Announce release
