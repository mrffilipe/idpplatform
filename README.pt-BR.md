# IdP Platform

[English](./README.md) | [Português](./README.pt-BR.md)

Plataforma de **identidade e acesso (IdP)** para um ecossistema de aplicações: centraliza autenticação local, emite tokens JWT via OIDC, organiza tenants (organizações), membros, papéis, aplicações OAuth e suporta federação de provedores externos (Firebase, Cognito, etc.).

Inspirado no modelo Keycloak-like: um IdP gerenciado, multi-tenant, com painel administrativo próprio.

---

## Primeiros passos

| Caminho | Uso |
|---------|-----|
| **Desenvolvimento** | Clone o repo e siga [GETTING_STARTED.pt-BR.md](./GETTING_STARTED.pt-BR.md) seções **1–6** (código-fonte, PostgreSQL/Redis local) |
| **Produção** | Imagem monólito `idpplatform` — [GETTING_STARTED.pt-BR.md § Produção](./GETTING_STARTED.pt-BR.md#7-deploy-em-produção-docker-compose) (um serviço, `.env` unificado) |
| **Mantenedores** | Build e push de imagens — [docs/DOCKER_PUBLISH.pt-BR.md](./docs/DOCKER_PUBLISH.pt-BR.md) |

---

## Estrutura do repositório

```
backend/    API ASP.NET Core 8 — Clean Architecture (Domain / Application / Infrastructure / API)
            Ferramenta auxiliar: tools/GenerateOidcKey (gera chave RSA no diretório da API)
frontend/   Painel admin SPA — React 19 + MUI + React Router 7 + Vite
samples/    Aplicações consumidoras de referência (ex.: pulse-crm — CRM SaaS + OIDC)
rules/      Padrões e convenções: backend-rules.md, frontend-rules.md
docker/     Artefatos de build de imagens (nginx, entrypoint da API)
docs/       Guias para mantenedores (ex.: publicação Docker)
.github/    CI (publicação Docker em tags de versão)
```

---

## Documentação

| Documento | Conteúdo |
|-----------|----------|
| [GETTING_STARTED.pt-BR.md](./GETTING_STARTED.pt-BR.md) | Desenvolvimento (§1–6) e deploy Docker em produção (§7) |
| [docs/DOCKER_PUBLISH.pt-BR.md](./docs/DOCKER_PUBLISH.pt-BR.md) | Build/push de imagens: workflow CI e comandos manuais |
| [rules/backend-rules.md](./rules/backend-rules.md) | Convenções do backend, formatação, options pattern, segredos, OIDC |
| [rules/frontend-rules.md](./rules/frontend-rules.md) | Convenções do frontend, variáveis com defaults, fluxo OIDC |
| [backend/README.md](./backend/README.md) | Arquitetura, configuração, endpoints, migrations e OIDC do backend |
| [frontend/README.md](./frontend/README.md) | Stack, variáveis de ambiente, fluxo OIDC e páginas do frontend |
| [samples/README.md](./samples/README.md) | Samples consumidores e checklist de OAuth no painel |
| [samples/pulse-crm/README.md](./samples/pulse-crm/README.md) | PulseCRM: como rodar e testar integração OIDC |

---

## Visão geral do produto

| Conceito | Descrição |
|----------|-----------|
| **IdP local** | Autenticação por email + senha armazenada com BCrypt. Configurado no bootstrap. |
| **OIDC** | Fluxo authorization code + PKCE. Tokens RS256. Discovery em `/.well-known/openid-configuration`. |
| **Multi-tenant** | Usuários pertencem a múltiplos tenants com papéis independentes. |
| **Platform admin** | Usuários com `prole=plat_admin` gerenciam tenants, applications e IdPs globais. |
| **Applications OAuth** | Registro de apps consumidoras com clients públicos (PKCE) ou confidenciais. |
| **Identity Providers** | Federação extensível: Local (padrão), Firebase, Cognito, Genérico. Cada um declara flags `IdpCapability` (LocalPassword, GoogleSocial, etc.) com hard-lock para email/senha e warnings em conflito de socials. |
| **Self-registration** | Página central `/account/register` no IdP, com política de senha configurável e rate limit. Apps cliente redirecionam para OIDC; nunca expõem endpoint próprio de cadastro. |
| **UI moderna de login** | Login e cadastro renderizados via Blazor Web App Static SSR (sem MVC views). Google login usa `signInWithRedirect` (mesma aba, sem popup). |
| **Proteção de segredos** | Credenciais dos IdPs (Firebase ServiceAccount, WebApiKey) são criptografadas em repouso via ASP.NET Core Data Protection. |
| **Audit logs** | Rastreio de eventos por tenant. |

---

## Pré-requisitos rápidos

- .NET 8 SDK
- Node.js (LTS)
- PostgreSQL 14+
- Redis (opcional)

Para o guia completo de instalação: [GETTING_STARTED.pt-BR.md](./GETTING_STARTED.pt-BR.md).
