# Samples — aplicações consumidoras

Exemplos de integração com a IdP Platform (OAuth2 / OIDC).

| Sample | Descrição |
|--------|-----------|
| [pulse-crm](./pulse-crm/) | CRM SaaS de referência: login OIDC, onboarding com plano, `auth/subscribe`, CRUD por tenant |

## PulseCRM — checklist no painel admin

Antes de rodar o sample, crie no painel (`http://localhost:3000`) em **Applications**:

| Campo | Valor |
|-------|-------|
| Nome | Pulse CRM |
| Slug | `pulse-crm` |
| Tipo | Web |

Em **Clients** da application:

| Campo | Valor |
|-------|-------|
| Client ID | `pulse-crm-web` |
| Tipo | Public |
| Redirect URIs | `http://localhost:5173/auth/callback` |
| Scopes permitidos | `openid profile email offline_access` |

Os mesmos valores estão em [`pulse-crm/frontend/.env.example`](./pulse-crm/frontend/.env.example).

Guia completo: [pulse-crm/README.md](./pulse-crm/README.md).
