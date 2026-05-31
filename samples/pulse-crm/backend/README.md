# PulseCRM API — authentication and authorization (Kyvo)

[English](./README.md) | [Português](./README.pt-BR.md)

Sample API that validates JWTs issued by the Kyvo and calls `POST /v1.0/auth/subscribe` to link tenant + plan (`ApplicationTenant`).

## 1. Register the Application and Client in the admin console

| Field | Value |
|-------|-------|
| Application slug | `pulse-crm` |
| Client ID | `pulse-crm-web` |
| Type | Public |
| Redirect URI | `http://localhost:5173/auth/callback` |
| Scopes | `openid profile email offline_access` |

`appsettings.Development.json` must point to the same issuer/audience as the platform:

```json
"Kyvo": {
  "Authority": "http://localhost:5000",
  "Audience": "kyvo-api"
}
```

## 2. OAuth2 / OIDC flow (SPA)

```
1. SPA → GET {Authority}/connect/authorize?client_id=...&code_challenge=...&scope=openid+profile+email+offline_access
2. The user authenticates at /account/login OR creates an account at /account/register (central Kyvo signup; no signup screen in this sample)
3. Redirect → http://localhost:5173/auth/callback?code=...&state=...
4. SPA → POST {Authority}/connect/token (authorization_code + code_verifier)
5. SPA stores access_token + refresh_token
6. SPA → POST PulseCRM /api/onboarding/complete (Bearer access_token)
7. PulseCRM → POST {Authority}/v1.0/auth/subscribe (forwards the same Bearer)
8. SPA → POST /connect/token (refresh_token) to obtain a JWT with tid and mid claims
```

## 3. JWT validation in this API

- **Authority** = issuer (`Jwt:Issuer` from the platform, e.g., `http://localhost:5000`)
- **Audience** = `kyvo-api` (`aud` claim of the access token)
- Public keys via JWKS: `{Authority}/.well-known/jwks.json`

Useful access-token claims:

| Claim | CRM usage |
|-------|-----------|
| `uid` / `sub` | User identifier |
| `email` | Profile |
| `tid` | Active tenant (required for `/api/contacts`) |
| `mid` | Membership in the tenant |
| `trole` | Tenant roles |
| `prole` | Platform roles |

No `tid` after login: the user has not subscribed yet, or has not refreshed the token.

## 4. `auth/subscribe` and ApplicationTenant

`POST /api/onboarding/complete` calls the platform:

```http
POST /v1.0/auth/subscribe
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "tenantName": "Acme Corp",
  "tenantKey": "acme-corp",
  "planCode": "professional",
  "externalCustomerId": "pay_mock_..."
}
```

The platform creates:

- **Tenant** + membership (owner) for the user of the OAuth session
- **ApplicationTenant** linking the application of client `pulse-crm-web` to the tenant, with `planCode` and `externalCustomerId`

The CRM also stores a local copy in SQLite (`Subscriptions`) to display the plan on the dashboard.

## 5. Refresh after subscribe

The access token issued **before** subscribing does not contain `tid`. The SPA must call:

```http
POST /connect/token
grant_type=refresh_token&refresh_token=...&client_id=pulse-crm-web
```

Requires the `offline_access` scope on the client.

## 6. Troubleshooting

| Error | Cause | Solution |
|-------|-------|----------|
| `invalid_scope` / `offline_access` | Client without the scope | Add `offline_access` to allowed scopes |
| 401 on the CRM API | Wrong audience/issuer | Check `Kyvo:Authority` and `Kyvo:Audience` |
| Contacts 400 "missing tid" | Stale token | Refresh the token after onboarding |
| Subscribe 403/400 | Session without an OAuth client | Sign in via the authorize endpoint of the `pulse-crm-web` client |

## Local endpoints

| Method | Path |
|--------|------|
| GET | `/api/health` |
| GET | `/api/me` |
| POST | `/api/onboarding/complete` |
| GET | `/api/subscription` |
| CRUD | `/api/contacts` |
