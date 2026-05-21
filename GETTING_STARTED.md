# Getting Started — IdP Platform

Guia completo para configurar e rodar o IdP Platform do zero em ambiente de desenvolvimento local.

---

## 1. Pré-requisitos

Instale antes de continuar:

| Ferramenta | Como instalar | Versão mínima |
|------------|---------------|---------------|
| .NET SDK | [dotnet.microsoft.com](https://dotnet.microsoft.com/download/dotnet/8.0) | 8.0 |
| Node.js | [nodejs.org](https://nodejs.org/) | LTS atual |
| PostgreSQL | [postgresql.org](https://www.postgresql.org/download/) | 14 |
| Redis | [redis.io](https://redis.io/downloads/) | Opcional (in-memory em dev) |
| dotnet-ef (CLI) | `dotnet tool install --global dotnet-ef` | 8.x |
| openssl | Incluso no macOS/Linux; Windows: Git Bash ou scoop | Qualquer |

Clone o repositório:

```bash
git clone <url-do-repo>
cd idpplatformproject
```

---

## 2. Configurar o banco de dados

Crie um banco PostgreSQL para o projeto:

```sql
CREATE DATABASE idpplatform_db;
```

Ou via linha de comando:

```bash
createdb idpplatform_db
```

---

## 3. Configurar o backend

### 3.1 Editar appsettings de desenvolvimento

No arquivo `backend/IdPPlatform.API/appsettings.Development.json`, ajuste a string de conexão:

```json
{
  "Database": {
    "ConnectionString": "Host=localhost;Port=5432;Database=idpplatform_db;Username=SEU_USUARIO;Password=SUA_SENHA"
  }
}
```

As demais seções já têm valores padrão adequados para desenvolvimento local.

### 3.2 Gerar a chave RSA para assinar os JWTs

O OIDC usa RS256 (RSA + SHA-256). Gere uma chave privada:

```bash
cd backend/IdPPlatform.API
mkdir keys
openssl genpkey -algorithm RSA -pkeyopt rsa_keygen_bits:2048 -out keys/oidc-signing.pem
```

O appsettings.Development.json já aponta para `"SigningKeyPath": "keys/oidc-signing.pem"`. Não commite esta chave.

### 3.3 Configurar credenciais do admin raiz (bootstrap)

As credenciais do primeiro administrador são lidas de variáveis de ambiente **ou** da seção `Bootstrap` do appsettings.Development.json.

Para desenvolvimento, a forma mais simples é editar o appsettings:

```json
{
  "Bootstrap": {
    "AdminEmail": "admin@localhost",
    "AdminPassword": "SuaSenhaSegura@123",
    "AdminDisplayName": "Platform Admin"
  }
}
```

> Em produção, use variáveis de ambiente (`PLATFORM_BOOTSTRAP_ADMIN_EMAIL`, `PLATFORM_BOOTSTRAP_ADMIN_PASSWORD`, `PLATFORM_BOOTSTRAP_ADMIN_DISPLAY_NAME`) e **nunca** coloque credenciais reais no appsettings.

### 3.4 Aplicar a migration ao banco

```bash
cd backend

dotnet ef database update \
  --project IdPPlatform.Infrastructure \
  --startup-project IdPPlatform.API
```

Isso cria todas as tabelas (`users`, `user_credentials`, `platform_roles`, `identity_providers`, `tenants`, `applications`, `application_clients`, `auth_sessions`, `audit_logs`, etc.).

### 3.5 Iniciar a API

```bash
cd backend
dotnet run --project IdPPlatform.API
```

A API estará disponível em `http://localhost:5000`. O Swagger fica em `http://localhost:5000/swagger`.

Confirme que está saudável:

```bash
curl http://localhost:5000/v1.0/platform/status
# Resposta esperada: { "isConfigured": false, "requiresBootstrap": true, "oauthClientId": null }
```

---

## 4. Executar o bootstrap

O bootstrap inicializa a plataforma uma única vez. Com a API rodando:

```bash
curl -X POST http://localhost:5000/v1.0/platform/bootstrap
```

Resposta de sucesso:

```json
{
  "isConfigured": true,
  "rootUserId": "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx",
  "oauthClientId": "platform-admin-web"
}
```

O que o bootstrap cria:
- Usuário admin com a senha configurada no appsettings/env vars
- Role de plataforma `plat_admin` atribuída ao admin
- Identity Provider `local` habilitado
- Application `platform-admin` + Client OAuth `platform-admin-web` (fixos, não editáveis via API)

Verifique o status após o bootstrap:

```bash
curl http://localhost:5000/v1.0/platform/status
# { "isConfigured": true, "requiresBootstrap": false, "oauthClientId": "platform-admin-web" }
```

> Após o bootstrap bem-sucedido em produção, remova as variáveis de ambiente de credenciais. Elas não têm mais efeito.

---

## 5. Configurar e iniciar o frontend

### 5.1 Criar o arquivo .env

```bash
cd frontend
cp .env.example .env
```

O `.env.example` já tem os valores padrão para desenvolvimento local:

```env
VITE_API_BASE_URL=http://localhost:5000
VITE_API_VERSION=1.0
VITE_API_TIMEOUT_MS=30000
VITE_OAUTH_CLIENT_ID=platform-admin-web
VITE_OAUTH_REDIRECT_URI=http://localhost:3000/auth/callback
```

Não é necessário alterar nada para dev local.

### 5.2 Instalar dependências e iniciar

```bash
cd frontend
npm install
npm run dev
```

O frontend estará em `http://localhost:3000`.

---

## 6. Fazer login

Acesse `http://localhost:3000`. Você será redirecionado para a tela de login.

1. Clique em **"Entrar na plataforma"**
2. Você será redirecionado para `/account/login` no backend
3. Informe o email e senha configurados no bootstrap (ex: `admin@localhost` / `SuaSenhaSegura@123`)
4. Após autenticar, o backend redireciona para o callback OIDC
5. O frontend salva os tokens e você acessa o painel

---

## 7. Próximos passos

### Criar um tenant

No painel, vá em **Tenants** → **Criar tenant**. Informe nome e chave única (ex: `minha-org`).

### Convidar membros

Dentro de um tenant, acesse **Tenants** → selecione o tenant → **Convidar membro**. Um link será enviado por e-mail (configure AWS SES em `Email.*` para envio real; em dev o convite é gerado mas não enviado).

### Registrar uma application OAuth

Vá em **Applications** → **Nova application**. Após criar, acesse os detalhes e registre um **Client OAuth** com as redirect URIs da sua aplicação consumidora.

### Adicionar provedores de identidade externos (opcional)

Como platform admin, acesse **Identity Providers** → **Adicionar IdP**. Suporta Firebase, Amazon Cognito e provedores genéricos. O provedor `local` está sempre ativo.

### Integrar uma aplicação consumidora

Consulte `sample-consumer/` para um exemplo completo de integração OIDC. A discovery URL é `http://localhost:5000/.well-known/openid-configuration`.

---

## 8. Configuração para produção

### Variáveis de ambiente críticas

| Variável / Chave | Produção |
|------------------|----------|
| `Database:ConnectionString` | String de conexão ao banco gerenciado (RDS, Cloud SQL, etc.) |
| `Jwt:SigningKeyPem` | Conteúdo PEM da chave privada RSA (inline, sem arquivo) |
| `Jwt:Issuer` | URL pública do backend (ex: `https://auth.meusite.com`) |
| `PLATFORM_BOOTSTRAP_ADMIN_EMAIL` | Apenas no primeiro deploy; remover após bootstrap |
| `PLATFORM_BOOTSTRAP_ADMIN_PASSWORD` | Apenas no primeiro deploy; remover após bootstrap |
| `Email:FromAddress`, `Email:Region`, etc. | Configuração AWS SES para convites |
| `Redis:ConnectionString` | Cache distribuído (ElastiCache, Redis Cloud, etc.) |
| `VITE_API_BASE_URL` | URL pública da API (durante o build do frontend) |
| `VITE_OAUTH_REDIRECT_URI` | URL pública do callback OIDC do frontend |

### Build do frontend para produção

```bash
cd frontend
# Configure as variáveis VITE_* antes do build
npm run build
# Servir a pasta dist/ com nginx, Cloudflare Pages, etc.
```

### HTTPS

Em produção, toda comunicação deve ser via HTTPS. O `Jwt:Issuer` deve usar `https://` para que o OIDC funcione corretamente.

---

## 9. Referência rápida de comandos

```bash
# Backend: aplicar migrations
dotnet ef database update --project IdPPlatform.Infrastructure --startup-project IdPPlatform.API

# Backend: gerar nova migration
dotnet ef migrations add NomeDaMigration --project IdPPlatform.Infrastructure --startup-project IdPPlatform.API --output-dir Migrations

# Backend: rodar em dev
dotnet run --project backend/IdPPlatform.API

# Frontend: rodar em dev
cd frontend && npm run dev

# Frontend: build
cd frontend && npm run build

# Bootstrap (com API rodando)
curl -X POST http://localhost:5000/v1.0/platform/status  # verificar
curl -X POST http://localhost:5000/v1.0/platform/bootstrap  # executar
```

---

## 10. Solução de problemas

| Problema | Causa provável | Solução |
|----------|---------------|---------|
| API não inicia: erro de chave RSA | `keys/oidc-signing.pem` não existe | Gerar com `openssl genpkey` (passo 3.2) |
| Bootstrap retorna 400 | Credenciais não configuradas no appsettings/env | Verificar seção `Bootstrap` ou env vars |
| Bootstrap retorna "já bootstrapped" | Bootstrap já foi executado | Ignorar; fazer login normalmente |
| Frontend não carrega após login | `VITE_OAUTH_REDIRECT_URI` incorreta | Confirmar que o `redirect_uri` bate com o `platform-admin-web` client |
| JWT expirado / 401 | Token expirado e refresh falhou | Fazer logout e login novamente |
| Convites não chegam por email | AWS SES não configurado | Configurar `Email:*` com credenciais SES válidas |
| Erro de CORS | Frontend em URL diferente | Verificar `VITE_API_BASE_URL` e CORS da API |
