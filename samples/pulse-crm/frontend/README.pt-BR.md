# PulseCRM — Frontend (sample)

[English](./README.md) | [Português](./README.pt-BR.md)

SPA consumidora do sample Pulse CRM. Dispara o fluxo OIDC **authorization code + PKCE** contra a Kyvo e chama a API do CRM para onboarding, assinatura e contatos.

> Guia completo (API + fluxo de teste): [../README.pt-BR.md](../README.pt-BR.md)  
> Convenções de código: [../../../rules/frontend-rules.md](../../../rules/frontend-rules.md) (ver §12 — cadastro central na Kyvo).

---

## Stack

| Tecnologia | Uso |
|------------|-----|
| React | UI |
| React Router | Rotas |
| Vite | Dev server e build |
| TypeScript | Tipagem estática |

---

## Pré-requisitos

- Kyvo em `http://localhost:5000` (bootstrap concluído)
- API Pulse CRM em `http://localhost:5100`
- Client OAuth `pulse-crm-web` no painel admin (ver [../../README.pt-BR.md](../../README.pt-BR.md))

---

## Configuração

Os defaults em `src/config/env.ts` permitem rodar sem `.env`. Para sobrescrever, copie `.env.example` para `.env`:

```bash
cp .env.example .env
```

| Variável | Default | Descrição |
|----------|---------|-----------|
| `VITE_KYVO_AUTHORITY` | `http://localhost:5000` | Issuer / authority da Kyvo |
| `VITE_KYVO_CLIENT_ID` | `pulse-crm-web` | Client OAuth público |
| `VITE_KYVO_REDIRECT_URI` | `http://localhost:5173/auth/callback` | Callback OIDC |
| `VITE_KYVO_SCOPES` | `openid profile email offline_access` | Scopes solicitados |
| `VITE_CRM_API_URL` | `http://localhost:5100` | Base URL da API Pulse CRM |

---

## Como rodar

```bash
npm install
npm run dev    # http://localhost:5173
npm run build
npm run preview
```

---

## Autenticação e cadastro

Esta SPA **não tem rota `/register`** nem formulário local de cadastro. A tela de login só redireciona para `/connect/authorize`; login e criação de conta ocorrem no domínio da Kyvo:

- Usuários existentes: `/account/login`
- Novos usuários: link na tela de login da Kyvo para `/account/register` (cadastro central)

Após a primeira troca de tokens, usuários sem claim `tid` vão para **onboarding** → pagamento mock → `auth/subscribe` na plataforma. Ver [../README.pt-BR.md#fluxo-de-teste](../README.pt-BR.md#fluxo-de-teste).

---

## Rotas principais

| Rota | Função |
|------|--------|
| `/login` | Inicia redirect OIDC |
| `/auth/callback` | Troca `code` por tokens |
| `/onboarding` | Plano + nome da empresa |
| `/payment` | Checkout mock → subscribe |
| `/dashboard` | Home pós-assinatura |
| `/contacts` | CRUD por tenant |
