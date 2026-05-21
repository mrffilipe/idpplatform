# PulseCRM API — autenticação e autorização (IdP Platform)

API de exemplo que valida JWTs emitidos pela IdP Platform e chama `POST /v1.0/auth/subscribe` para vincular tenant + plano (`ApplicationTenant`).

## 1. Registrar Application e Client no painel

| Campo | Valor |
|-------|-------|
| Application slug | `pulse-crm` |
| Client ID | `pulse-crm-web` |
| Tipo | Public |
| Redirect URI | `http://localhost:5173/auth/callback` |
| Scopes | `openid profile email offline_access` |

`appsettings.Development.json` deve apontar para o mesmo issuer/audience da plataforma:

```json
"IdP": {
  "Authority": "http://localhost:5000",
  "Audience": "idpplatform-api"
}
```

## 2. Fluxo OAuth2 / OIDC (SPA)

```
1. SPA → GET {Authority}/connect/authorize?client_id=...&code_challenge=...&scope=openid+profile+email+offline_access
2. Usuário autentica em /account/login (IdP)
3. Redirect → http://localhost:5173/auth/callback?code=...&state=...
4. SPA → POST {Authority}/connect/token (authorization_code + code_verifier)
5. SPA armazena access_token + refresh_token
6. SPA → POST PulseCRM /api/onboarding/complete (Bearer access_token)
7. PulseCRM → POST {Authority}/v1.0/auth/subscribe (repassa o mesmo Bearer)
8. SPA → POST /connect/token (refresh_token) para obter JWT com claims tid e mid
```

## 3. Validação JWT nesta API

- **Authority** = issuer (`Jwt:Issuer` da plataforma, ex. `http://localhost:5000`)
- **Audience** = `idpplatform-api` (claim `aud` do access token)
- Chaves públicas via JWKS: `{Authority}/.well-known/jwks.json`

Claims úteis no access token:

| Claim | Uso no CRM |
|-------|------------|
| `uid` / `sub` | Identificador do usuário |
| `email` | Perfil |
| `tid` | Tenant ativo (obrigatório para `/api/contacts`) |
| `mid` | Membership no tenant |
| `trole` | Papéis no tenant |
| `prole` | Papéis de plataforma |

Sem `tid` após login: o usuário ainda não fez subscribe ou não renovou o token.

## 4. `auth/subscribe` e ApplicationTenant

`POST /api/onboarding/complete` chama a plataforma:

```http
POST /v1.0/auth/subscribe
Authorization: Bearer {access_token}
Content-Type: application/json

{
  "tenantName": "Acme Corp",
  "tenantKey": "acme-corp",
  "planCode": "professional",
  "externalCustomerId": "pay_mock_..."
}
```

A plataforma cria:

- **Tenant** + membership (owner) para o usuário da sessão OAuth
- **ApplicationTenant** ligando a application do client `pulse-crm-web` ao tenant, com `planCode` e `externalCustomerId`

O CRM persiste cópia local em SQLite (`Subscriptions`) para exibir plano no dashboard.

## 5. Refresh após subscribe

O access token emitido **antes** do subscribe não contém `tid`. O SPA deve chamar:

```http
POST /connect/token
grant_type=refresh_token&refresh_token=...&client_id=pulse-crm-web
```

Requer scope `offline_access` no client.

## 6. Troubleshooting

| Erro | Causa | Solução |
|------|-------|---------|
| `invalid_scope` / `offline_access` | Client sem scope | Adicionar `offline_access` nos allowed scopes |
| 401 na API CRM | Audience/issuer incorretos | Conferir `IdP:Authority` e `IdP:Audience` |
| Contacts 400 “missing tid” | Token antigo | Refresh token após onboarding |
| Subscribe 403/400 | Sessão sem client OAuth | Login via authorize do client `pulse-crm-web` |

## Endpoints locais

| Método | Rota |
|--------|------|
| GET | `/api/health` |
| GET | `/api/me` |
| POST | `/api/onboarding/complete` |
| GET | `/api/subscription` |
| CRUD | `/api/contacts` |
