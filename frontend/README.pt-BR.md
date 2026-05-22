# IdP Platform — Frontend

[English](./README.md) | [Português](./README.pt-BR.md)

Painel administrativo (SPA) do IdP Platform. Consome a API via OIDC (authorization code + PKCE) e expõe interface para gestão de tenants, memberships, applications, identity providers e audit logs.

> Padrões e convenções obrigatórias: [../rules/frontend-rules.md](../rules/frontend-rules.md).

---

## Stack

| Tecnologia | Versão | Uso |
|------------|--------|-----|
| React | 19 | UI |
| React Router | 7 (Data Mode) | Roteamento com loaders |
| Material UI | 9 | Design system |
| Axios | 1.x | HTTP client |
| TypeScript | 6 | Tipagem estática |
| Vite | 8 | Build e dev server |

---

## Pré-requisitos

- Node.js (versão compatível com `package.json`)
- Backend rodando em `VITE_API_BASE_URL` (ver configuração)
- Credenciais de bootstrap configuradas no backend (`Bootstrap` no appsettings ou `Bootstrap__*` no ambiente)
- Se a plataforma ainda não foi inicializada, o próprio frontend executa o bootstrap na tela `/login` (botão **Inicializar plataforma**)

---

## Configuração

Toda variável abaixo tem um default embutido em `src/config/env.ts`, então o SPA roda sem `.env` em ambiente local. Para sobrescrever defaults localmente, copie `.env.example` para `.env`:

```bash
cp .env.example .env
```

| Variável | Default | Descrição |
|----------|---------|-----------|
| `VITE_API_BASE_URL` | `http://localhost:5000` | URL base da API backend |
| `VITE_API_VERSION` | `1.0` | Versão da API (gera `/v1.0/...`) |
| `VITE_API_TIMEOUT_MS` | `30000` | Timeout das requisições Axios (ms) |
| `VITE_OAUTH_CLIENT_ID` | `platform-admin-web` | Client OAuth registrado no IdP |
| `VITE_OAUTH_REDIRECT_URI` | `http://localhost:3000/auth/callback` | URI de callback OIDC |

Os defaults são mantidos em sincronia com as constantes do backend (`PlatformDefaults.AdminConsole.ClientId` e `DefaultRedirectUris`) e o `appsettings.Development.json` — mude todos juntos.

---

## Como rodar

```bash
# Instalar dependências
npm install

# Desenvolvimento (porta 3000)
npm run dev

# Build de produção
npm run build

# Preview do build
npm run preview
```

---

## Fluxo de bootstrap e autenticação

```
1. Usuário acessa a aplicação (ex.: / ou /login)
2. loginLoader / requireAuthLoader consultam GET /v1.0/platform/status
3. Se requiresBootstrap → /login exibe botão "Inicializar plataforma" → POST /v1.0/platform/bootstrap
4. Após bootstrap → mesma rota exibe login OIDC
```

### OIDC (após bootstrap)

```
1. Usuário acessa rota protegida
2. requireAuthLoader verifica status e localStorage (idp.auth.session)
3. Se sem sessão → redirect /login?returnUrl=...
4. LoginPage → redirectToOidcLogin()
5. Browser navega para GET /connect/authorize (PKCE, state em sessionStorage)
6. Backend redireciona para /account/login (formulário email + senha)
7. Usuário faz login local → cookie de sessão
8. Backend completa o authorize → redirect /auth/callback?code=...&state=...
9. AuthCallbackPage valida state, POST /connect/token (code + verifier)
10. Tokens salvos em localStorage (idp.auth.session)
11. Redirect para a rota original (returnUrl)
```

O refresh token é trocado automaticamente via interceptor Axios em respostas 401.

O logout limpa o `localStorage` e redireciona para `GET /connect/logout`.

---

## Páginas e rotas

| Rota | Componente | Auth | Descrição |
|------|-----------|------|-----------|
| `/login` | `LoginPage` | Público | Bootstrap (se `requiresBootstrap`) ou inicia fluxo OIDC |
| `/auth/callback` | `AuthCallbackPage` | Público | Troca código por token |
| `/` | `HomePage` | JWT | Dashboard com links para módulos |
| `/profile` | `ProfilePage` | JWT | Perfil e memberships do usuário |
| `/sessions` | `SessionsPage` | JWT | Listar e revogar sessões |
| `/tenants` | `TenantsPage` | JWT | CRUD de tenants, convites, switch de tenant |
| `/memberships` | `MembershipsPage` | JWT | Memberships do tenant ativo |
| `/tenant-roles` | `TenantRolesPage` | JWT | Papéis configuráveis do tenant |
| `/applications` | `ApplicationsPage` | JWT | Listar e criar applications OAuth |
| `/applications/:id` | `ApplicationDetailPage` | JWT | Detalhes, clients OAuth, provisioning |
| `/identity-providers` | `IdentityProvidersPage` | JWT + plat_admin | CRUD de provedores de identidade |
| `/accept-invite` | `AcceptInvitePage` | JWT | Aceitar convite de tenant via token |
| `/audit-logs` | `AuditLogsPage` | JWT | Logs de auditoria com filtros |
| `/jwks` | `JwksPage` | JWT | Exibir JWKS da plataforma |

---

## Estrutura de pastas

```
src/
├── components/
│   ├── AppLayout.tsx       Shell principal com sidebar e topbar
│   ├── AuthLayout.tsx      Layout centralizado para telas de auth
│   └── ui/                 Componentes reutilizáveis (DataTable, PageHeader, etc.)
├── config/
│   ├── axios.ts            Instâncias Axios (api / publicApi) + interceptor 401
│   ├── env.ts              Leitura de variáveis de ambiente com defaults embutidos
│   └── index.ts            Re-exportações
├── contexts/
│   ├── AuthContext.tsx      Estado de autenticação (JWT claims, platform/tenant roles)
│   ├── TenantContext.tsx    Tenant ativo selecionado (localStorage)
│   └── ThemeModeContext.tsx Tema claro/escuro
├── pages/                  Um componente por rota
├── routes/
│   └── loaders.ts          Route loaders (requireAuthLoader, loginLoader)
├── routes.tsx              Definição de todas as rotas com React Router
├── services/               Funções de chamada à API por recurso
├── theme/                  Tokens e createAppTheme (MUI)
├── types/                  Interfaces TypeScript alinhadas ao OpenAPI
└── utils/
    ├── authStorage.ts      Leitura/escrita de sessão no localStorage
    ├── apiError.ts         Extração de mensagem de erro da API
    ├── apiMappers.ts       Normalização de respostas da API (camelCase)
    ├── pkce.ts             Geração de code_verifier e code_challenge
    └── jwt.ts              Decodificação de JWT (sem validação)
```

---

## Autorização frontend

O `AuthContext` expõe `platformRoles` (claim `prole` do JWT). Para verificar se o usuário é platform admin:

```tsx
const { platformRoles } = useAuth()
const isPlatformAdmin = platformRoles.includes('plat_admin')
```

O item de navegação "Identity Providers" e a tela de criação de applications só aparecem para `plat_admin`.

### Identity Providers — schemas `ConfigJson`

A página **Identity Providers** (`IdentityProvidersPage.tsx`) orienta o cadastro por tipo:

| Tipo | Campos no JSON | Observação na UI |
|------|----------------|------------------|
| Local | nenhum obrigatório | sem `ConfigJson` |
| Firebase | `projectId`, `webApiKey`, `serviceAccount` | guia na UI (`FirebaseConfigHelp`): **não** é o `firebaseConfig` do app Web; `serviceAccount` = arquivo `.json` da conta de serviço Admin SDK |
| Cognito | `userPoolId`, `region`, `clientId` | aviso: login ainda não disponível |
| Generic | `issuer`, `jwksUri`, `audience` | aviso: login ainda não disponível |

Tipos TypeScript espelhando os schemas: `src/types/identityProviders.ts` (`FirebaseProviderConfig`, etc.). O `LoginPage.tsx` do painel **não** altera o fluxo OIDC — apenas redireciona para o authorize do backend.

---

## Swagger / OpenAPI

O arquivo `swagger.json` na raiz do projeto contém a especificação OpenAPI da API atual. Serve como contrato de referência para os tipos TypeScript em `src/types/`.
