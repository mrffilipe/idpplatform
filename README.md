# IdP Platform

[English](./README.md) | [Português](./README.pt-BR.md)

An **identity and access platform (IdP)** for an ecosystem of applications: centralizes local authentication, issues JWT tokens via OIDC, organizes tenants (organizations), members, roles, OAuth applications, and supports federation with external providers (Firebase, Cognito, etc.).

Inspired by Keycloak-style platforms: a managed, multi-tenant IdP with its own admin console.

---

## Getting started

See **[GETTING_STARTED.md](./GETTING_STARTED.md)** for a full walkthrough to configure and run the system from scratch.

---

## Repository layout

```
backend/    ASP.NET Core 8 API — Clean Architecture (Domain / Application / Infrastructure / API)
            Helper tool: tools/GenerateOidcKey (writes the RSA key into the API folder)
frontend/   Admin SPA — React 19 + MUI + React Router 7 + Vite
samples/    Reference consumer applications (e.g., pulse-crm — SaaS CRM + OIDC)
rules/      Standards and conventions: backend-rules.md, frontend-rules.md
.github/    CI workflows (when configured)
```

---

## Documentation

| Document | Content |
|----------|---------|
| [GETTING_STARTED.md](./GETTING_STARTED.md) | Step-by-step guide: configure, run, and bootstrap from scratch |
| [rules/backend-rules.md](./rules/backend-rules.md) | Backend conventions, formatting, options pattern, secrets, OIDC |
| [rules/frontend-rules.md](./rules/frontend-rules.md) | Frontend conventions, env vars with defaults, OIDC flow |
| [backend/README.md](./backend/README.md) | Backend architecture, configuration, endpoints, migrations, OIDC |
| [frontend/README.md](./frontend/README.md) | Frontend stack, environment variables, OIDC flow, and pages |
| [samples/README.md](./samples/README.md) | Sample consumers and OAuth checklist in the admin console |
| [samples/pulse-crm/README.md](./samples/pulse-crm/README.md) | PulseCRM: how to run and test the OIDC integration |

---

## Product overview

| Concept | Description |
|---------|-------------|
| **Local IdP** | Email + password authentication stored with BCrypt. Configured at bootstrap. |
| **OIDC** | Authorization code + PKCE flow. RS256 tokens. Discovery at `/.well-known/openid-configuration`. |
| **Multi-tenant** | Users belong to multiple tenants with independent roles. |
| **Platform admin** | Users with `prole=plat_admin` manage tenants, applications, and global IdPs. |
| **OAuth applications** | Registry of consumer apps with public (PKCE) or confidential clients. |
| **Identity Providers** | Extensible federation: Local (default), Firebase, Cognito, Generic. |
| **Secret protection** | IdP credentials (Firebase ServiceAccount, WebApiKey) are encrypted at rest via ASP.NET Core Data Protection. |
| **Audit logs** | Event tracking per tenant. |

---

## Quick prerequisites

- .NET 8 SDK
- Node.js (LTS)
- PostgreSQL 14+
- Redis (optional)

For the full installation guide see [GETTING_STARTED.md](./GETTING_STARTED.md).
