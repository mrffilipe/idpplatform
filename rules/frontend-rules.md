# Frontend Rules

> Conventions, best practices, and required patterns for the Vite + React 19 admin console (`frontend/`).
> Every contribution MUST conform to these rules.

## 1. Stack

- **Build tool:** Vite 8 (`vite.config.ts`, dev server on port 3000).
- **Framework:** React 19 with the React Compiler enabled in Babel.
- **Routing:** React Router 7 (data mode) — see `src/routes.tsx` and `src/routes/loaders.ts`.
- **UI:** MUI 9 + Emotion. Theme entry: `src/theme/`.
- **HTTP:** Axios via `src/config/axios.ts`.
- **Language:** TypeScript with `strict` mode enabled.

## 2. Folder layout

```
src/
├── components/   Reusable presentational and layout components (AppLayout, AuthLayout, ui/).
├── config/       Cross-cutting wiring: axios instance, env loader, runtime constants.
├── contexts/     React contexts (AuthContext, TenantContext, ThemeModeContext).
├── content/      Static content (provider config presets, copy).
├── pages/        Route components, one per page.
├── routes/       Route loaders and helpers (use with react-router data routes).
├── services/     API service modules and route-aware fetchers. One file per resource.
├── theme/        MUI theme, palette, and CSS variables.
├── types/        Shared TypeScript types and DTOs.
└── utils/        Pure functions.
```

- One component per file. Co-locate `Component.test.tsx`, `Component.module.css`, and small helpers when they only support that component.
- Avoid default exports for shared utilities; use named exports so refactors stay searchable.

## 3. Environment variables

- All `VITE_*` variables are read exclusively via `src/config/env.ts`. Components, services, and contexts import `env` from `../config/env` — never `import.meta.env` directly.
- Every variable MUST have a built-in default in `ENV_DEFAULTS`. The default is the value the local dev stack expects, kept in sync with the backend `PlatformDefaults` constants and `appsettings.Development.json`.
- `.env.example` lists every supported variable and matches `ENV_DEFAULTS` exactly. `.env.development` (if present) overrides defaults locally and is gitignored.
- Treat env values as immutable strings; parse and normalize once at module load (URL trimming, number coercion), then re-export.

## 4. OAuth 2.0 / OIDC flow

- The admin SPA uses **authorization_code + PKCE** with the platform IdP. The endpoint paths under `env.apiBaseUrl` are hardcoded in `src/services/httpPaths.ts`:
  - `/.well-known/openid-configuration`
  - `/connect/authorize`
  - `/connect/token`
  - `/connect/userinfo`
  - `/connect/logout`
- Scopes (`openid profile email offline_access`) are constants in `src/services/oidcService.ts`; do not turn them into env variables.
- The post-logout redirect defaults to `window.location.origin + '/login'` when not provided; respect this fallback inside `buildLogoutUrl`.

## 5. HTTP and services

- Every API call goes through the shared Axios instance in `src/config/axios.ts` so that `baseURL`, timeout, interceptors, and token refresh stay centralized.
- Service files live in `src/services/`, one file per resource (`tenants.ts`, `identityProviders.ts`). Each function returns typed data (`Promise<MyDto>`); errors propagate to the caller.
- Use the `apiPaths` map in `httpPaths.ts` for URL construction. Do not concatenate strings inline.

## 6. State management

- Local component state for view-only data.
- React Context (`AuthContext`, `TenantContext`, `ThemeModeContext`) for cross-cutting state. New global concerns belong in a new context, not in a global module variable.
- React Router loaders (`src/routes/loaders.ts`) fetch route-required data; pages read it via `useLoaderData`.
- No external state libraries (Redux, Zustand) unless a documented design decision is added to this file.

## 7. UI conventions

- Use MUI components first; introduce custom components only when MUI lacks an idiomatic option.
- Keep components presentational; lift side effects to `useEffect` or service calls in the page.
- Light / dark themes are driven by `ThemeModeContext`; new components must work in both palettes (avoid hardcoded colors — use `theme.palette.*`).
- Texts are in English. Localization is not in scope for this app today.

## 8. TypeScript

- `strict: true` and `noUncheckedIndexedAccess`. Do not weaken these.
- Avoid `any`. When the API returns an unknown shape, model it as `unknown` and narrow with a type guard.
- Export DTO types alongside the service that fetches them (`tenants.ts` exports `TenantDto`, `CreateTenantPayload`, etc.).

## 9. Comments

- Comments are in **English** and explain *why*, not *what*. Remove redundant comments when touching a file.

## 10. Build and verification

- `npm run dev` for local development (port 3000).
- `npm run build` runs TypeScript and produces a production bundle. Build must succeed without warnings before opening a PR.
- `npm run preview` to validate the production bundle locally.

## 11. Backend coordination

- Endpoint paths and OAuth client defaults are owned by the backend. When changing them, update the backend constants (`PlatformDefaults`, controllers under `IdPPlatform.API/Controllers/`) **and** the frontend env defaults in the same PR.
- The discovery document (`/.well-known/openid-configuration`) is the source of truth at runtime; if a backend endpoint moves, the frontend should still work as long as discovery is consulted before fixed paths.

## 12. Account creation and signup

- Self-signup is **owned by the IdP** at `/account/register`. The admin SPA and sample client apps do NOT implement their own registration screens; they only render a "Sign in" button that drives the standard OIDC `authorization_code + PKCE` flow.
- The IdP login screen always exposes a "Create account" link to `/account/register`, so users without an account are funnelled through the same place regardless of which application initiated the flow.
- After sign-in, the application detects the absence of a tenant claim (`tid`) in the access token and routes the user into an onboarding flow that calls `POST /v1.0/auth/subscribe`. The frontend never calls a "create user" endpoint directly.

## 13. Identity provider capabilities

- The Identity Providers admin page surfaces the new `IdpCapability` flags (`LocalPassword`, `GoogleSocial`, `MicrosoftSocial`, `AppleSocial`, `GenericOidc`).
- The form must select at least one capability for non-Local providers, lock `LocalPassword` to the Local provider, and surface backend `warnings` in a dismissible alert when present.
- The listing must render capabilities as chips alongside the existing alias/type/status columns.
