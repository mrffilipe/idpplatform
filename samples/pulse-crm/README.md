# PulseCRM — IdP Platform consumer sample

[English](./README.md) | [Português](./README.pt-BR.md)

SPA + API that simulate a SaaS CRM integrated with the platform: OIDC login, plan selection, mock payment, and **application ↔ tenant** linking through `POST /v1.0/auth/subscribe`.

## Prerequisites

- IdP Platform running with bootstrap completed (`http://localhost:5000` from source, or **`https://localhost:8443`** via Docker — see below)
- Application + OAuth client created in the admin console (see [../README.md](../README.md))
- A user account on the IdP — either the bootstrap admin, an invited user, OR a new account created via the central **/account/register** page on the IdP (the sample does NOT have its own signup screen).
- .NET 8 SDK and Node.js LTS

## Development ports

| Service | URL |
|---------|-----|
| IdP Platform | http://localhost:5000 |
| PulseCRM API | http://localhost:5100 |
| PulseCRM SPA | http://localhost:5173 |

## Run

### 1. CRM API

```bash
cd samples/pulse-crm/backend/PulseCrm.Api
dotnet run
```

Swagger: http://localhost:5100/swagger

### 2. Frontend

```bash
cd samples/pulse-crm/frontend
cp .env.example .env   # optional — defaults are baked into src/config/env.ts
npm install
npm run dev
```

Open http://localhost:5173

## IdP on Docker (`https://localhost:8443`)

1. Register **Pulse CRM** / client `pulse-crm-web` in the admin console at `https://localhost:8443` (redirect `http://localhost:5173/auth/callback`).
2. Frontend `.env`: `VITE_IDP_AUTHORITY=https://localhost:8443`
3. API `appsettings.Development.json`:

```json
"IdP": {
  "Authority": "https://localhost:8443",
  "Audience": "idpplatform-api",
  "AllowInvalidIdpCertificate": true
}
```

`AllowInvalidIdpCertificate` lets the CRM API trust the IdP’s self-signed TLS cert in dev (JWT metadata/JWKS + `auth/subscribe`). **Restart** `dotnet run` after changing settings.

## Test flow

1. **Sign in / Create account** — the SPA redirects to `/connect/authorize`. The IdP login page lets the user sign in OR follow the link to create an account (`/account/register`). New users are signed in immediately after registration.
2. **Onboarding** — back in the SPA, the absence of a `tid` claim drives the user to pick a plan (`starter`, `professional`, `enterprise`) and a company name.
3. **Payment** — mock approved → the CRM API calls `auth/subscribe` on the platform to create Tenant + Membership + ApplicationTenant.
4. **Token refresh** — the SPA refreshes the token to obtain `tid` / `mid` claims.
5. **Dashboard** — purchased plan + decoded JWT claims.
6. **Contacts** — local CRUD isolated per tenant (`tid` from the token).

Backend OIDC/JWT documentation: [backend/README.md](./backend/README.md).
