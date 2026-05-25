# IdP Platform — Backend

[English](./README.md) | [Português](./README.pt-BR.md)

API .NET 8 que implementa um **Identity Provider (IdP)** completo: autenticação local, OIDC (authorization code + PKCE), multi-tenant, roles, aplicações OAuth e federação de provedores externos.

> Padrões e convenções obrigatórias: [../rules/backend-rules.md](../rules/backend-rules.md).

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
| `IRegistrationService` | Self-registration (User + UserCredential, sem tenant) |
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
POST /account/signin (email + senha)
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
| `Jwt` | `Issuer`, `Audience`, `SigningKeyPath`, `SigningKeyPem`, `KeyId`, `RefreshTokenDays` | Configuração de tokens RS256 |
| `Bootstrap` | `AdminEmail`, `AdminPassword`, `AdminDisplayName` | Credenciais do admin raiz (ver abaixo) |
| `RateLimit` | `BootstrapPermitLimit`, `BootstrapWindowMinutes` | Rate limit do endpoint de bootstrap |
| `Invite` | `ExpirationHours` | Validade dos convites |
| `Email` | `FromAddress`, `Region`, `AccessKeyId`, `SecretAccessKey` | AWS SES para envio de convites |
| `Redis` | `ConnectionString`, `InstanceName`, `TenantIdentifierCacheMinutes` | Cache distribuído (opcional) |
| `SecretProtection` | `KeyDirectoryPath`, `ApplicationName` | Keyring do Data Protection usado para cifrar credenciais de IdP em repouso |
| `PasswordPolicy` | `MinLength`, `RequireDigit`, `RequireLetter` | Política de senha aplicada no self-registration |

Toda Options class tem bind + validação em startup (`IValidateOptions<T>` + `ValidateOnStart()`). Configurações inválidas em produção falham logo na inicialização.

### Variáveis de ambiente (`.env` de produção Docker)

O ASP.NET Core mapeia `Secao__Propriedade` para `Secao:Propriedade` (equivalente ao JSON aninhado). Exemplo para bootstrap:

| Variável | Obrigatória | Descrição |
|----------|-------------|-----------|
| `Bootstrap__AdminEmail` | Sim | Email do administrador raiz |
| `Bootstrap__AdminPassword` | Sim | Senha inicial (nunca persiste em texto) |
| `Bootstrap__AdminDisplayName` | Não | Nome de exibição (padrão: parte do email) |

Outras chaves comuns: `Database__ConnectionString`, `Jwt__Issuer`, `Jwt__SigningKeyPem`, `Redis__ConnectionString`, `Email__FromAddress`, `SecretProtection__KeyDirectoryPath`, etc.

Em desenvolvimento local, a seção `Bootstrap` no `appsettings.Development.json` é suficiente.

> Após o bootstrap, remova `Bootstrap__*` do ambiente em produção. Elas só são necessárias na primeira inicialização.

### Imagem Docker da API

A API faz parte da imagem monólito em [../docker/Dockerfile](../docker/Dockerfile). **Deploy:** [../GETTING_STARTED.pt-BR.md § Produção](../GETTING_STARTED.pt-BR.md#7-deploy-em-produção-docker-compose). **Build/push:** [../docs/DOCKER_PUBLISH.pt-BR.md](../docs/DOCKER_PUBLISH.pt-BR.md). O [`Dockerfile`](./Dockerfile) isolado permanece para build local só da API.

| Tópico | Detalhe |
|--------|---------|
| Porta | `8080` no container; compose mapeia para `5000` no host por padrão |
| Migrations | Bundle EF executado quando `Database__ApplyMigrationsOnStartup=true` |
| Chave JWT | `Jwt__SigningKeyPem` ou volume com `Jwt__SigningKeyPath` |
| Data Protection | Volume em `/app/keys/data-protection` |
| Health | `GET /v1.0/platform/status` na porta `8080` |
| HTTPS | Forwarded Headers para TLS no proxy nginx do monólito |

### Chave RSA para OIDC

O JWT é assinado com RSA (RS256). Gere a chave antes de iniciar.

**Opção recomendada — projeto `GenerateOidcKey` na solução:**

```bash
cd backend
dotnet run --project tools/GenerateOidcKey/GenerateOidcKey.csproj
# Grava IdPPlatform.API/keys/oidc-signing.pem por padrão
```

Caminho customizado: `dotnet run --project tools/GenerateOidcKey/GenerateOidcKey.csproj -- caminho/para/chave.pem`

**Alternativa com OpenSSL:**

```bash
cd backend/IdPPlatform.API
mkdir keys
openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 -out keys/oidc-signing.pem
```

Configure `Jwt:SigningKeyPath` (ou env `Jwt__SigningKeyPath`) com o caminho do arquivo, ou `Jwt:SigningKeyPem` / `Jwt__SigningKeyPem` com o conteúdo PEM inline (útil em containers).

---

## Proteção de segredos em repouso

O JSON de configuração dos IdPs (`IdentityProvider.ConfigJson`) costuma conter segredos (Firebase `ServiceAccount`, `WebApiKey`, etc.). Os campos sensíveis de nível superior são cifrados antes da persistência via ASP.NET Core Data Protection através de `ISecretProtector` e `IdentityProviderConfigCipher`.

- Payloads ainda em texto puro continuam legíveis em runtime e são re-cifrados na próxima escrita.
- Valores cifrados recebem o prefixo `enc:v1:`.
- O keyring é persistido em `SecretProtection:KeyDirectoryPath` (default `keys/data-protection`). **Perder essas chaves significa perder o acesso aos segredos armazenados** — faça backup junto com o banco.

`IdentityProviderDto` propositalmente omite `ConfigJson`; segredos nunca são devolvidos para consumidores da API.

---

## Build da imagem Docker

Na raiz do repositório:

```bash
docker build -f backend/Dockerfile -t mrffilipe/idpplatform-api:<tag> .
```

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

O bootstrap inicializa a plataforma pela primeira vez (executado uma única vez).

**Fluxo recomendado:** com a API e o frontend rodando, acesse `http://localhost:3000`. Se `GET /v1.0/platform/status` indicar `requiresBootstrap: true`, a tela de login exibe o botão **Inicializar plataforma**, que chama `POST /v1.0/platform/bootstrap` (sem body; credenciais vêm só da configuração do backend).

**Alternativa (ops / CI):**

```bash
curl -X POST http://localhost:5000/v1.0/platform/bootstrap
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
| GET | `/account/login` | Público | Página de login (Blazor Web App Static SSR) |
| POST | `/account/signin` | Público | Handler de credencial local (cookie sign-in) |
| GET | `/account/register` | Público | Página de self-registration (Blazor SSR) |
| POST | `/account/register` | Público, rate-limited | Handler de cadastro (cria User + UserCredential) |
| POST | `/account/external-signin` | Público | Handler de login federado (id_token do Firebase) |
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
| POST | `/v1.0/IdentityProviders` | JWT + plat_admin | Adicionar IdP (campos sensíveis do ConfigJson são cifrados ao salvar) |
| PATCH | `/v1.0/IdentityProviders/{id}` | JWT + plat_admin | Atualizar IdP |
| POST | `/v1.0/IdentityProviders/{id}/enable` | JWT + plat_admin | Habilitar |
| POST | `/v1.0/IdentityProviders/{id}/disable` | JWT + plat_admin | Desabilitar |

#### Identity Providers federados

A configuração de cada IdP externo fica em `ConfigJson` no banco (cadastro via painel). **Não há seção `Firebase` em `appsettings`.**

| Tipo | `ConfigJson` (campos principais) | Login em `/account/login` |
|------|----------------------------------|---------------------------|
| `Local` | opcional / vazio | email + senha |
| `Firebase` | `projectId`, `webApiKey`, `authDomain` (opcional), `serviceAccount` | botão Google (Firebase JS + `POST /account/external-signin`) |
| `Cognito` | `userPoolId`, `region`, `clientId` | cadastro validado; login ainda não implementado |
| `Generic` | `issuer`, `jwksUri`, `audience` | cadastro validado; login ainda não implementado |

Fluxo OIDC: o painel admin inicia `connect/authorize` → redirect para `/account/login` → métodos exibidos conforme IdPs **habilitados** → cookie de sessão → retorno ao cliente.

**Firebase / Google:** no [Firebase Console](https://console.firebase.google.com/), no mesmo `projectId` do `ConfigJson`, ative **Authentication** e o provedor **Google**. A `serviceAccount` é o JSON da conta de serviço do Firebase Admin (verificação do `idToken` no servidor).

O `ExternalIdentity.Provider` gravado no banco usa o **alias** do registro (ex.: `firebase`), não uma string fixa global.

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
