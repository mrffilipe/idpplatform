# Publicar imagens Docker — mantenedores

[English](./DOCKER_PUBLISH.md) | [Português](./DOCKER_PUBLISH.pt-BR.md)

Este guia é para **mantenedores do repositório** que fazem build e push da imagem do IdP Platform. Operadores que implantam imagens publicadas devem usar [GETTING_STARTED.pt-BR.md § Produção](../GETTING_STARTED.pt-BR.md#7-deploy-em-produção-docker-compose).

---

## Visão geral

| Imagem | Dockerfile | Nome no registry (sugerido) |
|--------|------------|----------------------------|
| IdP Platform (monólito) | [docker/Dockerfile](../docker/Dockerfile) | `mrffilipe/idpplatform` |

O monólito inclui:

- API ASP.NET Core (Kestrel em `127.0.0.1:8080` dentro do container)
- SPA admin (arquivos estáticos; URLs da API/redirect usam **mesma origem** do nginx em runtime)
- nginx (TLS em `:443`, redirect HTTP em `:80`)

Contexto de build: **raiz do repositório**.

Arquivos de suporte:

| Caminho | Finalidade |
|---------|------------|
| [docker/nginx/app.conf](../docker/nginx/app.conf) | TLS + reverse proxy + SPA |
| [docker/scripts/entrypoint-app.sh](../docker/scripts/entrypoint-app.sh) | Migrations, API, nginx |
| [backend/Dockerfile](../backend/Dockerfile) | Referência legada (só API) |
| [frontend/Dockerfile](../frontend/Dockerfile) | Referência legada (só SPA) |

---

## Configuração em runtime (operadores)

O consumidor usa **um** arquivo `.env`. **Não** é necessário rebuild para alterar banco, Redis, JWT (`Jwt__Issuer` deve coincidir com a URL pública), bootstrap, e-mail, etc.

O SPA admin consome a API no **mesmo host** que o nginx expõe (ex.: `https://auth.exemplo.com/v1.0/...`). Defina `Jwt__Issuer` com essa origem pública; **não** use `VITE_*` no `.env` de deploy.

Só uma **nova release** da plataforma (nova tag de imagem) exige pull/build de novo.

---

## CI/CD (GitHub Actions)

Workflow: [.github/workflows/docker-publish.yml](../.github/workflows/docker-publish.yml)

| Item | Valor |
|------|-------|
| **Gatilhos** | Tag `v*`; `workflow_dispatch` |
| **Job** | `build-app` |
| **Registry** | Docker Hub |
| **Imagem** | `${{ secrets.DOCKERHUB_USERNAME }}/idpplatform` |

### Secrets (obrigatórios)

| Secret | Finalidade |
|--------|------------|
| `DOCKERHUB_USERNAME` | Usuário ou org Docker Hub |
| `DOCKERHUB_TOKEN` | Token com permissão de push |

Não são necessárias Variables no GitHub para o workflow.

### Tags (semver)

Na tag `v1.2.3`: `1.2.3`, `1.2`, `1`, `latest`.

Em produção restrita, use `IMAGE_TAG` imutável (ex.: `1.2.3`).

### Release

```bash
git tag v1.0.0
git push origin v1.0.0
```

---

## Build e push manual

Substitua `<versao>` pela tag desejada (ex.: `1.0.0`).

```bash
docker login

docker build -f docker/Dockerfile -t mrffilipe/idpplatform:<versao> .
docker tag mrffilipe/idpplatform:<versao> mrffilipe/idpplatform:latest
docker push mrffilipe/idpplatform:<versao>
docker push mrffilipe/idpplatform:latest
```

---

## GHCR (alternativa)

```bash
docker tag mrffilipe/idpplatform:<versao> ghcr.io/mrffilipe/idpplatform:<versao>
docker push ghcr.io/<owner>/idpplatform:<versao>
```

---

## Operadores

Indique [GETTING_STARTED.pt-BR.md § Deploy em produção](../GETTING_STARTED.pt-BR.md#7-deploy-em-produção-docker-compose) — serviço `app` único, `.env` unificado, certificados em `./certs/`.

---

## Documentação relacionada

- [GETTING_STARTED.pt-BR.md](../GETTING_STARTED.pt-BR.md)
- [backend/README.pt-BR.md](../backend/README.pt-BR.md)
- [frontend/README.pt-BR.md](../frontend/README.pt-BR.md)
