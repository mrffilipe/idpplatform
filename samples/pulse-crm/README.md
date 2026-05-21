# PulseCRM — sample consumidor IdP Platform

SPA + API que simulam um CRM SaaS integrado à plataforma: login OIDC, escolha de plano, pagamento mock e vínculo **application ↔ tenant** via `POST /v1.0/auth/subscribe`.

## Pré-requisitos

- IdP Platform rodando (`http://localhost:5000`) com bootstrap concluído
- Application + OAuth client criados no painel (ver [../README.md](../README.md))
- Usuário existente no IdP (login local no bootstrap ou convite)
- .NET 8 SDK e Node.js LTS

## Portas (desenvolvimento)

| Serviço | URL |
|---------|-----|
| IdP Platform | http://localhost:5000 |
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
cp .env.example .env
npm install
npm run dev
```

Abra http://localhost:5173

## Fluxo de teste

1. **Login** — Entrar com IdP Platform (conta já existente no IdP)
2. **Onboarding** — Escolher plano (`starter`, `professional`, `enterprise`)
3. **Pagamento** — Mock aprovado → API chama `auth/subscribe` na plataforma
4. **Dashboard** — Plano contratado + claims do JWT decodificado
5. **Contatos** — CRUD local isolado por tenant (`tid` no token)

Documentação OIDC/JWT do backend: [backend/README.md](./backend/README.md).
