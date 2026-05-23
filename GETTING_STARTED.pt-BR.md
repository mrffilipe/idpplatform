# Getting Started — IdP Platform

[English](./GETTING_STARTED.md) | [Português](./GETTING_STARTED.pt-BR.md)

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

O OIDC usa RS256 (RSA + SHA-256). A solução inclui o utilitário **GenerateOidcKey**, que grava a chave diretamente em `IdPPlatform.API/keys/oidc-signing.pem`:

```bash
cd backend
dotnet run --project tools/GenerateOidcKey/GenerateOidcKey.csproj
```

Alternativa com OpenSSL:

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

> Em produção ou Docker, use variáveis de ambiente no formato `Bootstrap__AdminEmail`, `Bootstrap__AdminPassword`, `Bootstrap__AdminDisplayName` (o `__` representa o aninhamento JSON) e **nunca** coloque credenciais reais no appsettings commitado.

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

## 7. Executando com Docker

Use este caminho para rodar **imagens de container publicadas** em vez de compilar o código. PostgreSQL e Redis são iniciados à parte (compose de infraestrutura opcional ou serviços gerenciados).

**Guia completo:** [docker/README.pt-BR.md](./docker/README.pt-BR.md) (build, push no Docker Hub, variáveis, volumes).

### Pré-requisitos

| Ferramenta | Finalidade |
|------------|------------|
| Docker Engine + Docker Compose v2 | Executar containers |
| Imagens publicadas no Docker Hub | Definir `DOCKERHUB_USERNAME` e `IMAGE_TAG` em `docker/.env` |

Não é necessário .NET SDK nem Node.js no host para **rodar** a aplicação (apenas para **gerar** imagens ou a chave OIDC).

### Visão geral

1. Subir PostgreSQL e Redis — [docker/docker-compose.infrastructure.yml](./docker/docker-compose.infrastructure.yml) ou hosts próprios.
2. Gerar `oidc-signing.pem` (passo 3.2) e configurar JWT em `docker/.env`.
3. Copiar `docker/.env.app.example` → `docker/.env` e preencher connection strings e bootstrap.
4. `docker compose -f docker/docker-compose.yml --env-file docker/.env up -d` (adicione `-f docker/docker-compose.infra-network.yml` se a infra usar a rede compartilhada).
5. Abrir `http://localhost:3000`, fazer bootstrap, remover `Bootstrap__*` do `.env` e reiniciar a API.

### Variáveis de ambiente (Docker)

As variáveis ficam em `docker/.env` (modelo: [docker/.env.app.example](./docker/.env.app.example)). ASP.NET Core usa o formato `Section__Property`.

| Variável | Notas |
|----------|-------|
| `Database__ConnectionString` | `Host=postgres` na rede da infra, ou `Host=host.docker.internal` com DB no host |
| `Redis__ConnectionString` | Mesmo padrão; recomendado em produção |
| `Jwt__Issuer` | URL pública da API (ex.: `http://localhost:5000`) |
| `Jwt__SigningKeyPem` ou `Jwt__SigningKeyPath` | Obrigatório |
| `SecretProtection__KeyDirectoryPath` | Volume Docker `api-dataprotection` |
| `Bootstrap__AdminEmail` / `Bootstrap__AdminPassword` | Só no primeiro deploy |
| `Database__ApplyMigrationsOnStartup` | `true` aplica migrations na subida do container |

A **imagem do frontend** é compilada com `VITE_*` (URL da API, redirect OAuth). Mudar URLs públicas exige rebuild e novo push — não são variáveis de runtime no compose.

Valores padrão da imagem (portas locais do compose):

- `VITE_API_BASE_URL=http://localhost:5000`
- `VITE_OAUTH_REDIRECT_URI=http://localhost:3000/auth/callback`

### Gerar e publicar imagens (mantenedores)

Na raiz do repositório:

```bash
docker build -f backend/Dockerfile -t <usuario>/idpplatform-api:1.0.0 .
docker build -f frontend/Dockerfile \
  --build-arg VITE_API_BASE_URL=http://localhost:5000 \
  --build-arg VITE_OAUTH_REDIRECT_URI=http://localhost:3000/auth/callback \
  -t <usuario>/idpplatform-frontend:1.0.0 .
docker push <usuario>/idpplatform-api:1.0.0
docker push <usuario>/idpplatform-frontend:1.0.0
```

Detalhes de Docker Hub vs GHCR em [docker/README.pt-BR.md](./docker/README.pt-BR.md).

### Problemas comuns (Docker)

| Problema | Solução |
|----------|---------|
| Não conecta ao banco | Verificar `Database__ConnectionString` e o overlay [infra-network](./docker/docker-compose.infra-network.yml) |
| API unhealthy | `docker logs idpplatform-api` — frequentemente falta chave JWT |
| Redirect OAuth incorreto | Rebuild do frontend com `VITE_OAUTH_REDIRECT_URI` correto |

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
| `SecretProtection__ApplicationName` | Nome lógico para isolar o keyring (default `IdPPlatform`) |
| `VITE_API_BASE_URL` | URL pública da API (durante o build do frontend) |
| `VITE_OAUTH_REDIRECT_URI` | URL pública do callback OIDC do frontend |

No `appsettings.json` de produção, o equivalente usa `:` (ex.: `Database:ConnectionString`).

### Build do frontend para produção

A partir do código-fonte:

```bash
cd frontend
# Configure as variáveis VITE_* antes do build (ou confie nos defaults em src/config/env.ts)
npm run build
# Servir a pasta dist/ com nginx, Cloudflare Pages, etc.
```

Com Docker, use os mesmos `VITE_*` como **build-args** na imagem do frontend (veja [seção 7](#7-executando-com-docker) e [docker/README.pt-BR.md](./docker/README.pt-BR.md)).

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
| Docker: rede `idpplatform-infra` não encontrada | Overlay sem compose de infra | Subir [docker-compose.infrastructure.yml](./docker/docker-compose.infrastructure.yml) ou remover o overlay |
| Docker: erro de redirect OAuth | Imagem frontend com `VITE_OAUTH_REDIRECT_URI` errado | Rebuild e push da imagem; conferir client OAuth no admin |
