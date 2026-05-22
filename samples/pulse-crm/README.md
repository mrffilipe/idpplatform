# PulseCRM — IdP Platform consumer sample

[English](./README.md) | [Português](./README.pt-BR.md)

SPA + API that simulate a SaaS CRM integrated with the platform: OIDC login, plan selection, mock payment, and **application ↔ tenant** linking through `POST /v1.0/auth/subscribe`.

## Prerequisites

- IdP Platform running (`http://localhost:5000`) with the bootstrap completed
- Application + OAuth client created in the admin console (see [../README.md](../README.md))
- An existing IdP user (local login from bootstrap or an invite)
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

## Test flow

1. **Login** — sign in with the IdP Platform (existing IdP account)
2. **Onboarding** — pick a plan (`starter`, `professional`, `enterprise`)
3. **Payment** — mock approved → the API calls `auth/subscribe` on the platform
4. **Dashboard** — purchased plan + decoded JWT claims
5. **Contacts** — local CRUD isolated per tenant (`tid` from the token)

Backend OIDC/JWT documentation: [backend/README.md](./backend/README.md).
