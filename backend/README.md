# IdP Platform — Backend

[English](./README.md) | [Português](./README.pt-BR.md)

A .NET 8 API that implements a full **Identity Provider (IdP)**: local authentication, OIDC (authorization code + PKCE), multi-tenancy, roles, OAuth applications, and federation with external providers.

> Coding conventions and required patterns: see [../rules/backend-rules.md](../rules/backend-rules.md).

---

## Architecture

The solution follows **Clean Architecture** with 4 projects:

```
IdPPlatform.Domain          → Entities, value objects, repository interfaces, domain rules
IdPPlatform.Application     → Services per aggregate, DTOs, requests, technical service interfaces
IdPPlatform.Infrastructure  → Implementations: EF Core, OIDC, email (AWS SES), technical services
IdPPlatform.API             → ASP.NET Core controllers, Program.cs, middlewares, MVC views (login)
```

### Services per aggregate (Application layer)

| Interface | Responsibility |
|-----------|----------------|
| `IPlatformService` | Bootstrap and platform status |
| `IUserService` | Create/update user, list memberships, link external identity |
| `IRegistrationService` | Self-registration (User + UserCredential, no tenant) |
| `ITenantService` | Tenant CRUD, invites, accept invite |
| `ITenantRoleService` | CRUD of tenant-scoped roles |
| `IMembershipService` | Create/revoke/update memberships |
| `IApplicationService` | OAuth application CRUD, create clients, tenant provisioning |
| `IAuditLogService` | List audit logs |
| `IAuthService` | Switch/subscribe tenant, manage sessions |
| `ILocalAuthenticationService` | Local login (email + BCrypt) |
| `IIdentityProviderService` | CRUD of identity providers (Local, Firebase, Cognito…) with capability flags |

### Authentication flow

```
POST /account/signin (email + password)
  → SessionCookie with OidcLoginContext

GET /connect/authorize (PKCE)
  → Backend validates the cookie and issues an authorization_code

POST /connect/token (code + verifier)
  → JWT RS256 (access_token + id_token + refresh_token)

Bearer JWT → protected v1 controllers
```

---

## Prerequisites

| Tool | Version |
|------|---------|
| .NET SDK | 8.0+ |
| PostgreSQL | 14+ |
| Redis | optional (tenant cache; falls back to in-memory) |
| `dotnet-ef` | `dotnet tool install --global dotnet-ef` |

---

## Configuration

All configuration lives in `IdPPlatform.API/appsettings.json` (template) and `appsettings.Development.json` (local development values).

### Appsettings sections

| Section | Main keys | Description |
|---------|-----------|-------------|
| `Database` | `ConnectionString` | PostgreSQL connection string |
| `Jwt` | `Issuer`, `Audience`, `SigningKeyPath`, `SigningKeyPem`, `KeyId`, `RefreshTokenDays` | RS256 token configuration |
| `Bootstrap` | `AdminEmail`, `AdminPassword`, `AdminDisplayName` | Root admin credentials (see below) |
| `RateLimit` | `BootstrapPermitLimit`, `BootstrapWindowMinutes` | Bootstrap endpoint rate limit |
| `Invite` | `ExpirationHours` | Invite expiration |
| `Email` | `FromAddress`, `Region`, `AccessKeyId`, `SecretAccessKey` | AWS SES for invite delivery |
| `Redis` | `ConnectionString`, `InstanceName`, `TenantIdentifierCacheMinutes` | Distributed cache (optional) |
| `SecretProtection` | `KeyDirectoryPath`, `ApplicationName` | Data protection key ring used to encrypt IdP credentials at rest |
| `PasswordPolicy` | `MinLength`, `RequireDigit`, `RequireLetter` | Password policy enforced on self-registration |

Every Options class is bound and validated at startup (`IValidateOptions<T>` + `ValidateOnStart()`). Misconfigured production deployments fail fast.

### Environment variables (Docker production `.env`)

ASP.NET Core maps `Section__Property` to `Section:Property` (equivalent to nested JSON). Example for bootstrap:

| Variable | Required | Description |
|----------|----------|-------------|
| `Bootstrap__AdminEmail` | Yes | Root admin email |
| `Bootstrap__AdminPassword` | Yes | Initial password (never stored in plain text) |
| `Bootstrap__AdminDisplayName` | No | Display name (defaults to the local part of the email) |

Other common keys: `Database__ConnectionString`, `Jwt__Issuer`, `Jwt__SigningKeyPem`, `Redis__ConnectionString`, `Email__FromAddress`, `SecretProtection__KeyDirectoryPath`, etc.

In local development the `Bootstrap` section in `appsettings.Development.json` is sufficient.

> After bootstrap, remove `Bootstrap__*` from the production environment. They are only needed on the first run.

### Docker container image

The API is included in the monolith image built from [../docker/Dockerfile](../docker/Dockerfile). **Deploy:** [../GETTING_STARTED.md § Production](../GETTING_STARTED.md#7-production-deployment-docker-compose). **Build/push:** [../docs/DOCKER_PUBLISH.md](../docs/DOCKER_PUBLISH.md). The standalone [`Dockerfile`](./Dockerfile) remains for API-only local builds.

| Topic | Detail |
|-------|--------|
| Listen port | `8080` inside the container (`ASPNETCORE_URLS=http://+:8080`); map to host port `5000` in compose by default |
| Migrations | EF Core migrations bundle runs when `Database__ApplyMigrationsOnStartup=true` (entrypoint) |
| JWT key | Prefer `Jwt__SigningKeyPem` or mount `keys/oidc-signing.pem` with `Jwt__SigningKeyPath` |
| Data Protection | Mount volume at `/app/keys/data-protection`; set `SecretProtection__KeyDirectoryPath=keys/data-protection` |
| Health | Orchestrators can probe `GET /v1.0/platform/status` on port `8080` |
| HTTPS | Forwarded Headers enabled for TLS termination in the monolith nginx proxy |

### RSA key for OIDC

JWTs are signed with RSA (RS256). Generate the key before starting.

**Recommended option — the `GenerateOidcKey` tool in the solution:**

```bash
cd backend
dotnet run --project tools/GenerateOidcKey/GenerateOidcKey.csproj
# Writes IdPPlatform.API/keys/oidc-signing.pem by default
```

Custom path: `dotnet run --project tools/GenerateOidcKey/GenerateOidcKey.csproj -- path/to/key.pem`

**OpenSSL alternative:**

```bash
cd backend/IdPPlatform.API
mkdir keys
openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 -out keys/oidc-signing.pem
```

Configure `Jwt:SigningKeyPath` (or `Jwt__SigningKeyPath`) with the file path, or `Jwt:SigningKeyPem` / `Jwt__SigningKeyPem` with the inline PEM contents (useful in containers).

---

## Secret protection at rest

Identity provider configuration JSON (`IdentityProvider.ConfigJson`) frequently contains secrets (Firebase `ServiceAccount`, `WebApiKey`, etc.). These sensitive top-level paths are encrypted before persistence using ASP.NET Core Data Protection through `ISecretProtector` and `IdentityProviderConfigCipher`.

- Plain-text payloads are still readable at runtime; they are re-encrypted lazily on the next write.
- Encrypted values are tagged with the prefix `enc:v1:`.
- The keyring is persisted under `SecretProtection:KeyDirectoryPath` (default `keys/data-protection`). **Lose those keys and previously stored secrets become unreadable** — back them up alongside the database.

`IdentityProviderDto` deliberately omits `ConfigJson`; secrets are never returned to API consumers.

---

## Build the Docker image

From the repository root:

```bash
docker build -f backend/Dockerfile -t <dockerhub-username>/idpplatform-api:<tag> .
```

---

## Run locally

```bash
cd backend

# 1. Restore dependencies
dotnet restore

# 2. Apply migrations (the database must already exist)
dotnet ef database update \
  --project IdPPlatform.Infrastructure \
  --startup-project IdPPlatform.API

# 3. Start the API
dotnet run --project IdPPlatform.API
```

The API runs at `http://localhost:5000`. Swagger is available under `/swagger` in Development/Staging.

---

## Bootstrap

Bootstrap initializes the platform for the first time (executed only once).

**Recommended flow:** with the API and the frontend running, open `http://localhost:3000`. If `GET /v1.0/platform/status` reports `requiresBootstrap: true`, the login screen shows the **Initialize platform** button, which calls `POST /v1.0/platform/bootstrap` (no body; credentials come from backend configuration only).

**Alternative (ops / CI):**

```bash
curl -X POST http://localhost:5000/v1.0/platform/bootstrap
# { "isConfigured": true, "rootUserId": "...", "oauthClientId": "platform-admin-web" }
```

Bootstrap automatically creates:

- Root admin user with a local credential (BCrypt)
- `plat_admin` platform role assigned to the admin
- `local` Identity Provider enabled
- Application `platform-admin` + Client `platform-admin-web` (fixed, not editable via API)
- A `PlatformConfiguration` row marking the system as bootstrapped

Check the status first:

```bash
curl http://localhost:5000/v1.0/platform/status
# { "isConfigured": false, "requiresBootstrap": true, "oauthClientId": null }
```

---

## Main endpoints

### Platform

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/v1.0/platform/status` | Public | Status and whether bootstrap is required |
| POST | `/v1.0/platform/bootstrap` | Public (rate limited) | One-time platform initialization |

### Account / OIDC

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/account/login` | Public | Login page (Blazor Web App Static SSR) |
| POST | `/account/signin` | Public | Local credential handler (cookie sign-in) |
| GET | `/account/register` | Public | Self-registration page (Blazor SSR) |
| POST | `/account/register` | Public, rate-limited | Self-registration handler (creates User + UserCredential) |
| POST | `/account/external-signin` | Public | Federated login handler (Firebase id_token) |
| GET/POST | `/connect/authorize` | Cookie | OIDC authorization endpoint |
| POST | `/connect/token` | Client credentials | Code-to-token exchange |
| GET | `/.well-known/openid-configuration` | Public | OIDC discovery |
| GET | `/.well-known/jwks.json` | Public | Public RSA keys |

### Auth (JWT)

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/v1.0/auth/subscribe` | JWT | SaaS onboarding (create a tenant via the OAuth app) |
| POST | `/v1.0/auth/switch-tenant` | JWT | Switch the active tenant in the session |
| GET | `/v1.0/auth/sessions` | JWT | List active sessions |
| DELETE | `/v1.0/auth/sessions/{id}` | JWT | Revoke a session |

### Users

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/v1.0/Users/me` | JWT | Current user profile |
| PATCH | `/v1.0/Users/me` | JWT | Update profile |
| GET | `/v1.0/Users/me/memberships` | JWT | User memberships |

### Identity Providers

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| GET | `/v1.0/IdentityProviders` | JWT + plat_admin | List IdPs |
| POST | `/v1.0/IdentityProviders` | JWT + plat_admin | Add IdP (ConfigJson sensitive fields are encrypted on save) |
| PATCH | `/v1.0/IdentityProviders/{id}` | JWT + plat_admin | Update IdP |
| POST | `/v1.0/IdentityProviders/{id}/enable` | JWT + plat_admin | Enable |
| POST | `/v1.0/IdentityProviders/{id}/disable` | JWT + plat_admin | Disable |

#### Federated identity providers

Each external IdP's configuration lives in the database (`ConfigJson`), entered through the admin console. **There is no `Firebase` section in appsettings.**

| Type | `ConfigJson` (main fields) | Login on `/account/login` |
|------|-----------------------------|---------------------------|
| `Local` | optional / empty | email + password |
| `Firebase` | `projectId`, `webApiKey`, `authDomain` (optional), `serviceAccount` | Google button (Firebase JS + `POST /account/external-signin`) |
| `Cognito` | `userPoolId`, `region`, `clientId` | registration validated; login not yet implemented |
| `Generic` | `issuer`, `jwksUri`, `audience` | registration validated; login not yet implemented |

OIDC flow: the admin console kicks off `connect/authorize` → redirect to `/account/login` → methods displayed according to the **enabled** IdPs → session cookie → return to the client.

**Firebase / Google:** in the [Firebase Console](https://console.firebase.google.com/), under the same `projectId` from the `ConfigJson`, enable **Authentication** and the **Google** provider. `serviceAccount` is the Firebase Admin service account JSON (used to verify the `idToken` on the server).

The stored `ExternalIdentity.Provider` uses the **alias** of the record (e.g., `firebase`), not a hardcoded global string.

### Tenants, Memberships, Applications, Audit Logs

See `frontend/swagger.json` for the complete endpoint list.

---

## Authorization

- **Claim `prole=plat_admin`**: platform administrator. Resolved by reading `UserPlatformRole` + `PlatformRole` from the database.
- **Policy `PlatformAdministrator`**: protects tenant/application/IdP management.
- **`trole`**: roles of the active tenant (owner, admin, member, viewer).
- **Tenant context**: `tid` (tenant id) and `mid` (membership id) claims in the JWT.

---

## Domain entities

| Entity | Table | Description |
|--------|-------|-------------|
| `User` | `users` | Platform user |
| `UserCredential` | `user_credentials` | Local BCrypt credential |
| `UserPlatformRole` | `user_platform_roles` | Platform role assignment |
| `PlatformRole` | `platform_roles` | Global roles (e.g., `plat_admin`) |
| `ExternalIdentity` | `external_identities` | External IdP identity link |
| `IdentityProvider` | `identity_providers` | IdP configuration (Local, Firebase…) |
| `Tenant` | `tenants` | Organization / isolated space |
| `TenantRole` | `tenant_roles` | Per-tenant configurable roles |
| `TenantMembership` | `tenant_memberships` | User ↔ tenant link |
| `Application` | `applications` | Registered OAuth application |
| `ApplicationClient` | `application_clients` | OAuth client (public/confidential) |
| `ApplicationTenant` | `application_tenants` | App ↔ tenant link (provisioning) |
| `AuthSession` | `auth_sessions` | Active session (binds cookie to JWT) |
| `AuditLog` | `audit_logs` | Per-tenant event log |
| `TenantInvite` | `tenant_invites` | Tenant invite |

---

## Migrations

```bash
# Create a new migration
dotnet ef migrations add MigrationName \
  --project IdPPlatform.Infrastructure \
  --startup-project IdPPlatform.API \
  --output-dir Migrations

# Apply to the database
dotnet ef database update \
  --project IdPPlatform.Infrastructure \
  --startup-project IdPPlatform.API

# Remove the last (unapplied) migration
dotnet ef migrations remove \
  --project IdPPlatform.Infrastructure \
  --startup-project IdPPlatform.API
```

---

## Project structure

```
IdPPlatform.API/
├── Components/          Blazor Web App (Static SSR) - login/register pages and shared layout
│   ├── App.razor            Root document (html/head/body)
│   ├── Routes.razor         Router
│   ├── Layout/              AccountLayout (split-screen with hero)
│   └── Pages/Account/       Login.razor, Register.razor
├── Controllers/         API controllers (Account POST handlers, Authorization, WellKnown, v1 REST)
├── Common/              Base controllers, middlewares, OidcLoginContext, FederatedProviderClientConfig
├── wwwroot/css/         Modern, theme-aware account.css (no framework, no build step)
├── appsettings.json     Configuration template
└── Program.cs           Startup (DI, OIDC, policies, rate limiting, Razor components)

IdPPlatform.Application/
├── Services/            Interfaces and DTOs per aggregate
│   ├── AppService/      IApplicationService
│   ├── AuditLog/        IAuditLogService
│   ├── Auth/            IAuthService, IExternalLoginService, OIDC DTOs
│   ├── IdentityProvider/IIdentityProviderService, IIdentityProviderConfigCipher
│   ├── LocalAuthentication/ ILocalAuthenticationService
│   ├── Membership/      IMembershipService
│   ├── Oidc/            IOidcTokenService, IOidcClaimsService, ApplicationClientValidationContext
│   ├── Platform/        IPlatformService
│   ├── Registration/    IRegistrationService, IPasswordPolicy
│   ├── Security/        ISecretProtector
│   ├── Tenant/          ITenantService
│   ├── TenantRoles/     ITenantRoleService
│   └── Users/           IUserService
├── Common/              PagedRequest, PagedResult, ApplicationClientListFields
└── Exceptions/          ApplicationErrorMessages (static messages)

IdPPlatform.Infrastructure/
├── Configurations/      JwtOptions, BootstrapOptions, DatabaseOptions, SecretProtectionOptions, PasswordPolicyOptions, validators
├── Extensions/          AddInfrastructure, AddAggregateServices, AddRepositories, AddServices, AddSecretProtection
├── Migrations/          FirstMigration
├── Persistence/
│   ├── Configurations/  EF Fluent API per entity
│   ├── Repositories/    Repository implementations
│   └── ApplicationDbContext.cs
└── Services/
    ├── Security/        DataProtectionSecretProtector
    └── ...              All other service implementations
```
