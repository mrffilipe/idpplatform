# Deploy com Docker — IdP Platform

[English](./README.md) | [Português](./README.pt-BR.md)

Execute a IdP Platform a partir de **imagens publicadas**, sem clonar o repositório. PostgreSQL e Redis **não** fazem parte do compose da aplicação; use o compose de infraestrutura opcional ou serviços gerenciados.

---

## Estrutura

| Arquivo | Função |
|---------|--------|
| [`docker-compose.yml`](./docker-compose.yml) | API + frontend admin (imagens do Docker Hub) |
| [`docker-compose.infrastructure.yml`](./docker-compose.infrastructure.yml) | Somente PostgreSQL + Redis |
| [`docker-compose.infra-network.yml`](./docker-compose.infra-network.yml) | Overlay opcional: API na mesma rede Docker da infra |
| [`.env.app.example`](./.env.app.example) | Modelo de variáveis da aplicação |
| [`.env.infrastructure.example`](./.env.infrastructure.example) | Modelo de PostgreSQL / Redis |
| [`../backend/Dockerfile`](../backend/Dockerfile) | Build da imagem da API |
| [`../frontend/Dockerfile`](../frontend/Dockerfile) | Build do frontend (Vite + nginx) |

---

## Início rápido (consumidor)

### 1. Subir PostgreSQL e Redis

**Opção A — compose de infraestrutura (recomendado para local / lab):**

```bash
cp docker/.env.infrastructure.example docker/.env.infrastructure
docker compose -f docker/docker-compose.infrastructure.yml --env-file docker/.env.infrastructure up -d
```

**Opção B — serviços gerenciados:** use seus próprios hosts e pule esta etapa.

### 2. Gerar chave de assinatura OIDC (em máquina confiável)

```bash
cd backend
dotnet run --project tools/GenerateOidcKey/GenerateOidcKey.csproj
```

No container, use `Jwt__SigningKeyPem` em `docker/.env` ou monte o arquivo em `docker/keys/oidc-signing.pem` (veja comentário no [`docker-compose.yml`](./docker-compose.yml)).

### 3. Configurar e subir a aplicação

```bash
cp docker/.env.app.example docker/.env
# Edite docker/.env: DOCKERHUB_USERNAME, Database__*, Redis__*, Jwt__*, Bootstrap__*, etc.
```

**Com infra no Docker** (hostnames `postgres` / `redis`):

```bash
docker compose -f docker/docker-compose.yml -f docker/docker-compose.infra-network.yml --env-file docker/.env up -d
```

**Com PostgreSQL/Redis no host** (`host.docker.internal`):

```bash
docker compose -f docker/docker-compose.yml --env-file docker/.env up -d
```

### 4. Bootstrap e endurecimento

1. Acesse `http://localhost:3000`.
2. Conclua o bootstrap da plataforma.
3. Remova `Bootstrap__*` de `docker/.env` e reinicie a API.
4. Opcional: `Database__ApplyMigrationsOnStartup=false` após o primeiro deploy.

O frontend foi compilado com `VITE_*` fixos; o client OAuth `platform-admin-web` deve aceitar o mesmo `redirect_uri` da imagem.

---

## Gerar imagens (mantenedores)

Contexto de build: **raiz do repositório**.

### Backend

```bash
docker build -f backend/Dockerfile -t <usuario-dockerhub>/idpplatform-api:1.0.0 .
docker tag <usuario-dockerhub>/idpplatform-api:1.0.0 <usuario-dockerhub>/idpplatform-api:latest
```

### Frontend

```bash
docker build -f frontend/Dockerfile \
  --build-arg VITE_API_BASE_URL=http://localhost:5000 \
  --build-arg VITE_OAUTH_REDIRECT_URI=http://localhost:3000/auth/callback \
  -t <usuario-dockerhub>/idpplatform-frontend:1.0.0 .
```

Alterar URLs públicas exige **novo build e push** da imagem do frontend.

---

## Publicar no Docker Hub (registry padrão)

```bash
docker login

docker push <usuario-dockerhub>/idpplatform-api:1.0.0
docker push <usuario-dockerhub>/idpplatform-api:latest
docker push <usuario-dockerhub>/idpplatform-frontend:1.0.0
docker push <usuario-dockerhub>/idpplatform-frontend:latest
```

Consumidores configuram em `docker/.env`:

```env
DOCKERHUB_USERNAME=<usuario-dockerhub>
IMAGE_TAG=1.0.0
```

Use tags semver fixas em produção; `latest` apenas para demos.

---

## Alternativa: GitHub Container Registry (ghcr.io)

```bash
echo $GITHUB_TOKEN | docker login ghcr.io -u <usuario-github> --password-stdin
docker tag <usuario>/idpplatform-api:1.0.0 ghcr.io/<owner>/idpplatform-api:1.0.0
docker push ghcr.io/<owner>/idpplatform-api:1.0.0
```

PAT com `write:packages`. Nos compose, troque o prefixo `image:` para `ghcr.io/<owner>/...`.

| Critério | Docker Hub | GHCR |
|----------|------------|------|
| Descoberta | Ampla | Ligada ao GitHub |
| CI no GitHub | Suportado | Nativo |

---

## Variáveis de ambiente

Lista completa em [`.env.app.example`](./.env.app.example).

| Variável | Obrigatório | Notas |
|----------|-------------|-------|
| `Database__ConnectionString` | Sim | Acessível do container da API |
| `Jwt__Issuer` | Sim | URL pública da API |
| `Jwt__SigningKeyPem` ou `Jwt__SigningKeyPath` | Sim | Um dos dois |
| `Redis__ConnectionString` | Recomendado | Vazio → cache em memória |
| `SecretProtection__KeyDirectoryPath` | Sim | Volume `api-dataprotection` |
| `Bootstrap__*` | Só primeiro deploy | Remover depois |
| `Database__ApplyMigrationsOnStartup` | Opcional | `true` no primeiro deploy |

---

## Volumes

| Volume | Serviço | Finalidade |
|--------|---------|------------|
| `api-dataprotection` | API | Keyring do Data Protection — fazer backup |
| `pgdata` | postgres | Dados do banco |
| `redisdata` | redis | Persistência AOF |

---

## HTTPS em produção

TLS no proxy; `Jwt__Issuer` e `VITE_API_BASE_URL` com `https://`; rebuild do frontend; redirect URIs OAuth alinhados.

---

## Problemas comuns

| Problema | Causa | Ação |
|----------|-------|------|
| Rede `idpplatform-infra` não encontrada | Overlay sem infra | Subir infra antes ou remover overlay |
| API não inicia | JWT ou config inválida | `docker logs idpplatform-api` |
| Erro de redirect no login | `VITE_OAUTH_REDIRECT_URI` | Rebuild da imagem frontend |
| IdP não decripta | Volume perdido | Restaurar backup do volume |

---

## Documentação relacionada

- [GETTING_STARTED.pt-BR.md](../GETTING_STARTED.pt-BR.md) — seção **Executando com Docker**
- [backend/README.pt-BR.md](../backend/README.pt-BR.md)
- [frontend/README.pt-BR.md](../frontend/README.pt-BR.md)
