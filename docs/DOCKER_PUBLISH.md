# Publishing Docker images — maintainers

[English](./DOCKER_PUBLISH.md) | [Português](./DOCKER_PUBLISH.pt-BR.md)

This guide is for **repository maintainers** who build and push the IdP Platform container image. Operators who deploy published images should use [GETTING_STARTED.md § Production](../GETTING_STARTED.md#7-production-deployment-docker-compose).

---

## Overview

| Image | Dockerfile | Registry name (suggested) |
|-------|------------|---------------------------|
| IdP Platform (monolith) | [docker/Dockerfile](../docker/Dockerfile) | `mrffilipe/idpplatform` |

The monolith includes:

- ASP.NET Core API (Kestrel on `127.0.0.1:8080` inside the container)
- Admin SPA (static files; API/redirect URLs use **same origin** as nginx at runtime)
- nginx (TLS on `:443`, HTTP redirect on `:80`)

Build context is always the **repository root**.

Supporting files:

| Path | Purpose |
|------|---------|
| [docker/nginx/app.conf](../docker/nginx/app.conf) | TLS + reverse proxy + SPA |
| [docker/scripts/entrypoint-app.sh](../docker/scripts/entrypoint-app.sh) | Migrations, API, nginx startup |
| [backend/Dockerfile](../backend/Dockerfile) | Legacy reference (API-only local builds) |
| [frontend/Dockerfile](../frontend/Dockerfile) | Legacy reference (SPA-only local builds) |

---

## Runtime configuration (operators)

Consumers configure **one** `.env` file. No image rebuild is required to change database, Redis, JWT (`Jwt__Issuer` must match the public URL), bootstrap, email, etc.

The admin SPA talks to the API on the **same host** nginx exposes (for example `https://auth.example.com/v1.0/...`). Set `Jwt__Issuer` to that public origin; do not set `VITE_*` in the deploy `.env`.

Only a **new platform release** (new image tag) requires pulling/building again.

---

## CI/CD (GitHub Actions)

Workflow: [.github/workflows/docker-publish.yml](../.github/workflows/docker-publish.yml)

| Setting | Value |
|---------|-------|
| **Triggers** | Git tag push matching `v*` (e.g. `v1.0.0`); manual `workflow_dispatch` |
| **Job** | `build-app` |
| **Registry** | Docker Hub |
| **Image** | `mrffilipe/idpplatform` |

### Repository secrets (required)

| Secret | Purpose |
|--------|---------|
| `DOCKERHUB_TOKEN` | Access token for Docker Hub user **mrffilipe** (push permission) |

Login username is fixed in the workflow (`mrffilipe`); no `DOCKERHUB_USERNAME` secret is required.

No repository variables are required for the workflow (monolith SPA build uses empty `VITE_API_BASE_URL` / `VITE_OAUTH_REDIRECT_URI` for same-origin).

### Tags produced (semver)

On tag `v1.2.3`, `docker/metadata-action` pushes:

- `1.2.3`
- `1.2`
- `1`
- `latest`

Pin consumer deploys to an immutable tag (e.g. `IMAGE_TAG=1.2.3`), not `latest`, in strict production.

### Run CI manually

GitHub → **Actions** → **Docker publish** → **Run workflow**.

### Release with a version tag

```bash
git tag v1.0.0
git push origin v1.0.0
```

---

## Manual build and push

Replace `<version>` with a semver tag (e.g. `1.0.0`).

### 1. Login

```bash
docker login
```

### 2. Build and push

From the repository root:

```bash
docker build -f docker/Dockerfile -t mrffilipe/idpplatform:<version> .
docker tag mrffilipe/idpplatform:<version> mrffilipe/idpplatform:latest

docker push mrffilipe/idpplatform:<version>
docker push mrffilipe/idpplatform:latest
```

The image includes an EF Core **migrations bundle**. At runtime, `Database__ApplyMigrationsOnStartup=true` applies migrations before the API starts.

### 3. Verify

```bash
docker pull mrffilipe/idpplatform:<version>
```

Optional: `docker scout quickview mrffilipe/idpplatform:<version>`

---

## GitHub Container Registry (alternative)

Same Dockerfile; image prefix `ghcr.io/mrffilipe/idpplatform`.

```bash
echo $GITHUB_TOKEN | docker login ghcr.io -u mrffilipe --password-stdin

docker tag mrffilipe/idpplatform:<version> ghcr.io/mrffilipe/idpplatform:<version>
docker push ghcr.io/mrffilipe/idpplatform:<version>
```

PAT needs `write:packages` (and `read:packages` for private images).

---

## Operators (not maintainers)

Share [GETTING_STARTED.md § Production deployment](../GETTING_STARTED.md#7-production-deployment-docker-compose) — single `app` service, unified `.env`, TLS certs in `./certs/`.

---

## Related documentation

- [GETTING_STARTED.md](../GETTING_STARTED.md)
- [backend/README.md](../backend/README.md)
- [frontend/README.md](../frontend/README.md)
