# Contributing to XAUUSD Quantum Trading Platform

## Development Workflow

1. Create a feature branch from `develop`: `git checkout -b feature/your-feature develop`
2. Make changes following the coding standards
3. Write or update tests
4. Run the full test suite: `pytest`
5. Update documentation
6. Create a pull request to `develop`
7. Ensure CI passes (lint, type-check, unit tests, integration tests)
8. Request review from the appropriate CODEOWNER
9. Merge after approval

## Branching Strategy

- `main` — Production-ready code. Only merged from `release/*` branches.
- `develop` — Integration branch for features. Must pass all tests.
- `feature/*` — New features. Branch from `develop`, merge back to `develop`.
- `fix/*` — Bug fixes. Branch from `develop`, merge back to `develop`.
- `release/*` — Release candidates. Branch from `develop`, merge to `main` and back to `develop`.
- `hotfix/*` — Urgent production fixes. Branch from `main`, merge to `main` and `develop`.

## Quality Gates

| Gate | Requirement |
|------|-------------|
| Pre-commit | `ruff check .`, `mypy backend/` |
| PR to develop | All unit + integration tests pass, coverage ≥ 80% |
| PR to main | Full regression suite passes, audit score ≥ 85 |
| Release | All release gates passed (Alpha → Beta → RC → Production) |

## Coding Standards

- Python 3.11+, type annotations required on all public APIs
- Formatting: `ruff format` (line length 120)
- Imports: standard library → third-party → local (separated by blank line)
- Documentation: all modules, classes, and public functions require docstrings
- Tests: pytest, property-based testing with hypothesis for edge cases

## Commit Messages

Follow conventional commits: `type(scope): description`

- `feat:` — New feature
- `fix:` — Bug fix
- `docs:` — Documentation
- `test:` — Tests
- `refactor:` — Code restructuring
- `perf:` — Performance improvement
- `security:` — Security fix
- `chore:` — Maintenance

## Versioning

Semantic versioning: `MAJOR.MINOR.PATCH`

- MAJOR: Breaking API or schema changes
- MINOR: New features, backward compatible
- PATCH: Bug fixes, backward compatible

## Security

- Report vulnerabilities to SECURITY.md
- Never commit API keys, secrets, or credentials
- All secrets must use environment variables or Cloudflare Workers secrets

## Review Process

1. Automated checks pass (CI)
2. At least one CODEOWNER approval required
3. No unresolved comments
4. All changes traceable to RTM requirements

## Documentation Requirements

Every PR must include or update:
- Relevant documentation files in `/docs/`
- CHANGELOG entry (if user-facing change)
- RTM update (if requirement status changed)
- ADR (if architectural decision was made)
