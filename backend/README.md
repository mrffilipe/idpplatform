# IdP Platform — Backend

API .NET 8 que implementa um **Identity Provider (IdP)** completo: autenticação local, OIDC (authorization code + PKCE), multi-tenant, roles, aplicações OAuth e federação de provedores externos.

---

## Arquitetura

A solução segue **Clean Architecture** com 4 projetos:

```
IdPPlatform.Domain          → Entidades, value objects, interfaces de repositório, regras de domínio
IdPPlatform.Application     → Services por agregado, DTOs, requests, interfaces de serviços técnicos
IdPPlatform.Infrastructure  → Implementações: EF Core, OIDC, email (AWS SES), serviços técnicos
IdPPlatform.API             → Controllers ASP.NET Core, Program.cs, middlewares, views MVC (login)
```

### Services por agregado (Application layer)

| Interface | Responsabilidade |
|-----------|-----------------|
| `IPlatformService` | Bootstrap e status da plataforma |
| `IUserService` | Criar/atualizar usuário, listar memberships, linkar identidade externa |
| `ITenantService` | CRUD de tenants, convites, aceitar convite |
| `ITenantRoleService` | CRUD de papéis por tenant |
| `IMembershipService` | Criar/revogar/atualizar memberships |
| `IApplicationService` | CRUD de applications OAuth, criar clients, provisionar tenant |
| `IAuditLogService` | Listagem de audit logs |
| `IAuthService` | Switch/subscribe de tenant, gerenciar sessões |
| `ILocalAuthenticationService` | Login local (email + BCrypt) |
| `IIdentityProviderService` | CRUD de provedores de identidade (Local, Firebase, Cognito…) |

### Fluxo de autenticação

```
POST /account/login (email + senha)
  → SessionCookie com OidcLoginContext

GET /connect/authorize (PKCE)
  → Backend valida cookie, gera authorization_code

POST /connect/token (code + verifier)
  → JWT RS256 (access_token + id_token + refresh_token)

Bearer JWT → controllers v1 protegidos
```

---

## Pré-requisitos

| Ferramenta | Versão |
|------------|--------|
| .NET SDK | 8.0+ |
| PostgreSQL | 14+ |
| Redis | opcional (cache de tenant; sem ele usa in-memory) |
| `dotnet-ef` | `dotnet tool install --global dotnet-ef` |

---

## Configuração

Todas as configurações ficam em `IdPPlatform.API/appsettings.json` (template) e `appsettings.Development.json` (valores de desenvolvimento local).

### Seções do appsettings

| Seção | Chaves principais | Descrição |
|-------|------------------|-----------|
| `Database` | `ConnectionString` | String de conexão PostgreSQL |
| `Jwt` | `Issuer`, `Audience`, `SigningKeyPath`, `SigningKeyPem`, `KeyId`, `AccessTokenMinutes`, `RefreshTokenDays` | Configuração de tokens RS256 |
| `Bootstrap` | `AdminEmail`, `AdminPassword`, `AdminDisplayName` | Credenciais do admin raiz (ver abaixo) |
| `Session` | `MaxSessionsPerUser` | Máximo de sessões simultâneas |
| `RateLimit` | `BootstrapPermitLimit`, `BootstrapWindowMinutes` | Rate limit do endpoint de bootstrap |
| `Invite` | `ExpirationHours` | Validade dos convites |
| `Email` | `FromAddress`, `Region`, `AccessKeyId`, `SecretAccessKey` | AWS SES para envio de convites |
| `Redis` | `ConnectionString`, `InstanceName`, `TenantIdentifierCacheMinutes` | Cache distribuído (opcional) |

### Variáveis de ambiente de bootstrap

As credenciais do admin raiz são lidas **prioritariamente** de variáveis de ambiente (sobrepõem o appsettings):

| Variável | Obrigatória | Descrição |
|----------|-------------|-----------|
| `PLATFORM_BOOTSTRAP_ADMIN_EMAIL` | Sim | Email do administrador raiz |
| `PLATFORM_BOOTSTRAP_ADMIN_PASSWORD` | Sim | Senha inicial (nunca persiste em texto) |
| `PLATFORM_BOOTSTRAP_ADMIN_DISPLAY_NAME` | Não | Nome de exibição (padrão: parte do email) |

> Após o bootstrap, remova essas variáveis de ambiente. Elas só são lidas durante a primeira execução e não afetam o sistema depois.

### Chave RSA para OIDC

O JWT é assinado com RSA (RS256). Gere a chave antes de iniciar:

```bash
cd backend
mkdir -p IdPPlatform.API/keys
openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 -out IdPPlatform.API/keys/oidc-signing.pem
```

Configure `Jwt:SigningKeyPath` com o caminho do arquivo, ou `Jwt:SigningKeyPem` com o conteúdo PEM inline (útil em containers).

---

## Como rodar localmente

```bash
cd backend

# 1. Restaurar dependências
dotnet restore

# 2. Aplicar migration (banco deve existir)
dotnet ef database update \
  --project IdPPlatform.Infrastructure \
  --startup-project IdPPlatform.API

# 3. Iniciar a API
dotnet run --project IdPPlatform.API
```

A API sobe em `http://localhost:5000`. Swagger disponível em `/swagger` nos ambientes Development/Staging.

---

## Bootstrap

O bootstrap inicializa a plataforma pela primeira vez (executado uma única vez):

```bash
# Com as variáveis de ambiente configuradas, chamar:
curl -X POST http://localhost:5000/v1.0/platform/bootstrap

# Retorna:
# { "isConfigured": true, "rootUserId": "...", "oauthClientId": "platform-admin-web" }
```

O bootstrap cria automaticamente:
- Usuário admin raiz com credencial local (BCrypt)
- Role de plataforma `plat_admin` atribuída ao admin
- Identity Provider `local` habilitado
- Application `platform-admin` + Client `platform-admin-web` (fixos, não editáveis via API)
- Registro de `PlatformConfiguration` marcando o sistema como bootstrapped

Verifique o status antes:
```bash
curl http://localhost:5000/v1.0/platform/status
# { "isConfigured": false, "requiresBootstrap": true, "oauthClientId": null }
```

---

## Endpoints principais

### Platform
| Método | Path | Auth | Descrição |
|--------|------|------|-----------|
| GET | `/v1.0/platform/status` | Público | Status e se requer bootstrap |
| POST | `/v1.0/platform/bootstrap` | Público (rate limited) | Inicialização única da plataforma |

### Account / OIDC
| Método | Path | Auth | Descrição |
|--------|------|------|-----------|
| GET/POST | `/account/login` | Público | Login local (cookie MVC) |
| GET/POST | `/connect/authorize` | Cookie | Endpoint de autorização OIDC |
| POST | `/connect/token` | Client credentials | Troca de código por token |
| GET | `/.well-known/openid-configuration` | Público | Discovery OIDC |
| GET | `/.well-known/jwks.json` | Público | Chaves públicas RSA |

### Auth (JWT)
| Método | Path | Auth | Descrição |
|--------|------|------|-----------|
| POST | `/v1.0/auth/subscribe` | JWT | Onboarding SaaS (criar tenant via app OAuth) |
| POST | `/v1.0/auth/switch-tenant` | JWT | Mudar tenant ativo na sessão |
| GET | `/v1.0/auth/sessions` | JWT | Listar sessões ativas |
| DELETE | `/v1.0/auth/sessions/{id}` | JWT | Revogar sessão |

### Users
| Método | Path | Auth | Descrição |
|--------|------|------|-----------|
| GET | `/v1.0/Users/me` | JWT | Perfil do usuário atual |
| PATCH | `/v1.0/Users/me` | JWT | Atualizar perfil |
| GET | `/v1.0/Users/me/memberships` | JWT | Memberships do usuário |

### Identity Providers
| Método | Path | Auth | Descrição |
|--------|------|------|-----------|
| GET | `/v1.0/IdentityProviders` | JWT + plat_admin | Listar IdPs |
| POST | `/v1.0/IdentityProviders` | JWT + plat_admin | Adicionar IdP |
| PATCH | `/v1.0/IdentityProviders/{id}` | JWT + plat_admin | Atualizar IdP |
| POST | `/v1.0/IdentityProviders/{id}/enable` | JWT + plat_admin | Habilitar |
| POST | `/v1.0/IdentityProviders/{id}/disable` | JWT + plat_admin | Desabilitar |

### Tenants, Memberships, Applications, Audit Logs
Ver `frontend/swagger.json` para a lista completa de endpoints.

---

## Autorização

- **Claim `prole=plat_admin`**: administrador de plataforma. Resolvida consultando `UserPlatformRole` + `PlatformRole` no banco.
- **Policy `PlatformAdministrator`**: protege criação de tenants, applications, gestão de IdPs.
- **`trole`**: papéis do tenant ativo (owner, admin, member, viewer).
- **Tenant context**: claims `tid` (tenant id) e `mid` (membership id) no JWT.

---

## Entidades de domínio

| Entidade | Tabela | Descrição |
|----------|--------|-----------|
| `User` | `users` | Usuário da plataforma |
| `UserCredential` | `user_credentials` | Credencial local BCrypt |
| `UserPlatformRole` | `user_platform_roles` | Atribuição de role de plataforma |
| `PlatformRole` | `platform_roles` | Papéis globais (ex: `plat_admin`) |
| `ExternalIdentity` | `external_identities` | Identidade vinculada de IdP externo |
| `IdentityProvider` | `identity_providers` | Configuração de IdP (Local, Firebase…) |
| `Tenant` | `tenants` | Organização / espaço isolado |
| `TenantRole` | `tenant_roles` | Papéis configuráveis por tenant |
| `TenantMembership` | `tenant_memberships` | Vínculo usuário ↔ tenant |
| `Application` | `applications` | Aplicação OAuth registrada |
| `ApplicationClient` | `application_clients` | Client OAuth (public/confidential) |
| `ApplicationTenant` | `application_tenants` | Vínculo app ↔ tenant (provisioning) |
| `AuthSession` | `auth_sessions` | Sessão ativa (vincula cookie a JWT) |
| `AuditLog` | `audit_logs` | Registro de eventos por tenant |
| `TenantInvite` | `tenant_invites` | Convite de membro para tenant |

---

## Migrations

```bash
# Gerar nova migration
dotnet ef migrations add NomeDaMigration \
  --project IdPPlatform.Infrastructure \
  --startup-project IdPPlatform.API \
  --output-dir Migrations

# Aplicar ao banco
dotnet ef database update \
  --project IdPPlatform.Infrastructure \
  --startup-project IdPPlatform.API

# Remover última migration (não aplicada)
dotnet ef migrations remove \
  --project IdPPlatform.Infrastructure \
  --startup-project IdPPlatform.API
```

---

## Estrutura do projeto

```
IdPPlatform.API/
├── Controllers/         Todos os controllers REST + MVC (Account, Authorization, WellKnown)
├── Common/              Base controllers, middlewares, OidcLoginContext
├── Views/Account/       Login.cshtml (formulário local)
├── appsettings.json     Template de configuração
└── Program.cs           Startup (DI, OIDC, policies, rate limiting)

IdPPlatform.Application/
├── Services/            Interfaces e DTOs por agregado
│   ├── AppService/      IApplicationService
│   ├── AuditLog/        IAuditLogService
│   ├── Auth/            IAuthService, IExternalLoginService, DTOs OIDC
│   ├── IdentityProvider/IIdentityProviderService
│   ├── LocalAuthentication/ ILocalAuthenticationService
│   ├── Membership/      IMembershipService
│   ├── Oidc/            IOidcTokenService, IOidcClaimsService, etc.
│   ├── Platform/        IPlatformService
│   ├── Tenant/          ITenantService
│   ├── TenantRoles/     ITenantRoleService
│   └── Users/           IUserService
├── Common/              PagedRequest, PagedResult, ApplicationClientListFields
└── Exceptions/          ApplicationErrorMessages (mensagens estáticas)

IdPPlatform.Infrastructure/
├── Configurations/      JwtOptions, BootstrapOptions, DatabaseOptions, etc.
├── Extensions/          AddInfrastructure, AddAggregateServices, AddRepositories, AddServices
├── Migrations/          FirstMigration
├── Persistence/
│   ├── Configurations/  EF FluentAPI por entidade
│   ├── Repositories/    Implementações dos repositórios
│   └── ApplicationDbContext.cs
└── Services/            Implementações de todos os services
```
