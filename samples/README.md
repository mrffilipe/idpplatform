# Samples — consumer applications

[English](./README.md) | [Português](./README.pt-BR.md)

Integration examples for the IdP Platform (OAuth2 / OIDC).

| Sample | Description |
|--------|-------------|
| [pulse-crm](./pulse-crm/) | Reference SaaS CRM: OIDC login, plan onboarding, `auth/subscribe`, per-tenant CRUD |

## PulseCRM — admin console checklist

Before running the sample, create the following in the admin console (`http://localhost:3000`) under **Applications**:

| Field | Value |
|-------|-------|
| Name | Pulse CRM |
| Slug | `pulse-crm` |
| Type | Web |

Under the application's **Clients**:

| Field | Value |
|-------|-------|
| Client ID | `pulse-crm-web` |
| Type | Public |
| Redirect URIs | `http://localhost:5173/auth/callback` |
| Allowed scopes | `openid profile email offline_access` |

The same values live in [`pulse-crm/frontend/.env.example`](./pulse-crm/frontend/.env.example).

Full guide: [pulse-crm/README.md](./pulse-crm/README.md).
