# Security Policy

## Supported Versions

| Version | Supported |
|---------|-----------|
| 4.0.x   | ✅ |
| < 4.0   | ❌ (pre-architecture) |

## Reporting a Vulnerability

**Do not open public issues for security vulnerabilities.**

Contact the security team directly:
- Email: security@quantum-trading.com (placeholder)
- PGP key: (to be published)

You should receive a response within 48 hours. If not, escalate to the project owner.

## Disclosure Policy

1. Report received and acknowledged within 48 hours
2. Investigation and validation within 5 business days
3. Fix developed and tested
4. Patch released with security advisory
5. Public disclosure after patch is available

## Security Controls

### Authentication
- All API endpoints require `X-API-Key` header
- Keys are managed via Cloudflare Workers secrets (never in code or config files)
- Keys can be rotated without downtime

### Rate Limiting
- Cloudflare Workers: 100 requests per minute per IP
- Supabase: 60 requests per minute per API key (free tier)

### Input Validation
- All webhook payloads validated against expected schema
- SQL injection protection via parameterized queries
- JSON body size limited to 100KB
- Unexpected fields rejected

### Secrets Management
- No secrets in source code
- No secrets in environment files committed to git
- Cloudflare Workers secrets for production
- Environment variables for local development

### Database
- Supabase Row-Level Security (RLS) enabled on all tables
- Service role key used only in Workers (never exposed to client)
- Least-privilege database user for read-only operations

### CORS
- Restricted to known origins in production
- No credentials in cross-origin requests

## Incident Response

| Severity | Response Time | Escalation |
|----------|---------------|------------|
| CRITICAL | Immediate | Project Owner + Security Engineer |
| HIGH | 1 hour | Security Engineer |
| MEDIUM | 24 hours | Team lead |
| LOW | 5 business days | Normal sprint cycle |
