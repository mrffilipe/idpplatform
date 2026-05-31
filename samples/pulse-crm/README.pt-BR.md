# PulseCRM — sample consumidor Kyvo

[English](./README.md) | [Português](./README.pt-BR.md)

SPA + API que simulam um CRM SaaS integrado à plataforma: login OIDC, escolha de plano, pagamento mock e vínculo **application ↔ tenant** via `POST /v1.0/auth/subscribe`.

## Pré-requisitos

- Kyvo com bootstrap concluído (`http://localhost:5000` no código-fonte ou **`https://localhost:8443`** no Docker)
- Application + OAuth client criados no painel (ver [../README.md](../README.md))
- Conta de usuário na Kyvo — admin do bootstrap, convite aceito, OU nova conta criada pela página central **/account/register** da Kyvo (o sample não tem tela própria de cadastro).
- .NET 8 SDK e Node.js LTS

## Portas (desenvolvimento)

| Serviço | URL |
|---------|-----|
| Kyvo | http://localhost:5000 |
| PulseCRM API | http://localhost:5100 |
| PulseCRM SPA | http://localhost:5173 |

## Como rodar

### 1. API CRM

```bash
cd samples/pulse-crm/backend/PulseCrm.Api
dotnet run
```

Swagger: http://localhost:5100/swagger

### 2. Frontend

```bash
cd samples/pulse-crm/frontend
cp .env.example .env   # opcional — defaults estão embutidos em src/config/env.ts
npm install
npm run dev
```

Abra http://localhost:5173

## Kyvo no Docker (`https://localhost:8443`)

1. Cadastre **Pulse CRM** / client `pulse-crm-web` no painel em `https://localhost:8443` (redirect `http://localhost:5173/auth/callback`).
2. Frontend `.env`: `VITE_KYVO_AUTHORITY=https://localhost:8443`
3. API `appsettings.Development.json`:

```json
"Kyvo": {
  "Authority": "https://localhost:8443",
  "Audience": "kyvo-api",
  "AllowInvalidKyvoCertificate": true
}
```

`AllowInvalidKyvoCertificate` permite à API do CRM confiar no certificado autoassinado da Kyvo em dev (JWKS + `auth/subscribe`). **Reinicie** o `dotnet run` após alterar o JSON.

## Fluxo de teste

1. **Login / Criar conta** — a SPA redireciona para `/connect/authorize`. A tela de login da Kyvo permite entrar OU seguir o link para criar conta (`/account/register`). Novos usuários ficam autenticados imediatamente após o cadastro.
2. **Onboarding** — de volta no SPA, a ausência de `tid` direciona o usuário para escolher plano (`starter`, `professional`, `enterprise`) e nome da empresa.
3. **Pagamento** — mock aprovado → a API do CRM chama `auth/subscribe` na plataforma para criar Tenant + Membership + ApplicationTenant.
4. **Refresh do token** — o SPA renova o token para obter claims `tid` / `mid`.
5. **Dashboard** — plano contratado + claims do JWT decodificado.
6. **Contatos** — CRUD local isolado por tenant (`tid` no token).

Documentação OIDC/JWT do backend: [backend/README.md](./backend/README.md).
