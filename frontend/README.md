# IdP Platform — Frontend

[English](./README.md) | [Português](./README.pt-BR.md)

Admin SPA for the IdP Platform. Consumes the API via OIDC (authorization code + PKCE) and exposes the UI to manage tenants, memberships, applications, identity providers, and audit logs.

> Coding conventions and required patterns: see [../rules/frontend-rules.md](../rules/frontend-rules.md).

---

## Stack

| Technology | Version | Use |
|------------|---------|-----|
| React | 19 | UI |
| React Router | 7 (Data mode) | Routing with loaders |
| Material UI | 9 | Design system |
| Axios | 1.x | HTTP client |
| TypeScript | 6 | Static typing |
| Vite | 8 | Build and dev server |

---

## Prerequisites

- Node.js (compatible with the version declared in `package.json`)
- Backend running at `VITE_API_BASE_URL` (see configuration)
- Bootstrap credentials configured in the backend (`Bootstrap` in appsettings or `Bootstrap__*` env vars)
- If the platform has not been bootstrapped yet, the frontend itself runs the bootstrap from the `/login` screen (**Initialize platform** button)

---

## Configuration

Every variable below has a built-in default in `src/config/env.ts`, so the SPA runs in local development without an `.env` file. To override defaults locally, copy `.env.example` to `.env`:

```bash
cp .env.example .env
```

| Variable | Default | Description |
|----------|---------|-------------|
| `VITE_API_BASE_URL` | `http://localhost:5000` | Backend API base URL |
| `VITE_API_VERSION` | `1.0` | API version (produces `/v1.0/...`) |
| `VITE_API_TIMEOUT_MS` | `30000` | Axios request timeout (ms) |
| `VITE_OAUTH_CLIENT_ID` | `platform-admin-web` | OAuth client registered in the IdP |
| `VITE_OAUTH_REDIRECT_URI` | `http://localhost:3000/auth/callback` | OIDC callback URI |

Defaults are kept in sync with the backend constants (`PlatformDefaults.AdminConsole.ClientId` and `DefaultRedirectUris`) and `appsettings.Development.json` — change them together.

### Docker image

The admin SPA is built into a static nginx image ([`Dockerfile`](./Dockerfile); context: repository root). All `VITE_*` values are **fixed at image build time** via `--build-arg`. Changing the public API URL or OAuth redirect requires rebuilding and republishing the image.

```bash
docker build -f frontend/Dockerfile \
  --build-arg VITE_API_BASE_URL=http://localhost:5000 \
  --build-arg VITE_OAUTH_REDIRECT_URI=http://localhost:3000/auth/callback \
  -t <dockerhub-username>/idpplatform-frontend:<tag> .
```

See [../docker/README.md](../docker/README.md) for compose, push to Docker Hub, and consumer setup.

---

## Run

```bash
# Install dependencies
npm install

# Development (port 3000)
npm run dev

# Production build
npm run build

# Preview the build
npm run preview
```

---

## Bootstrap and authentication flow

```
1. The user opens the app (e.g., / or /login)
2. loginLoader / requireAuthLoader call GET /v1.0/platform/status
3. If requiresBootstrap → /login shows "Initialize platform" → POST /v1.0/platform/bootstrap
4. After bootstrap → the same route shows the OIDC login
```

### OIDC (after bootstrap)

```
1. The user navigates to a protected route
2. requireAuthLoader checks the status and the local storage (idp.auth.session)
3. If there is no session → redirect to /login?returnUrl=...
4. LoginPage → redirectToOidcLogin()
5. Browser navigates to GET /connect/authorize (PKCE, state in sessionStorage)
6. Backend redirects to /account/login (email + password form)
7. The user signs in locally → session cookie
8. Backend completes the authorize → redirect to /auth/callback?code=...&state=...
9. AuthCallbackPage validates the state, POST /connect/token (code + verifier)
10. Tokens saved in localStorage (idp.auth.session)
11. Redirect to the original route (returnUrl)
```

The refresh token is rotated automatically via an Axios interceptor when a request returns 401.

Logout clears `localStorage` and redirects to `GET /connect/logout`.

---

## Pages and routes

| Route | Component | Auth | Description |
|-------|-----------|------|-------------|
| `/login` | `LoginPage` | Public | Bootstrap (when `requiresBootstrap`) or kicks off the OIDC flow |
| `/auth/callback` | `AuthCallbackPage` | Public | Exchanges code for tokens |
| `/` | `HomePage` | JWT | Dashboard with module links |
| `/profile` | `ProfilePage` | JWT | User profile and memberships |
| `/sessions` | `SessionsPage` | JWT | List and revoke sessions |
| `/tenants` | `TenantsPage` | JWT | Tenant CRUD, invites, tenant switching |
| `/memberships` | `MembershipsPage` | JWT | Memberships of the active tenant |
| `/tenant-roles` | `TenantRolesPage` | JWT | Tenant-scoped role configuration |
| `/applications` | `ApplicationsPage` | JWT | List and create OAuth applications |
| `/applications/:id` | `ApplicationDetailPage` | JWT | Details, OAuth clients, provisioning |
| `/identity-providers` | `IdentityProvidersPage` | JWT + plat_admin | CRUD of identity providers |
| `/accept-invite` | `AcceptInvitePage` | JWT | Accept a tenant invite by token |
| `/audit-logs` | `AuditLogsPage` | JWT | Audit logs with filters |
| `/jwks` | `JwksPage` | JWT | Display the platform's JWKS |

---

## Folder structure

```
src/
├── components/
│   ├── AppLayout.tsx       Main shell with sidebar and topbar
│   ├── AuthLayout.tsx      Centered layout for auth screens
│   └── ui/                 Reusable components (DataTable, PageHeader, etc.)
├── config/
│   ├── axios.ts            Axios instances (api / publicApi) + 401 interceptor
│   ├── env.ts              Env variable loader with built-in defaults
│   └── index.ts            Re-exports
├── contexts/
│   ├── AuthContext.tsx      Authentication state (JWT claims, platform/tenant roles)
│   ├── TenantContext.tsx    Currently selected tenant (localStorage)
│   └── ThemeModeContext.tsx Light/dark theme
├── pages/                  One component per route
├── routes/
│   └── loaders.ts          Route loaders (requireAuthLoader, loginLoader)
├── routes.tsx              All routes defined with React Router
├── services/               API call functions per resource
├── theme/                  Tokens and createAppTheme (MUI)
├── types/                  TypeScript interfaces aligned with the OpenAPI document
└── utils/
    ├── authStorage.ts      Read/write session in localStorage
    ├── apiError.ts         Extract API error messages
    ├── apiMappers.ts       Normalize API responses (camelCase)
    ├── pkce.ts             Generate code_verifier and code_challenge
    └── jwt.ts              JWT decoding (no validation)
```

---

## API services

| File | Functions |
|------|-----------|
| `platformService.ts` | `getPlatformStatus`, `bootstrapPlatform` |
| `authService.ts` | `subscribeTenant`, `switchTenant`, `listSessions`, `revokeSession` |
| `usersService.ts` | `getMe`, `updateMe`, `listMyMemberships` |
| `tenantsService.ts` | `createTenant`, `listTenants`, `getTenantById`, `updateTenant`, `inviteMember`, `acceptInvite` |
| `membershipsService.ts` | `createMembership`, `listMembershipsByTenant`, `updateMembershipRoles`, `revokeMembership` |
| `tenantRolesService.ts` | `listTenantRoles`, `createTenantRole`, `updateTenantRole` |
| `applicationsService.ts` | `createApplication`, `listApplications`, `getApplicationById`, `createApplicationClient`, `provisionApplicationTenant` |
| `identityProvidersService.ts` | `listIdentityProviders`, `addIdentityProvider`, `updateIdentityProvider`, `enableIdentityProvider`, `disableIdentityProvider` |
| `auditLogsService.ts` | `listAuditLogs` (with filters) |
| `wellKnownService.ts` | `getOpenIdConfiguration`, `getJwks` |
| `oidcService.ts` | `redirectToOidcLogin`, `exchangeCodeForTokens`, `refreshOidcTokens`, `buildLogoutUrl` |

---

## Frontend authorization

`AuthContext` exposes `platformRoles` (the `prole` JWT claim). To check whether the user is a platform admin:

```tsx
const { platformRoles } = useAuth()
const isPlatformAdmin = platformRoles.includes('plat_admin')
```

The "Identity Providers" navigation item and the application creation form are only visible to `plat_admin`.

### Identity Providers — `ConfigJson` schemas and capabilities

The **Identity Providers** page (`IdentityProvidersPage.tsx`) guides the operator by type and now collects `IdpCapability` flags (LocalPassword / GoogleSocial / MicrosoftSocial / AppleSocial / GenericOidc) via checkboxes. The form locks `LocalPassword` to the Local provider; backend `warnings` from social conflicts are surfaced in a dismissible alert.

| Type | JSON fields | Default capabilities | UI note |
|------|-------------|----------------------|---------|
| Local | none required | LocalPassword (locked) | no `ConfigJson` |
| Firebase | `projectId`, `webApiKey`, `serviceAccount` | GoogleSocial | UI guide (`FirebaseConfigHelp`): **not** the Web app `firebaseConfig`; `serviceAccount` = the Admin SDK service account `.json` file |
| Cognito | `userPoolId`, `region`, `clientId` | GenericOidc | notice: login not yet available |
| Generic | `issuer`, `jwksUri`, `audience` | GenericOidc | notice: login not yet available |

TypeScript types that mirror the schemas live in `src/types/identityProviders.ts` (`FirebaseProviderConfig`, `IdpCapability`, etc.). `LoginPage.tsx` of the admin SPA does **not** change the OIDC flow — it only redirects to the backend authorize endpoint. Self-signup for end users is owned by the IdP at `/account/register`, never by client apps.

---

## Swagger / OpenAPI

The `swagger.json` file at the root of the project contains the OpenAPI specification of the current API. It serves as a reference contract for the TypeScript types under `src/types/`.
