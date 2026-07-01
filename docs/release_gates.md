# Release Gates

Explicit acceptance criteria for every release stage.

## Gate 1: Alpha

**Purpose**: Verify the module compiles and basic functionality exists.

**Criteria**:
- [ ] All modules compile without errors
- [ ] Unit tests pass (pytest)
- [ ] Static analysis passes (ruff, mypy)
- [ ] Documentation exists for the module
- [ ] At least one smoke test demonstrates the feature

**Failure**: Do not proceed to Beta until all criteria met.

---

## Gate 2: Beta

**Purpose**: Verify the feature works correctly and can be validated.

**Criteria**:
- [ ] Integration tests pass
- [ ] Validation framework operational (can run all 5 streams)
- [ ] Audit score ≥ 70/100
- [ ] Regression tests pass against RI-1 baseline
- [ ] API contracts documented (if new endpoints added)
- [ ] Database migration script written and tested
- [ ] At least 100 historical bars processed without errors

**Failure**: If any criterion fails, investigate and fix before proceeding.

---

## Gate 3: Release Candidate

**Purpose**: Verify production readiness.

**Criteria**:
- [ ] Full regression suite passes (unit + integration + regression + performance)
- [ ] Performance targets met (see Technical Architecture Section 5)
- [ ] Security review complete (no open critical/high findings)
- [ ] Audit score ≥ 85/100
- [ ] All dimensions scored ≥ 70
- [ ] No new findings from the last full audit run
- [ ] Documentation complete (all relevant docs updated)
- [ ] CHANGELOG updated
- [ ] Version bump committed
- [ ] Deployment tested in staging environment

**Failure**: If any criterion fails, address and re-run RC gate.

---

## Gate 4: Production

**Purpose**: Final sign-off for release.

**Criteria**:
- [ ] All RC criteria still passing
- [ ] Validation complete: 5-stream pass rates ≥ 95%
- [ ] RTM updated: all requirements in scope marked accordingly
- [ ] Release notes reviewed and approved
- [ ] Rollback plan documented
- [ ] Monitoring alerts configured
- [ ] Project Owner sign-off obtained

**Failure**: Release blocked until all criteria satisfied.

---

## Summary Table

| Gate | Name | Key Criteria | Blocking |
|------|------|-------------|----------|
| G1 | Alpha | Compiles, unit tests, lint | Yes |
| G2 | Beta | Integration tests, validation, audit ≥ 70 | Yes |
| G3 | RC | Full regression, security, audit ≥ 85, docs | Yes |
| G4 | Production | Validation ≥ 95%, sign-off | Yes |

## Emergency Override

For security-critical fixes, the Security Engineer and Project Owner can jointly approve a direct path from Alpha → Production, but only if:
1. The fix is < 50 lines changed
2. Tests cover the fix
3. Rollback plan exists
4. Full gates are passed within 24 hours post-release
