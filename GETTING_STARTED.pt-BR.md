# Getting Started — Kyvo

[English](./GETTING_STARTED.md) | [Português](./GETTING_STARTED.pt-BR.md)

> **Pronúncia:** *Kyvo* pronuncia-se como **"Key"vo** — parecido com a palavra inglesa *key* + *vo*.

Guia para rodar o Kyvo em **desenvolvimento** (código-fonte) ou **produção** (imagens Docker publicadas).

### Escolha o caminho

| Caminho | Público | Seções |
|---------|---------|--------|
| **Desenvolvimento** | Você clonou o repositório e roda API e SPA do código-fonte | **1–6** abaixo |
| **Produção** | Você implanta imagens publicadas com Docker Compose (sem build deste repo) | **[§ 7 — Deploy em produção](#7-deploy-em-produção-docker-compose)** |

> **Mantenedores** (build e push de imagens): veja [docs/DOCKER_PUBLISH.pt-BR.md](./docs/DOCKER_PUBLISH.pt-BR.md), não este guia.

---

## Desenvolvimento (seções 1–6)

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
git clone https://github.com/mrffilipe/kyvo.git
cd kyvo
```

---

## 2. Configurar o banco de dados

Crie um banco PostgreSQL para o projeto:

```sql
CREATE DATABASE kyvo_db;
```

Ou via linha de comando:

```bash
createdb kyvo_db
```

---

## 3. Configurar o backend

### 3.1 Editar appsettings de desenvolvimento

No arquivo `backend/Kyvo.API/appsettings.Development.json`, ajuste a string de conexão:

```json
{
  "Database": {
    "ConnectionString": "Host=localhost;Port=5432;Database=kyvo_db;Username=SEU_USUARIO;Password=SUA_SENHA"
  }
}
```

As demais seções já têm valores padrão adequados para desenvolvimento local.

### 3.2 Gerar a chave RSA para assinar os JWTs

O OIDC usa RS256 (RSA + SHA-256). A solução inclui o utilitário **GenerateOidcKey**, que grava a chave diretamente em `Kyvo.API/keys/oidc-signing.pem`:

```bash
cd backend
dotnet run --project tools/GenerateOidcKey/GenerateOidcKey.csproj
```

Alternativa com OpenSSL:

```bash
cd backend/Kyvo.API
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

> Em produção ou Docker, use variáveis de ambiente no formato `Bootstrap__AdminEmail`, `Bootstrap__AdminPassword`, `Bootstrap__AdminDisplayName` (o `__` representa o aninhamento JSON) e **nunca** coloque credenciais reais no appsettings commitado.

### 3.4 Aplicar a migration ao banco

```bash
cd backend

dotnet ef database update \
  --project Kyvo.Infrastructure \
  --startup-project Kyvo.API
```

Isso cria todas as tabelas (`users`, `user_credentials`, `platform_roles`, `identity_providers`, `tenants`, `applications`, `application_clients`, `auth_sessions`, `audit_logs`, etc.).

### 3.5 Iniciar a API

```bash
cd backend
dotnet run --project Kyvo.API
```

A API estará disponível em `http://localhost:5000`. O Swagger fica em `http://localhost:5000/swagger`.

Confirme que está saudável:

```bash
curl http://localhost:5000/v1.0/platform/status
# Resposta esperada: { "isConfigured": false, "requiresBootstrap": true, "oauthClientId": null }
```

---

## 4. Configurar e iniciar o frontend

### 4.1 Criar o arquivo .env (opcional)

```bash
cd frontend
cp .env.example .env
```

O `.env.example` lista as variáveis suportadas; os mesmos valores estão embutidos em `src/config/env.ts` como defaults, então o SPA também roda **sem** um `.env` em ambiente local:

```env
VITE_API_BASE_URL=http://localhost:5000
VITE_API_VERSION=1.0
VITE_API_TIMEOUT_MS=30000
VITE_OAUTH_CLIENT_ID=platform-admin-web
VITE_OAUTH_REDIRECT_URI=http://localhost:3000/auth/callback
```

Não é necessário alterar nada para dev local.

### 4.2 Instalar dependências e iniciar

```bash
cd frontend
npm install
npm run dev
```

O frontend estará em `http://localhost:3000`.

---

## 5. Executar o bootstrap e fazer login

Acesse `http://localhost:3000` (API e frontend rodando).

### Bootstrap (primeira vez)

Se a plataforma ainda não foi inicializada, a tela em `/login` mostra **Inicializar plataforma** em vez do botão de login OIDC. Clique para executar o bootstrap (credenciais lidas do backend — seção `Bootstrap` ou env `Bootstrap__*`).

O bootstrap cria, uma única vez:
- Usuário admin com a senha configurada no appsettings/env vars
- Role de plataforma `plat_admin` atribuída ao admin
- Identity Provider `local` habilitado
- Application `platform-admin` + Client OAuth `platform-admin-web` (fixos, não editáveis via API)

Após sucesso, a mesma rota passa a exibir o login OIDC.

**Alternativa (ops):** com a API rodando, `curl -X POST http://localhost:5000/v1.0/platform/bootstrap`.

Verifique o status:

```bash
curl http://localhost:5000/v1.0/platform/status
# Antes: { "requiresBootstrap": true, ... }
# Depois: { "isConfigured": true, "requiresBootstrap": false, "oauthClientId": "platform-admin-web" }
```

> Após o bootstrap bem-sucedido em produção, remova `Bootstrap__*` do ambiente. Elas não têm mais efeito.

### Login

1. Clique em **"Entrar na plataforma"**
2. Você será redirecionado para `/account/login` no backend (página moderna em Blazor SSR, sem popup para o Google)
3. Informe o email e senha configurados no bootstrap (ex: `admin@localhost` / `SuaSenhaSegura@123`)
4. Após autenticar, o backend redireciona para o callback OIDC
5. O frontend salva os tokens e você acessa o painel

### Self-registration (novos usuários)

Para usuários que ainda NÃO têm conta na plataforma (cenário comum SaaS):

1. A partir de qualquer app cliente (ex.: Pulse CRM) o usuário clica em "Entrar" e é redirecionado para `/connect/authorize`.
2. A página de login do IdP exibe o link **Criar conta** apontando para `/account/register`.
3. O usuário preenche email, senha (respeitando `PasswordPolicy`) e nome. O endpoint é rate-limited pela policy `account_register`.
4. Após o sucesso a plataforma cria `User` + `UserCredential` e autentica o usuário via cookie — NÃO cria tenant nem membership ainda.
5. O usuário é redirecionado de volta para `/connect/authorize`; o app cliente recebe o `code` OIDC.
6. O app detecta ausência de `tid` no access token e dispara seu fluxo de onboarding, chamando `POST /v1.0/auth/subscribe` com tenant + plano para vincular o usuário a um tenant. Após o refresh do token, o novo access token traz `tid` / `mid`.

Esse modelo central significa que apps cliente NUNCA implementam tela própria de cadastro; a coleta de senha acontece apenas no domínio do IdP.

---

## 6. Próximos passos

### Criar um tenant

No painel, vá em **Tenants** → **Criar tenant**. Informe nome e chave única (ex: `minha-org`).

### Convidar membros

Dentro de um tenant, acesse **Tenants** → selecione o tenant → **Convidar membro**. Um link será enviado por e-mail (configure AWS SES em `Email.*` para envio real; em dev o convite é gerado mas não enviado).

### Registrar uma application OAuth

Vá em **Applications** → **Nova application**. Após criar, acesse os detalhes e registre um **Client OAuth** com as redirect URIs da sua aplicação consumidora.

### Adicionar provedores de identidade externos (opcional)

Como platform admin, acesse **Identity Providers** → **Adicionar IdP**. O provedor `local` (bootstrap) permanece habilitado para email/senha.

Os campos sensíveis das credenciais (Firebase `ServiceAccount`, `WebApiKey`, etc.) são armazenados **criptografados em repouso** via ASP.NET Core Data Protection. Os valores em texto puro só são informados na criação/edição e nunca são retornados em endpoints `GET`.

#### Capabilities

Cada identity provider declara uma ou mais flags `IdpCapability`. O formulário admin oferece checkboxes:

| Capability | Permitido em | Política de conflito |
|------------|--------------|----------------------|
| `LocalPassword` | Apenas `Local` (hard-lock) | Somente **um** provider ativo pode anunciá-la. Tentar adicionar segundo falha. |
| `GoogleSocial` | Firebase, Cognito, Generic | Adicionar segundo provider habilitado retorna `warnings` mas é aceito. |
| `MicrosoftSocial` | Firebase, Cognito, Generic | Warning em conflito. |
| `AppleSocial` | Firebase, Cognito, Generic | Warning em conflito. |
| `GenericOidc` | Cognito, Generic | Warning em conflito. |

O hard-lock para `LocalPassword` espelha a prática de IdPs corporativos (Microsoft Entra, etc.): uma única fonte de email/senha mantém account linking determinístico e evita ambiguidade na UI ("qual formulário é o legítimo?"). Os socials são mais flexíveis: cenários legítimos multi-realm rodam dois Google em paralelo; o warning só sinaliza ao admin para conferir.

#### Firebase + Google (login federado funcional)

O Firebase oferece **dois JSONs diferentes**. No painel IdP você monta **um terceiro formato** — só estes três campos na raiz:

| Campo | Origem no Firebase Console | Para quê |
|-------|---------------------------|----------|
| `projectId` | ⚙️ Configurações do projeto → **Geral** → ID do projeto | Identificar o projeto no login Google |
| `webApiKey` | Mesma tela → **Chave da API da Web** | SDK Firebase na página `/account/login` (popup Google) |
| `authDomain` | App Web → `firebaseConfig.authDomain` (ex.: `meu-projeto.firebaseapp.com`) | Obrigatório no SDK; se omitir no JSON, a API usa `{projectId}.firebaseapp.com` |
| `serviceAccount` | Configurações → **Contas de serviço** → Gerar nova chave privada (arquivo `.json`) | Validar o `idToken` no servidor (Admin SDK) |

**Não cole** o `firebaseConfig` / `google-services.json` do app Web inteiro (objeto com `authDomain`, `storageBucket`, etc.). Se você já tem esse trecho no frontend do seu app, use só para conferir `apiKey` → `webApiKey` e o ID do projeto → `projectId`; o `serviceAccount` vem **somente** do arquivo da conta de serviço baixado.

**Modelo de ConfigJson** (substitua pelos seus valores; o objeto `serviceAccount` é o conteúdo completo do arquivo `*-firebase-adminsdk-*.json`):

```json
{
  "projectId": "meu-projeto-firebase",
  "webApiKey": "AIzaSyXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX",
  "authDomain": "meu-projeto-firebase.firebaseapp.com",
  "serviceAccount": {
    "type": "service_account",
    "project_id": "meu-projeto-firebase",
    "private_key_id": "...",
    "private_key": "-----BEGIN PRIVATE KEY-----\n...\n-----END PRIVATE KEY-----\n",
    "client_email": "firebase-adminsdk-xxxxx@meu-projeto-firebase.iam.gserviceaccount.com",
    "client_id": "...",
    "auth_uri": "https://accounts.google.com/o/oauth2/auth",
    "token_uri": "https://oauth2.googleapis.com/token"
  }
}
```

Passos:

1. [Firebase Console](https://console.firebase.google.com/) → **Authentication** → **Sign-in method** → habilitar **Google**.
2. Baixar a chave da **conta de serviço** (Admin SDK) e anotar **Project ID** + **Web API Key** (Geral).
3. Painel admin (`http://localhost:3000`) → **Identity Providers** → **Adicionar IdP** → tipo **Firebase**, alias ex. `firebase`, cole o JSON acima → **Habilitado**.
4. Manter IdP `local` habilitado (bootstrap).
5. Teste: qualquer app OIDC (admin ou Pulse CRM) → redirect → `http://localhost:5000/account/login` → **Continuar com Google**.

**Pulse CRM com Google:** o CRM não integra Firebase diretamente; ele redireciona para o OIDC da plataforma. Com o IdP Firebase habilitado, em `/account/login` o usuário entra com Google, volta ao CRM com `code`, faz onboarding/subscribe e usa a API normalmente. Ver `samples/pulse-crm/backend/README.md`.

**Cognito / Genérico:** cadastro com `ConfigJson` válido; login na página `/account/login` ainda não implementado.

### Integrar uma aplicação consumidora

1. Registre uma **Application** e um **Client OAuth** no painel (redirect URIs da sua app).
2. Use a discovery URL: `http://localhost:5000/.well-known/openid-configuration` (em produção, substitua pelo host público da API).
3. Implemente authorization code + PKCE no seu cliente (SPA, backend, etc.).

---

## 7. Deploy em produção (Docker Compose)

Implante o Kyvo com **imagens de container publicadas**. Não é necessário clonar este repositório (exceto opcionalmente para gerar a chave OIDC com `GenerateOidcKey`).

**PostgreSQL e Redis são obrigatórios** e não estão no exemplo de compose da aplicação abaixo.

### Pré-requisitos

| Ferramenta | Finalidade |
|------------|------------|
| Docker Engine + Docker Compose v2 | Executar containers |
| PostgreSQL + Redis | Acessíveis pelo container do app |
| Imagem publicada no Docker Hub | `mrffilipe/kyvo:<tag>` (defina `IMAGE_TAG` no `.env`) |
| Certificados TLS | `fullchain.pem` e `privkey.pem` em `./certs/` |

Não é necessário .NET SDK nem Node.js no host, salvo para gerar a chave OIDC a partir deste repo.

### Uma URL pública (como funciona o roteamento)

Com `Jwt__Issuer=https://auth.meudominio.com.br` (e TLS nesse host), usuários e o SPA usam a **mesma origem**:

| O que você abre ou chama | URL | Atendido por |
|--------------------------|-----|--------------|
| Painel admin (SPA) | `https://auth.meudominio.com.br/` | nginx → arquivos estáticos |
| API (JSON, OIDC, login) | `https://auth.meudominio.com.br/v1.0/...`, `/connect/...`, `/account/...`, `/.well-known/...` | nginx → Kestrel (`127.0.0.1:8080`) |
| Callback OAuth após login | `https://auth.meudominio.com.br/auth/callback` | nginx → SPA (rota React `/auth/callback`) |

Defina **`Jwt__Issuer`** exatamente como a URL que o navegador usa (esquema + host, sem barra no final). O SPA usa esse mesmo host para API e redirect OAuth automaticamente.

### Diretório de deploy sugerido

Crie uma pasta fora deste repositório (ex.: `kyvo-deploy/`) com:

```
kyvo-deploy/
  docker-compose.yml
  .env
  certs/fullchain.pem
  certs/privkey.pem
  keys/oidc-signing.pem    # opcional com Jwt__SigningKeyPath
```

### PostgreSQL e Redis (infra)

Salve como `docker-compose.infra.yml` na pasta de deploy (ou use serviços gerenciados).

```yaml
# PostgreSQL + Redis locais sugeridos (não versionados no Kyvo)
services:
  postgres:
    image: postgres:16-alpine
    restart: unless-stopped
    environment:
      POSTGRES_USER: ${POSTGRES_USER:-postgres}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD:-postgrespassword}
      POSTGRES_DB: ${POSTGRES_DB:-kyvo_db}
    ports:
      - "${POSTGRES_PORT:-5432}:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER:-postgres} -d ${POSTGRES_DB:-kyvo_db}"]
      interval: 10s
      timeout: 5s
      retries: 5

  redis:
    image: redis:7-alpine
    restart: unless-stopped
    command: >
      redis-server
      --requirepass ${REDIS_PASSWORD:-default_password}
      --appendonly yes
    ports:
      - "${REDIS_PORT:-6379}:6379"
    volumes:
      - redisdata:/data

volumes:
  pgdata:
  redisdata:
```

Exemplo de `.env` para o snippet (mesmo diretório do arquivo acima):

```env
POSTGRES_USER=postgres
POSTGRES_PASSWORD=postgrespassword
POSTGRES_DB=kyvo_db
POSTGRES_PORT=5432
REDIS_PASSWORD=default_password
REDIS_PORT=6379
```

Subir a infra:

```bash
docker compose -f docker-compose.infra.local.yml --env-file .env.infra up -d
```

| Variável de infra | Padrão sugerido | Uso |
|-------------------|-----------------|-----|
| `POSTGRES_USER` | `postgres` | Usuário do banco |
| `POSTGRES_PASSWORD` | (definir senha forte) | Senha do banco |
| `POSTGRES_DB` | `kyvo_db` | Nome do banco |
| `POSTGRES_PORT` | `5432` | Porta no host |
| `REDIS_PASSWORD` | (definir senha forte) | Senha do Redis |
| `REDIS_PORT` | `6379` | Porta no host |

Alinhe `Database__ConnectionString` e `Redis__ConnectionString` no `.env` com esses valores (o exemplo abaixo usa `host.docker.internal` quando a infra publica portas no host).

### `docker-compose.yml` (produção)

Salve como `docker-compose.yml` na pasta de deploy:

```yaml
# Kyvo — imagem monólito (API + SPA admin + proxy HTTPS)
# Exige PostgreSQL e Redis acessíveis pelo container.

services:
  app:
    image: mrffilipe/kyvo:${IMAGE_TAG:-latest}
    container_name: kyvo-app
    restart: unless-stopped
    env_file:
      - path: .env
        required: true
    extra_hosts:
      - "host.docker.internal:host-gateway"
    ports:
      - "${PROXY_HTTP_PORT:-80}:80"
      - "${PROXY_HTTPS_PORT:-443}:443"
    volumes:
      - app-dataprotection:/app/keys/data-protection
      - ./certs:/etc/nginx/certs:ro
      # Descomente para montar chave JWT (Jwt__SigningKeyPath=keys/oidc-signing.pem):
      # - ./keys/oidc-signing.pem:/app/keys/oidc-signing.pem:ro

volumes:
  app-dataprotection:
```

### `.env` (aplicação)

Salve como `.env` ao lado de `docker-compose.yml`:

```env
# Imagem publicada (Docker Hub: mrffilipe/kyvo)
IMAGE_TAG=1.0.0

PROXY_HTTP_PORT=80
PROXY_HTTPS_PORT=443

Database__ConnectionString=Host=host.docker.internal;Port=5432;Database=kyvo_db;Username=postgres;Password=postgrespassword
Database__ApplyMigrationsOnStartup=true

Jwt__Issuer=https://auth.exemplo.com
Jwt__Audience=kyvo-api
Jwt__KeyId=default
Jwt__RefreshTokenDays=30
Jwt__SigningKeyPath=keys/oidc-signing.pem

Redis__ConnectionString=host.docker.internal:6379,password=default_password,ssl=false
Redis__InstanceName=kyvo:
Redis__TenantIdentifierCacheMinutes=5

SecretProtection__KeyDirectoryPath=keys/data-protection
SecretProtection__ApplicationName=Kyvo

Bootstrap__AdminEmail=admin@example.com
Bootstrap__AdminPassword=ChangeMe_Strong_Password_12
Bootstrap__AdminDisplayName=Platform Admin

Email__FromAddress=noreply@example.com
Email__Region=us-east-1
Email__AccessKeyId=
Email__SecretAccessKey=
Email__SessionToken=
```

ASP.NET Core usa `Section__Property`. Em produção **não** defina `VITE_*` no `.env` — a imagem monólito usa **mesma origem** (same-origin).

| Variável | Rebuild da imagem? | Notas |
|----------|-------------------|-------|
| `Database__*`, `Redis__*`, `Jwt__*`, `Bootstrap__*`, `Email__*` | Não | Edite `.env`, depois `docker compose restart app` |
| `Jwt__Issuer` | Não | Deve coincidir com a URL pública (`https://auth.meudominio.com.br`) |
| Código da plataforma | Sim | Nova tag `kyvo` |

Para **desenvolvimento local** (seções 1–6), `VITE_*` em `frontend/.env` continuam válidos com `npm run dev` na porta 3000 e API na 5000.

### Passos de deploy

1. Subir PostgreSQL e Redis (snippet de infra ou gerenciados).
2. Gerar `oidc-signing.pem` (passo 3.2 em desenvolvimento, ou em máquina confiável com este repo).
3. Criar os arquivos da pasta de deploy; colocar certificados em `certs/`.
4. Definir `Jwt__Issuer` com a URL pública `https://` (mesmo host que o usuário abre no navegador).
5. Subir o app:

```bash
cd kyvo-deploy
docker compose --env-file .env up -d
```

6. Abrir `https://seu-host-publico`, fazer bootstrap, remover `Bootstrap__*` do `.env` e reiniciar:

```bash
docker compose --env-file .env restart app
```

### Problemas comuns (produção)

| Problema | Solução |
|----------|---------|
| Não conecta ao banco | Verificar PostgreSQL e `Database__ConnectionString` |
| Container unhealthy ou cai | `docker logs kyvo-app` — chave JWT ou certificados |
| Redirect OAuth incorreto | `Jwt__Issuer` diferente da URL do navegador | Defina `Jwt__Issuer` = URL pública; reinicie o app (o redirect `https://<host>/auth/callback` é registrado no bootstrap e atualizado em `GET /platform/status`) |
| HTTPS não sobe | `fullchain.pem` / `privkey.pem` válidos em `./certs/` |
| SPA chama API errada | `Jwt__Issuer` incorreto | Igualar `Jwt__Issuer` à URL da barra do navegador e `docker compose restart app` |

---

## 8. Configuração para produção

### Variáveis de ambiente críticas

| Variável de ambiente (`__`) | Produção |
|-----------------------------|----------|
| `Database__ConnectionString` | String de conexão ao banco gerenciado (RDS, Cloud SQL, etc.) |
| `Jwt__SigningKeyPem` | Conteúdo PEM da chave privada RSA (inline, sem arquivo) |
| `Jwt__Issuer` | URL pública do backend (ex: `https://auth.meusite.com`) |
| `Bootstrap__AdminEmail` | Apenas no primeiro deploy; remover após bootstrap |
| `Bootstrap__AdminPassword` | Apenas no primeiro deploy; remover após bootstrap |
| `Bootstrap__AdminDisplayName` | Opcional no primeiro deploy |
| `Email__FromAddress`, `Email__Region`, etc. | Configuração AWS SES para convites |
| `Redis__ConnectionString` | Cache distribuído (ElastiCache, Redis Cloud, etc.) |
| `SecretProtection__KeyDirectoryPath` | Diretório persistente para o keyring do data protection (precisa sobreviver a restarts e ser backup) |
| `SecretProtection__ApplicationName` | Nome lógico para isolar o keyring (default `Kyvo`) |
No `appsettings.json` de produção, o equivalente usa `:` (ex.: `Database:ConnectionString`).

### Frontend em produção (monólito)

O SPA admin está na imagem `kyvo` e usa o **mesmo host** que a API. Configure apenas `Jwt__Issuer` no `.env` (seção 7). Para deploy com hosts separados a partir do código-fonte, defina `VITE_*` antes de `npm run build` em `frontend/`.

### HTTPS

Em produção, toda comunicação deve ser via HTTPS. O `Jwt:Issuer` deve usar `https://` para que o OIDC funcione corretamente.

---

## 9. Referência rápida de comandos

```bash
# Backend: aplicar migrations
dotnet ef database update --project Kyvo.Infrastructure --startup-project Kyvo.API

# Backend: gerar nova migration
dotnet ef migrations add NomeDaMigration --project Kyvo.Infrastructure --startup-project Kyvo.API --output-dir Migrations

# Backend: rodar em dev
dotnet run --project backend/Kyvo.API

# Frontend: rodar em dev
cd frontend && npm run dev

# Frontend: build
cd frontend && npm run build

# Chave OIDC (GenerateOidcKey)
dotnet run --project backend/tools/GenerateOidcKey/GenerateOidcKey.csproj

# Bootstrap (com API rodando) — ou use o botão no frontend em /login
curl http://localhost:5000/v1.0/platform/status
curl -X POST http://localhost:5000/v1.0/platform/bootstrap
```

---

## 10. Solução de problemas

| Problema | Causa provável | Solução |
|----------|---------------|---------|
| API não inicia: erro de chave RSA | `keys/oidc-signing.pem` não existe | Gerar com `openssl genpkey` (passo 3.2) |
| Bootstrap retorna 400 | Credenciais não configuradas no appsettings/env | Verificar seção `Bootstrap` ou `Bootstrap__AdminEmail` / `Bootstrap__AdminPassword` |
| Bootstrap retorna "já bootstrapped" | Bootstrap já foi executado | Ignorar; fazer login normalmente |
| Frontend não carrega após login | `VITE_OAUTH_REDIRECT_URI` incorreta | Confirmar que o `redirect_uri` bate com o `platform-admin-web` client |
| JWT expirado / 401 | Token expirado e refresh falhou | Fazer logout e login novamente |
| Convites não chegam por email | AWS SES não configurado | Configurar `Email:*` com credenciais SES válidas |
| Erro de CORS | Frontend em URL diferente | Verificar `VITE_API_BASE_URL` e CORS da API |
| Não decripta IdP existente | Keyring do Data Protection perdido | Restaurar `SecretProtection:KeyDirectoryPath` do backup ou recriar o IdP |
| Docker: não conecta ao PostgreSQL/Redis | Infra parada ou strings erradas | Subir infra ou serviços gerenciados; conferir `Database__*` e `Redis__*` no `.env` de deploy |
| Docker: erro de redirect OAuth | `Jwt__Issuer` ou redirect do client OAuth | `Jwt__Issuer` = URL pública; client `platform-admin-web` com `https://<host>/auth/callback` |
| Docker: HTTPS / esquema OIDC incorreto | Certificados ou `Jwt__Issuer` incorretos | Montar `./certs/` válidos; `Jwt__Issuer` com `https://...` e `restart app` |
