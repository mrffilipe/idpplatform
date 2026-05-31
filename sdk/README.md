# Kyvo — Product SDK

SDK for **product applications** (SPAs and consumer APIs), not the admin console.

| Package | Role |
|---------|------|
| `@kyvo/client` | Browser: OIDC (PKCE), session, JWT claims, REST v1 |
| `Kyvo.AspNetCore` | API: JWT validation, `IKyvoUserContext`, authorization policies |
| `Kyvo.Client` | Server: `SubscribeAsync` + typed REST (BFF) |
| `Kyvo.AspNetCore.TenancyKit` | Optional EF multi-tenant bridge (TenancyKit + `tid` claim) |

**Versioning:** SemVer per package, aligned with API `v1.0`. See [CHANGELOG.md](CHANGELOG.md).

## Who calls what (typical CRM)

| Kyvo resource | SPA (`@kyvo/client`) | Product API (.NET SDK) |
|--------------|------------------------------|-------------------------|
| OIDC login / refresh / logout | Yes | No (validates JWT only) |
| `POST /auth/subscribe` | **No** | **Yes** (BFF) |
| `auth/switch-tenant`, sessions | Yes | Optional |
| users, tenants, memberships, roles, audit | Yes | Optional |
| applications, Kyvo admin, platform bootstrap | No | No |

## Endpoint matrix (v1.0)

| Area | Methods | TS | .NET Client |
|------|---------|----|-------------|
| Auth | switch-tenant, sessions, revoke session | Yes | Yes |
| Auth | subscribe | **No** | **Yes** |
| Users | me, me/memberships, PATCH me | Yes | Yes |
| Tenants | list, get, patch, invites, accept invite | Yes | Yes |
| Memberships | CRUD under `/tenants/{id}/memberships` | Yes | Yes |
| Tenant roles | list/create under tenant; patch role | Yes | Yes |
| Audit logs | list | Yes | Yes |

Paths use prefix `/v1.0/` (configurable via `apiVersion`).

## TenancyKit

Product APIs with EF should prefer `Kyvo.AspNetCore.TenancyKit` over manual `tid` filtering. See [dotnet/TENANCYKIT.md](dotnet/TENANCYKIT.md).

## Samples

[Pulse CRM](../samples/pulse-crm/) is the reference consumer.
