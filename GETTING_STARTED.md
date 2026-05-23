# Getting Started — IdP Platform

[English](./GETTING_STARTED.md) | [Português](./GETTING_STARTED.pt-BR.md)

Complete guide to configure and run the IdP Platform from scratch in a local development environment.

---

## 1. Prerequisites

Install before continuing:

| Tool | How to install | Minimum version |
|------|----------------|-----------------|
| .NET SDK | [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0) | 8.0 |
| Node.js | [nodejs.org](https://nodejs.org/) | Current LTS |
| PostgreSQL | [postgresql.org](https://www.postgresql.org/download/) | 14 |
| Redis | [redis.io](https://redis.io/downloads/) | Optional (in-memory cache fallback in dev) |
| dotnet-ef (CLI) | `dotnet tool install --global dotnet-ef` | 8.x |
| openssl | Bundled with macOS/Linux; Windows: Git Bash or scoop | Any |

Clone the repository:

```bash
git clone <repo-url>
cd idpplatformproject
```

---

## 2. Configure the database

Create a PostgreSQL database for the project:

```sql
CREATE DATABASE idpplatform_db;
```

Or via the command line:

```bash
createdb idpplatform_db
```

---

## 3. Configure the backend

### 3.1 Edit the development appsettings

In `backend/IdPPlatform.API/appsettings.Development.json` update the connection string:

```json
{
  "Database": {
    "ConnectionString": "Host=localhost;Port=5432;Database=idpplatform_db;Username=YOUR_USER;Password=YOUR_PASSWORD"
  }
}
```

The remaining sections already ship with safe defaults for local development.

### 3.2 Generate the RSA key used to sign JWTs

OIDC uses RS256 (RSA + SHA-256). The solution ships the **GenerateOidcKey** utility, which writes the key into `IdPPlatform.API/keys/oidc-signing.pem`:

```bash
cd backend
dotnet run --project tools/GenerateOidcKey/GenerateOidcKey.csproj
```

OpenSSL alternative:

```bash
cd backend/IdPPlatform.API
mkdir keys
openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 -out keys/oidc-signing.pem
```

`appsettings.Development.json` already points to `"SigningKeyPath": "keys/oidc-signing.pem"`. Do not commit the key.

### 3.3 Configure bootstrap admin credentials

The first administrator's credentials are read from environment variables **or** the `Bootstrap` section of `appsettings.Development.json`.

For development, the simplest path is to edit appsettings:

```json
{
  "Bootstrap": {
    "AdminEmail": "admin@localhost",
    "AdminPassword": "YourSecurePassword@123",
    "AdminDisplayName": "Platform Admin"
  }
}
```

> In production or Docker, use environment variables in the `Bootstrap__AdminEmail`, `Bootstrap__AdminPassword`, `Bootstrap__AdminDisplayName` format (the `__` represents JSON nesting) and **never** commit real credentials to appsettings.

### 3.4 Apply the migration to the database

```bash
cd backend

dotnet ef database update \
  --project IdPPlatform.Infrastructure \
  --startup-project IdPPlatform.API
```

This creates every table (`users`, `user_credentials`, `platform_roles`, `identity_providers`, `tenants`, `applications`, `application_clients`, `auth_sessions`, `audit_logs`, etc.).

### 3.5 Start the API

```bash
cd backend
dotnet run --project IdPPlatform.API
```

The API is available at `http://localhost:5000`. Swagger lives at `http://localhost:5000/swagger`.

Confirm it is healthy:

```bash
curl http://localhost:5000/v1.0/platform/status
# Expected response: { "isConfigured": false, "requiresBootstrap": true, "oauthClientId": null }
```

---

## 4. Configure and start the frontend

### 4.1 Optionally create the .env file

```bash
cd frontend
cp .env.example .env
```

`.env.example` lists the variables that the admin SPA understands; the same values are baked into `src/config/env.ts` as defaults, so the SPA also runs **without** an `.env` file in local development:

```env
VITE_API_BASE_URL=http://localhost:5000
VITE_API_VERSION=1.0
VITE_API_TIMEOUT_MS=30000
VITE_OAUTH_CLIENT_ID=platform-admin-web
VITE_OAUTH_REDIRECT_URI=http://localhost:3000/auth/callback
```

No edits are required for local development.

### 4.2 Install dependencies and start

```bash
cd frontend
npm install
npm run dev
```

The frontend runs at `http://localhost:3000`.

---

## 5. Bootstrap and sign in

Open `http://localhost:3000` (with both the API and the frontend running).

### Bootstrap (first run)

If the platform has not been bootstrapped yet, the `/login` screen shows **Initialize platform** instead of the OIDC login button. Click it to run the bootstrap (credentials are read from the backend — `Bootstrap` section or `Bootstrap__*` env vars).

The bootstrap creates, exactly once:

- Admin user with the password configured in appsettings / env vars
- Platform role `plat_admin` assigned to the admin
- `local` Identity Provider enabled
- Application `platform-admin` + OAuth Client `platform-admin-web` (fixed, not editable via API)

Once it succeeds, the same route starts showing the OIDC login.

**Ops alternative:** with the API running, `curl -X POST http://localhost:5000/v1.0/platform/bootstrap`.

Check the status:

```bash
curl http://localhost:5000/v1.0/platform/status
# Before: { "requiresBootstrap": true, ... }
# After:  { "isConfigured": true, "requiresBootstrap": false, "oauthClientId": "platform-admin-web" }
```

> After a successful production bootstrap, remove `Bootstrap__*` from the environment. They no longer have any effect.

### Sign in

1. Click **"Sign in to the platform"**
2. You are redirected to `/account/login` on the backend (modern Blazor SSR page, no popup for Google)
3. Enter the email and password configured during bootstrap (e.g., `admin@localhost` / `YourSecurePassword@123`)
4. After authentication, the backend redirects to the OIDC callback
5. The frontend stores the tokens and opens the admin console

### Self-registration (new users)

For end users who do NOT yet have an account in the platform (typical SaaS onboarding):

1. From any consumer app (e.g., Pulse CRM) the user clicks "Sign in" and is redirected to `/connect/authorize`.
2. The IdP login page exposes a **Create account** link to `/account/register`.
3. The user fills email, password (matching `PasswordPolicy` requirements) and display name. The endpoint is rate-limited by the `account_register` policy.
4. After successful registration the platform creates a `User` + `UserCredential` and signs the user in via the cookie scheme — NO tenant or membership is created at this point.
5. The user is redirected back to `/connect/authorize`; the consumer app receives the OIDC `code`.
6. The consumer app detects the missing `tid` claim in the access token and triggers its onboarding flow, calling `POST /v1.0/auth/subscribe` with tenant + plan to attach the user to a tenant. After a refresh token, the new access token includes `tid` / `mid`.

This central signup model means client apps NEVER implement their own "create account" pages; password collection only happens on the IdP domain.

---

## 6. Next steps

### Create a tenant

In the admin console go to **Tenants** → **Create tenant**. Provide a name and a unique key (e.g., `my-org`).

### Invite members

Inside a tenant, navigate to **Tenants** → select the tenant → **Invite member**. A link is emailed (configure AWS SES under `Email.*` for real delivery; in dev the invite is generated but not sent).

### Register an OAuth application

Go to **Applications** → **New application**. After creation, open the details and register an **OAuth Client** with your consumer application's redirect URIs.

### Add external identity providers (optional)

As a platform admin, navigate to **Identity Providers** → **Add IdP**. The `local` provider (bootstrap) stays enabled for email/password.

Identity provider credentials (Firebase `ServiceAccount`, `WebApiKey`, etc.) are stored **encrypted at rest** using ASP.NET Core Data Protection. The plaintext values are required during creation/update only and are never returned by `GET` endpoints.

#### Capabilities

Each identity provider declares one or more `IdpCapability` flags. The admin form surfaces them as checkboxes:

| Capability | Allowed for | Conflict policy |
|------------|-------------|-----------------|
| `LocalPassword` | `Local` only (hard-locked) | Only **one** enabled provider can advertise it. Adding a second one fails. |
| `GoogleSocial` | Firebase, Cognito, Generic | Adding a second enabled provider returns a `warnings` payload but is allowed. |
| `MicrosoftSocial` | Firebase, Cognito, Generic | Soft warning on conflict. |
| `AppleSocial` | Firebase, Cognito, Generic | Soft warning on conflict. |
| `GenericOidc` | Cognito, Generic | Soft warning on conflict. |

The hard-lock for `LocalPassword` mirrors what Microsoft Entra and other enterprise IdPs do: a single source of email/password authentication keeps account linking deterministic and avoids UI ambiguity ("which email/password form is legitimate?"). Social providers are softer: legitimate multi-realm setups can run two Google connections side by side and you only get a warning so the admin acknowledges the conflict.

#### Firebase + Google (working federated login)

Firebase exposes **two different JSON files**. In the admin console you build **a third format** — only these fields at the root:

| Field | Source in Firebase Console | Purpose |
|-------|---------------------------|---------|
| `projectId` | ⚙️ Project settings → **General** → Project ID | Identify the project on Google login |
| `webApiKey` | Same screen → **Web API key** | Firebase SDK on `/account/login` (Google popup) |
| `authDomain` | Web app → `firebaseConfig.authDomain` (e.g., `my-project.firebaseapp.com`) | Required by the SDK; if omitted, the API uses `{projectId}.firebaseapp.com` |
| `serviceAccount` | Settings → **Service accounts** → Generate new private key (`.json` file) | Validate the `idToken` on the server (Admin SDK) |

**Do not paste** the entire `firebaseConfig` / `google-services.json` from the Web app (an object with `authDomain`, `storageBucket`, etc.). If you already have that snippet in your frontend, use it only to map `apiKey` → `webApiKey` and the project ID → `projectId`; the `serviceAccount` value comes **only** from the downloaded service account file.

**ConfigJson template** (replace with your own values; the `serviceAccount` object is the full content of the `*-firebase-adminsdk-*.json` file):

```json
{
  "projectId": "my-firebase-project",
  "webApiKey": "AIzaSyXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
  "authDomain": "my-firebase-project.firebaseapp.com",
  "serviceAccount": {
    "type": "service_account",
    "project_id": "my-firebase-project",
    "private_key_id": "...",
    "private_key": "-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----\n",
    "client_email": "firebase-adminsdk-xxxxx@my-firebase-project.iam.gserviceaccount.com",
    "client_id": "...",
    "auth_uri": "https://accounts.google.com/o/oauth2/auth",
    "token_uri": "https://oauth2.googleapis.com/token"
  }
}
```

Steps:

1. [Firebase Console](https://console.firebase.google.com/) → **Authentication** → **Sign-in method** → enable **Google**.
2. Download the **service account** (Admin SDK) key and note the **Project ID** + **Web API key** (General).
3. Admin console (`http://localhost:3000`) → **Identity Providers** → **Add IdP** → type **Firebase**, alias e.g. `firebase`, paste the JSON above → **Enabled**.
4. Keep the `local` IdP enabled (from bootstrap).
5. Test: any OIDC app (admin or Pulse CRM) → redirect → `http://localhost:5000/account/login` → **Continue with Google**.

**Pulse CRM with Google:** the CRM does not integrate Firebase directly; it redirects to the platform OIDC. With the Firebase IdP enabled, on `/account/login` the user signs in with Google, returns to the CRM with a `code`, completes onboarding/subscribe, and uses the API normally. See `samples/pulse-crm/backend/README.md`.

**Cognito / Generic:** registration with a valid `ConfigJson` works; sign-in on the `/account/login` page is not implemented yet.

### Integrate a consumer application

1. Register an **Application** and an **OAuth Client** in the admin console (your app's redirect URIs).
2. Use the discovery URL: `http://localhost:5000/.well-known/openid-configuration` (in production replace it with the public host of the API).
3. Implement authorization code + PKCE in your client (SPA, backend, etc.).

---

## 7. Running with Docker

Use this path when you want to run **pre-built container images** instead of compiling from source. PostgreSQL and Redis are started separately (optional infrastructure compose or managed services).

**Full guide:** [docker/README.md](./docker/README.md) (build, push to Docker Hub, environment variables, volumes).

### Prerequisites

| Tool | Purpose |
|------|---------|
| Docker Engine + Docker Compose v2 | Run containers |
| Published images on Docker Hub | Set `DOCKERHUB_USERNAME` and `IMAGE_TAG` in `docker/.env` |

You do **not** need the .NET SDK or Node.js on the host to run the application stack (only to **build** images or generate the OIDC key).

### Overview

1. Start PostgreSQL and Redis — [docker/docker-compose.infrastructure.yml](./docker/docker-compose.infrastructure.yml) or your own hosts.
2. Generate `oidc-signing.pem` (see step 3.2) and configure JWT in `docker/.env`.
3. Copy `docker/.env.app.example` → `docker/.env` and fill in connection strings and bootstrap credentials.
4. `docker compose -f docker/docker-compose.yml --env-file docker/.env up -d` (add `-f docker/docker-compose.infra-network.yml` when using the infrastructure compose on the shared network).
5. Open `http://localhost:3000`, complete bootstrap, then remove `Bootstrap__*` from `.env` and restart the API.

### Environment variables (Docker)

Application variables live in `docker/.env` (see [docker/.env.app.example](./docker/.env.app.example)). ASP.NET Core uses the `Section__Property` form.

| Variable | Notes |
|----------|-------|
| `Database__ConnectionString` | Use `Host=postgres` on the infra network, or `Host=host.docker.internal` when DB listens on the host |
| `Redis__ConnectionString` | Same pattern; recommended in production |
| `Jwt__Issuer` | Must match the URL users use for the API (e.g. `http://localhost:5000` with default port mapping) |
| `Jwt__SigningKeyPem` or `Jwt__SigningKeyPath` | Required; mount a volume or inline PEM |
| `SecretProtection__KeyDirectoryPath` | Persisted via Docker volume `api-dataprotection` |
| `Bootstrap__AdminEmail` / `Bootstrap__AdminPassword` | First deploy only |
| `Database__ApplyMigrationsOnStartup` | `true` applies EF migrations on container start (default in the example file) |

The **frontend image** is built with `VITE_*` arguments (API URL, OAuth redirect). Changing public URLs requires rebuilding and republishing the frontend image — not runtime env vars in compose.

Default baked-in values match local Docker port mapping:

- `VITE_API_BASE_URL=http://localhost:5000`
- `VITE_OAUTH_REDIRECT_URI=http://localhost:3000/auth/callback`

### Building and publishing images (maintainers)

From the repository root:

```bash
docker build -f backend/Dockerfile -t <username>/idpplatform-api:1.0.0 .
docker build -f frontend/Dockerfile \
  --build-arg VITE_API_BASE_URL=http://localhost:5000 \
  --build-arg VITE_OAUTH_REDIRECT_URI=http://localhost:3000/auth/callback \
  -t <username>/idpplatform-frontend:1.0.0 .
docker push <username>/idpplatform-api:1.0.0
docker push <username>/idpplatform-frontend:1.0.0
```

See [docker/README.md](./docker/README.md) for Docker Hub vs GitHub Packages (ghcr.io), tagging, and production HTTPS.

### Docker troubleshooting

| Issue | Solution |
|-------|----------|
| Cannot connect to database | Check `Database__ConnectionString` and whether you need the [infra-network overlay](./docker/docker-compose.infra-network.yml) |
| API unhealthy | `docker logs idpplatform-api` — often missing JWT key |
| OAuth redirect mismatch | Rebuild frontend with correct `VITE_OAUTH_REDIRECT_URI`; align OAuth client in admin |

---

## 8. Production configuration

### Critical environment variables

| Environment variable (`__`) | Production |
|-----------------------------|------------|
| `Database__ConnectionString` | Managed database connection string (RDS, Cloud SQL, etc.) |
| `Jwt__SigningKeyPem` | PEM contents of the RSA private key (inline, no file) |
| `Jwt__Issuer` | Public backend URL (e.g., `https://auth.mysite.com`) |
| `Bootstrap__AdminEmail` | Only on the first deploy; remove after bootstrap |
| `Bootstrap__AdminPassword` | Only on the first deploy; remove after bootstrap |
| `Bootstrap__AdminDisplayName` | Optional on the first deploy |
| `Email__FromAddress`, `Email__Region`, etc. | AWS SES configuration for invites |
| `Redis__ConnectionString` | Distributed cache (ElastiCache, Redis Cloud, etc.) |
| `SecretProtection__KeyDirectoryPath` | Persistent directory for the data protection key ring (must survive restarts and be backed up) |
| `SecretProtection__ApplicationName` | Logical name for key isolation (defaults to `IdPPlatform`) |
| `VITE_API_BASE_URL` | Public API URL (during the frontend build) |
| `VITE_OAUTH_REDIRECT_URI` | Public frontend OIDC callback URL |

In a production `appsettings.json`, use `:` instead (e.g., `Database:ConnectionString`).

### Frontend production build

From source:

```bash
cd frontend
# Configure the VITE_* variables before building (or rely on the defaults in src/config/env.ts)
npm run build
# Serve the dist/ folder with nginx, Cloudflare Pages, etc.
```

With Docker, pass the same `VITE_*` values as **build-args** when building the frontend image (see [section 7](#7-running-with-docker) and [docker/README.md](./docker/README.md)).

### HTTPS

In production every connection must use HTTPS. `Jwt:Issuer` must use `https://` for OIDC to work correctly.

---

## 9. Command quick reference

```bash
# Backend: apply migrations
dotnet ef database update --project IdPPlatform.Infrastructure --startup-project IdPPlatform.API

# Backend: create a new migration
dotnet ef migrations add MigrationName --project IdPPlatform.Infrastructure --startup-project IdPPlatform.API --output-dir Migrations

# Backend: run in dev
dotnet run --project backend/IdPPlatform.API

# Frontend: run in dev
cd frontend && npm run dev

# Frontend: build
cd frontend && npm run build

# OIDC key (GenerateOidcKey)
dotnet run --project backend/tools/GenerateOidcKey/GenerateOidcKey.csproj

# Bootstrap (with API running) — or use the button in the frontend at /login
curl http://localhost:5000/v1.0/platform/status
curl -X POST http://localhost:5000/v1.0/platform/bootstrap
```

---

## 10. Troubleshooting

| Issue | Likely cause | Solution |
|-------|--------------|----------|
| API fails to start: RSA key error | `keys/oidc-signing.pem` is missing | Generate it with `openssl genpkey` (step 3.2) |
| Bootstrap returns 400 | Credentials not configured in appsettings/env | Verify the `Bootstrap` section or `Bootstrap__AdminEmail` / `Bootstrap__AdminPassword` |
| Bootstrap returns "already bootstrapped" | Bootstrap was already executed | Ignore and sign in normally |
| Frontend does not load after login | `VITE_OAUTH_REDIRECT_URI` is wrong | Confirm that the `redirect_uri` matches the `platform-admin-web` client |
| Expired JWT / 401 | Token expired and refresh failed | Sign out and sign in again |
| Invites do not arrive by email | AWS SES is not configured | Configure `Email:*` with valid SES credentials |
| CORS error | Frontend on a different URL | Verify `VITE_API_BASE_URL` and the API's CORS settings |
| Cannot decrypt an existing IdP configuration | Data Protection key ring lost | Restore the `SecretProtection:KeyDirectoryPath` from backup, or recreate the IdP entry |
| Docker: network `idpplatform-infra` not found | Infra overlay without infrastructure compose | Start [docker-compose.infrastructure.yml](./docker/docker-compose.infrastructure.yml) or remove the infra-network overlay |
| Docker: OAuth redirect error after login | Frontend image built with wrong `VITE_OAUTH_REDIRECT_URI` | Rebuild and push the frontend image; verify the OAuth client redirect URI |
