# Docker deployment — IdP Platform

[English](./README.md) | [Português](./README.pt-BR.md)

Run the IdP Platform from **published container images** without cloning the source tree. PostgreSQL and Redis are **not** bundled with the application compose file; use the optional infrastructure compose or your own managed services.

---

## Layout

| File | Purpose |
|------|---------|
| [`docker-compose.yml`](./docker-compose.yml) | API + admin frontend (images from Docker Hub) |
| [`docker-compose.infrastructure.yml`](./docker-compose.infrastructure.yml) | PostgreSQL + Redis only |
| [`docker-compose.infra-network.yml`](./docker-compose.infra-network.yml) | Optional overlay: API on the same Docker network as infra |
| [`.env.app.example`](./.env.app.example) | Template for application environment variables |
| [`.env.infrastructure.example`](./.env.infrastructure.example) | Template for PostgreSQL / Redis |
| [`../backend/Dockerfile`](../backend/Dockerfile) | API image build |
| [`../frontend/Dockerfile`](../frontend/Dockerfile) | Frontend image build (Vite + nginx) |

---

## Quick start (consumer)

### 1. Start PostgreSQL and Redis

**Option A — infrastructure compose (recommended for local / lab):**

```bash
cp docker/.env.infrastructure.example docker/.env.infrastructure
docker compose -f docker/docker-compose.infrastructure.yml --env-file docker/.env.infrastructure up -d
```

**Option B — managed services:** use your own hosts and skip this step.

### 2. Generate the OIDC signing key (on a trusted machine)

```bash
cd backend
dotnet run --project tools/GenerateOidcKey/GenerateOidcKey.csproj
# Writes IdPPlatform.API/keys/oidc-signing.pem
```

For containers, either:

- Set `Jwt__SigningKeyPem` in `docker/.env` (multiline PEM), or
- Mount the file: create `docker/keys/oidc-signing.pem`, uncomment the volume in [`docker-compose.yml`](./docker-compose.yml), and set `Jwt__SigningKeyPath=keys/oidc-signing.pem`.

### 3. Configure and start the application

```bash
cp docker/.env.app.example docker/.env
# Edit docker/.env: DOCKERHUB_USERNAME, Database__*, Redis__*, Jwt__*, Bootstrap__*, etc.
```

**If infrastructure runs via compose** (service names `postgres` / `redis`):

```bash
docker compose -f docker/docker-compose.yml -f docker/docker-compose.infra-network.yml --env-file docker/.env up -d
```

**If PostgreSQL/Redis listen on the host** (ports published by infra compose or native install):

- Set `Database__ConnectionString` / `Redis__ConnectionString` with `Host=host.docker.internal` (see [`.env.app.example`](./.env.app.example)).
- Start without the infra-network overlay:

```bash
docker compose -f docker/docker-compose.yml --env-file docker/.env up -d
```

### 4. Bootstrap and harden

1. Open `http://localhost:3000` (or your `FRONTEND_HOST_PORT`).
2. Complete platform bootstrap (or `POST http://localhost:5000/v1.0/platform/bootstrap`).
3. Remove `Bootstrap__*` from `docker/.env` and restart the API:  
   `docker compose -f docker/docker-compose.yml --env-file docker/.env restart api`
4. Set `Database__ApplyMigrationsOnStartup=false` after the first successful deploy if you prefer manual migration control.

The frontend image was built with fixed `VITE_*` URLs. The default OAuth client `platform-admin-web` must allow the same `redirect_uri` baked into the image (default `http://localhost:3000/auth/callback`).

---

## Building images (maintainers)

All builds use the **repository root** as context.

### Backend (API)

```bash
docker build -f backend/Dockerfile -t <dockerhub-username>/idpplatform-api:1.0.0 .
docker tag <dockerhub-username>/idpplatform-api:1.0.0 <dockerhub-username>/idpplatform-api:latest
```

The image includes an EF Core **migrations bundle** (`efbundle`). On startup, when `Database__ApplyMigrationsOnStartup=true` (default in `.env.app.example`), the entrypoint applies migrations before starting Kestrel.

### Frontend (admin SPA)

`VITE_*` variables are **baked in at build time**. Rebuild and republish when public URLs change.

```bash
docker build -f frontend/Dockerfile \
  --build-arg VITE_API_BASE_URL=http://localhost:5000 \
  --build-arg VITE_OAUTH_REDIRECT_URI=http://localhost:3000/auth/callback \
  --build-arg VITE_OAUTH_CLIENT_ID=platform-admin-web \
  --build-arg VITE_API_VERSION=1.0 \
  --build-arg VITE_API_TIMEOUT_MS=30000 \
  -t <dockerhub-username>/idpplatform-frontend:1.0.0 .
docker tag <dockerhub-username>/idpplatform-frontend:1.0.0 <dockerhub-username>/idpplatform-frontend:latest
```

Production example with HTTPS:

```bash
docker build -f frontend/Dockerfile \
  --build-arg VITE_API_BASE_URL=https://auth.example.com \
  --build-arg VITE_OAUTH_REDIRECT_URI=https://admin.example.com/auth/callback \
  -t <dockerhub-username>/idpplatform-frontend:1.0.0 .
```

Never pass secrets as build-args; only public URLs and client identifiers.

---

## Publishing to Docker Hub (default registry)

### Prerequisites

1. [Docker Hub](https://hub.docker.com/) account.
2. Repository names (suggested): `idpplatform-api`, `idpplatform-frontend`.

### Login and push

```bash
docker login

docker push <dockerhub-username>/idpplatform-api:1.0.0
docker push <dockerhub-username>/idpplatform-api:latest

docker push <dockerhub-username>/idpplatform-frontend:1.0.0
docker push <dockerhub-username>/idpplatform-frontend:latest
```

### Tagging practices

| Tag | Use |
|-----|-----|
| `1.0.0`, `1.0.1` | Immutable releases (recommended for production) |
| `latest` | Convenience for demos; avoid in strict production without pinning |

Optional: scan images with `docker scout quickview <image>`.

Consumers set in `docker/.env`:

```env
DOCKERHUB_USERNAME=<dockerhub-username>
IMAGE_TAG=1.0.0
```

---

## Alternative: GitHub Container Registry (ghcr.io)

Same Dockerfiles; change the image prefix only.

```bash
echo $GITHUB_TOKEN | docker login ghcr.io -u <github-username> --password-stdin

docker tag <dockerhub-username>/idpplatform-api:1.0.0 ghcr.io/<github-owner>/idpplatform-api:1.0.0
docker push ghcr.io/<github-owner>/idpplatform-api:1.0.0
```

PAT needs `write:packages` (and `read:packages` for private pulls).

| Criterion | Docker Hub | GHCR |
|-----------|------------|------|
| Discovery for operators | Broad | Tied to GitHub |
| CI on GitHub | Supported | Native |
| Free tier limits | Pull rate limits | Generous for GH repos |

To use GHCR in compose, replace `image:` lines with `ghcr.io/<owner>/idpplatform-api:${IMAGE_TAG}` (and adjust env variable names in `.env` if desired).

---

## Environment variables (application)

See [`.env.app.example`](./.env.app.example) for the full template. Summary:

| Variable | Required | Notes |
|----------|----------|-------|
| `Database__ConnectionString` | Yes | Must be reachable from the API container |
| `Jwt__Issuer` | Yes | Public API URL (e.g. `http://localhost:5000`) |
| `Jwt__SigningKeyPem` or `Jwt__SigningKeyPath` | Yes | One of the two |
| `Redis__ConnectionString` | Recommended | Empty → in-memory cache |
| `SecretProtection__KeyDirectoryPath` | Yes | Use default + mounted volume |
| `Bootstrap__*` | First deploy only | Remove after bootstrap |
| `Email__*` | If using invites | AWS SES |
| `Database__ApplyMigrationsOnStartup` | Optional | `true` on first deploy |

Image selection:

| Variable | Example |
|----------|---------|
| `DOCKERHUB_USERNAME` | `myuser` |
| `IMAGE_TAG` | `1.0.0` |

---

## Infrastructure compose

[`.env.infrastructure.example`](./.env.infrastructure.example) defines PostgreSQL and Redis passwords and ports.

Example connection strings for the **shared Docker network** overlay:

```env
Database__ConnectionString=Host=postgres;Port=5432;Database=idpplatform_db;Username=postgres;Password=postgrespassword
Redis__ConnectionString=redis:6379,password=default_password,ssl=false
```

---

## Volumes and persistence

| Volume | Service | Purpose |
|--------|---------|---------|
| `api-dataprotection` | API | ASP.NET Data Protection key ring — **required**; backup with your DB |
| `pgdata` | postgres | Database files |
| `redisdata` | redis | AOF persistence |

Losing `api-dataprotection` makes existing encrypted IdP configuration unreadable.

---

## Health checks

The API health check calls `GET /v1.0/platform/status` on port `8080` inside the container (mapped to `API_HOST_PORT` on the host, default `5000`).

---

## HTTPS in production

- Terminate TLS at a reverse proxy or load balancer.
- Set `Jwt__Issuer` and `VITE_API_BASE_URL` to `https://` URLs and **rebuild** the frontend image.
- Register OAuth redirect URIs that match `VITE_OAUTH_REDIRECT_URI`.

---

## Troubleshooting

| Issue | Likely cause | Action |
|-------|--------------|--------|
| `network idpplatform-infra not found` | Infra overlay without infra compose | Start infrastructure compose first, or drop `-f docker-compose.infra-network.yml` |
| API exits on startup | Missing JWT key or invalid config | Check logs: `docker logs idpplatform-api` |
| Migrations fail | DB unreachable or wrong password | Verify `Database__ConnectionString` and postgres health |
| Frontend login redirect error | `VITE_OAUTH_REDIRECT_URI` mismatch | Rebuild frontend image; fix OAuth client in admin |
| Cannot decrypt IdP config after restart | Lost data-protection volume | Restore volume backup |

---

## Related documentation

- [GETTING_STARTED.md](../GETTING_STARTED.md) — section **Running with Docker**
- [backend/README.md](../backend/README.md) — configuration reference
- [frontend/README.md](../frontend/README.md) — `VITE_*` variables
