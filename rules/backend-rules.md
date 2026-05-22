# Backend Rules

> Conventions, best practices, and required patterns for the .NET 8 backend (`backend/`).
> Every contribution MUST conform to these rules; reviewers are expected to enforce them.

## 1. Project layout (Clean Architecture)

```
backend/
├── IdPPlatform.Domain/          Entities, value objects, repository interfaces, domain errors.
├── IdPPlatform.Application/     Service interfaces, DTOs, application errors, use-case contracts.
├── IdPPlatform.Infrastructure/  EF Core, repository implementations, service implementations, options, validators.
└── IdPPlatform.API/             Controllers, middlewares, view models, API-level error catalog.
```

Dependency direction: `API → Application → Domain`, `Infrastructure → Application → Domain`. Domain depends on nothing inside the solution.

## 2. Files and types

- **One top-level type per file.** Nested types are allowed when they exist only to describe the parent (controller-scoped `*Body` records, private cache DTOs). When the type is general-purpose, move it to its own file alongside its peers.
- **Folder = namespace.** Mirror folders in namespaces: `IdPPlatform.Application/Services/Auth/IAuthService.cs` lives in `IdPPlatform.Application.Services.Auth`.
- **`sealed` by default** for classes that are not part of a designed inheritance hierarchy.

## 3. Formatting

- **Method/constructor parameters**
  - Two or fewer parameters: keep on a single line.
  - More than two parameters: break each parameter onto its own line, with the closing parenthesis on the last parameter's line.
  - Optional `CancellationToken cancellationToken = default` counts as a parameter.
- **Records**
  - Use body syntax (`record Foo { public required T Bar { get; init; } }`), never positional primary constructors. Positional records make call sites fragile to property reordering.
  - Insert a blank line between properties inside a record body.
- **Interfaces**
  - Properties first, then methods, separated by a single blank line.
  - One blank line between method declarations.
- **Classes (entities)**
  - Group every foreign key with its navigation property: `TenantId` immediately followed by `Tenant`. Add a blank line before and after each FK+nav pair so the relationship is visually obvious.
- **Options classes**
  - One blank line between properties for readability.

## 4. Naming

- Interfaces start with `I` (`IUserRepository`, `IAuthService`).
- Async methods end with `Async`.
- Repository methods follow this lexicon:
  - `AddAsync(entity, ct)` — insert a new aggregate.
  - `GetForUpdateAsync(id, ct)` — fetch a tracked aggregate for mutation.
  - `GetByXAsync` / `GetEnabledByXAsync` — single-entity lookups.
  - `ListXAsync` / `ListAllAsync` — multi-entity reads (`IReadOnlyList<T>`).
  - `XAlreadyExistsAsync` / `AnyXAsync` — boolean existence checks.
- Service methods describe the use case (`SubscribeTenantAsync`, `BootstrapAsync`), not the storage operation.

## 5. Repository contract

- **Order of declarations:** `Add` → `Get*`/`List*` → `*AlreadyExists`/`Any*` → (rarely) `Remove`. Concrete implementations MUST mirror that order.
- **No `Update*` methods.** Aggregates returned from `Get*ForUpdate` are tracked by EF Core; mutations go through domain methods (`entity.Rename(...)`, `entity.Disable()`) and are committed by `IUnitOfWork.SaveChangesAsync`.
- Reads of entities that are not subject to mutation use `AsNoTracking()` in the implementation.

## 6. Exception messages

- **Never** hardcode message strings at the throw site. Every message lives in a centralized static catalog:
  - `IdPPlatform.Domain.Exceptions.DomainErrorMessages` for domain rules.
  - `IdPPlatform.Application.Exceptions.ApplicationErrorMessages` for use-case errors.
  - `IdPPlatform.API.Common.ApiErrorMessages` for HTTP-layer messages (ProblemDetails titles and inline UI strings).
- Messages are written in **English** only. Use clear, single-sentence, period-terminated strings.

## 7. Configuration

- Configuration values are bound to strongly typed Options classes in `IdPPlatform.Infrastructure/Configurations/`.
- Each Options class:
  - Exposes a `public const string Section` matching the appsettings key.
  - Has a paired `*OptionsValidator : IValidateOptions<T>` and is registered with `.ValidateOnStart()` in `ServiceCollectionExtensions.AddInfrastructure`.
  - Provides safe defaults in property initializers so the type can be instantiated for diagnostics.
- `appsettings.json` (production template) must list every key that the application reads; `appsettings.Development.json` must mirror the same keys with safe local values.
- Direct `IConfiguration` access (`configuration["..."]`) is allowed only when the value is needed before DI is built (DbContext connection string, distributed cache wiring).
- Environment variables follow ASP.NET Core's `Section__Property` convention; no `Environment.GetEnvironmentVariable` calls in application code.

## 8. Secret protection

- Identity provider configuration JSON (`IdentityProvider.ConfigJson`) is encrypted at rest. Sensitive top-level paths per provider are listed in `IdentityProviderConfigCipher`.
- Encryption goes through `ISecretProtector` (backed by ASP.NET Core Data Protection). Plain-text payloads are still accepted on read for backward compatibility and get re-encrypted on the next write.
- Never serialize a decrypted `ConfigJson` to API consumers. `IdentityProviderDto` MUST omit it.
- The data protection key directory and application name are configured via `SecretProtectionOptions`. Losing the keys means losing access to previously encrypted secrets — back them up alongside the database.

## 9. OAuth 2.0 / OIDC endpoints

- The IdP exposes endpoints under `/connect/*` (`/connect/authorize`, `/connect/token`, `/connect/userinfo`, `/connect/logout`) and `/.well-known/*` (`openid-configuration`, `jwks.json`).
- This layout matches IdentityServer / OpenIddict conventions and is OIDC-compliant because every endpoint is advertised through discovery. Do not change URL paths without simultaneously updating discovery, frontend `httpPaths.ts`, and the sample apps.
- New optional endpoints (`/connect/introspect`, `/connect/revoke`, `/connect/register`) MUST be advertised in the discovery document the same release they are shipped.

## 9.1 Identity provider capabilities

- Every `IdentityProvider` declares one or more `IdpCapability` values (`LocalPassword`, `GoogleSocial`, `MicrosoftSocial`, `AppleSocial`, `GenericOidc`).
- Hard invariant: `LocalPassword` is allowed ONLY for `IdentityProviderType.Local`. The domain (`IdentityProvider`) and the application service (`IdentityProviderService`) BOTH enforce this — never bypass.
- Hard invariant: only **one** enabled provider may advertise `LocalPassword` at a time. `IdentityProviderService.AddAsync` and `EnableAsync` query `ListEnabledByCapabilityAsync(IdpCapability.LocalPassword)` before persisting.
- Soft conflict: when adding a provider that advertises a social capability already advertised by another enabled provider, return a `warnings` payload (do NOT block). Surface the warning to the admin via the API response and the admin console.
- New providers MUST be backfilled with their capabilities via migration when introduced.

## 9.2 Self-registration

- Self-signup is exposed centrally at `/account/register` and implemented by `IRegistrationService`. Consumer applications must NOT expose private signup endpoints; they redirect to `/connect/authorize` which surfaces "Create account" from the IdP login page.
- Password policy is configured via `PasswordPolicyOptions` (section `PasswordPolicy`) and enforced by `IPasswordPolicy`. Defaults: 12 chars, at least one letter and one digit.
- Self-registration creates a `User` + `UserCredential` only. It does NOT create a tenant or membership — that happens after sign-in through `POST /v1.0/auth/subscribe`.
- The endpoint is rate-limited by the `account_register` policy (configurable under `RateLimit:AccountRegister*`).
- Registration is disabled automatically when no IdP with `LocalPassword` is enabled; the service raises `Registration.LocalPasswordDisabled`.

## 9.3 UI rendering

- Server-rendered pages for the IdP UI (login, register, future account screens) are implemented with **Blazor Web App Static Server Rendering** (`AddRazorComponents` + `MapRazorComponents<App>`) under `IdPPlatform.API/Components/`.
- The previous MVC Razor views under `/Views/` were removed. `AddControllersWithViews()` was replaced by `AddControllers()`; controllers are used only for JSON APIs and form POST handlers (`/account/login`, `/account/external-login`, `/account/logout`).
- The legacy Firebase popup flow was replaced by `signInWithRedirect` (full-page redirect to `accounts.google.com`, returns to `/account/login`, JS auto-submits the encoded id token to the controller). NEVER reintroduce popups: they break on mobile, are blocked by browsers, and weaken the phishing signal in the address bar.
- Static assets live in `IdPPlatform.API/wwwroot/`. The account stylesheet is `css/account.css` and uses only CSS variables + `prefers-color-scheme` for theming (no framework, no build step).
- Antiforgery: every form (Blazor `EditForm` or plain HTML) MUST include the antiforgery token. Blazor `EditForm` adds it automatically; plain forms render `<input type="hidden" name="__RequestVerificationToken" value="@_antiforgeryToken" />` populated from `IAntiforgery.GetAndStoreTokens(HttpContext)`.

## 10. Comments and documentation

- All comments are written in **English**. Translate or remove Portuguese comments when touching a file.
- Use XML doc comments (`///`) only when they add non-obvious information: invariants, security implications, lifecycle notes. Do not narrate what the method already says in its name.
- Avoid noise comments (`// loops the list`, `// returns the value`). Trust the reader.

## 11. Error handling

- Throw the most specific domain exception available (`DomainValidationException`, `DomainBusinessRuleException`, `DomainNotFoundException`, `UnauthorizedApplicationException`).
- Application middleware (`ApplicationExceptionMiddleware`) maps exceptions to ProblemDetails responses with `application/problem+json`. Do not handle these exceptions inside controllers.
- Avoid catch-all `catch (Exception)`. When unavoidable (cache misses, optional integrations), log and recover; never swallow silently.

## 12. Dead code policy

- A symbol with zero references (or one reference solely from its concrete implementation of an interface) must be removed in the same PR that exposes it. Configuration keys without a consumer must be removed from both Options classes and appsettings files.

## 13. Tests and validation

- Build the solution (`dotnet build backend/IdPPlatform.slnx`) before every commit; warnings must be triaged.
- When changing options validation, exercise the failure path locally to confirm the validator message is actionable.
- When changing encrypted fields, validate that legacy plain-text payloads in the database still decrypt cleanly (they should: `IdentityProviderConfigCipher.Decrypt` returns plain inputs unchanged).

## 14. Frontend / samples impact

- Any backend change that affects an HTTP contract, OAuth route, or env var default MUST be reflected in `frontend/` and `samples/pulse-crm/frontend/` in the same change.
- Endpoint URL renames require updating discovery, frontend `httpPaths.ts`, sample `idpOidc.ts`, and the matching READMEs.
