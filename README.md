# IdP Platform

Plataforma de **identidade e acesso (IdP)** para um ecossistema de aplicações: centraliza autenticação local, emite tokens JWT via OIDC, organiza tenants (organizações), membros, papéis, aplicações OAuth e suporta federação de provedores externos (Firebase, Cognito, etc.).

Inspirado no modelo Keycloak-like: um IdP gerenciado, multi-tenant, com painel administrativo próprio.

---

## Primeiros passos

Consulte o guia completo em **[GETTING_STARTED.md](./GETTING_STARTED.md)** para configurar e rodar o sistema do zero.

---

## Estrutura do repositório

```
backend/    API ASP.NET Core 8 — Clean Architecture (Domain / Application / Infrastructure / API)
            Ferramenta auxiliar: tools/GenerateOidcKey (gera chave RSA no diretório da API)
frontend/   Painel admin SPA — React 19 + MUI + React Router 7 + Vite
.github/    Workflows de CI (quando configurados)
```

---

## Documentação

| Documento | Conteúdo |
|-----------|----------|
| [GETTING_STARTED.md](./GETTING_STARTED.md) | Guia passo a passo: configurar, rodar e fazer o bootstrap do zero |
| [backend/README.md](./backend/README.md) | Arquitetura, configuração, endpoints, migrations e OIDC do backend |
| [frontend/README.md](./frontend/README.md) | Stack, variáveis de ambiente, fluxo OIDC e páginas do frontend |

---

## Visão geral do produto

| Conceito | Descrição |
|----------|-----------|
| **IdP local** | Autenticação por email + senha armazenada com BCrypt. Configurado no bootstrap. |
| **OIDC** | Fluxo authorization code + PKCE. Tokens RS256. Discovery em `/.well-known/openid-configuration`. |
| **Multi-tenant** | Usuários pertencem a múltiplos tenants com papéis independentes. |
| **Platform admin** | Usuários com `prole=plat_admin` gerenciam tenants, applications e IdPs globais. |
| **Applications OAuth** | Registro de apps consumidoras com clients públicos (PKCE) ou confidenciais. |
| **Identity Providers** | Federação extensível: Local (padrão), Firebase, Cognito, Genérico. |
| **Audit logs** | Rastreio de eventos por tenant. |

---

## Pré-requisitos rápidos

- .NET 8 SDK
- Node.js (LTS)
- PostgreSQL 14+
- Redis (opcional)

Para o guia completo de instalação: [GETTING_STARTED.md](./GETTING_STARTED.md).
