export interface OidcTokenResponse {
  access_token: string
  token_type: string
  expires_in: number
  refresh_token?: string
  id_token?: string
  scope?: string
}

export interface KyvoClientConfig {
  authority: string
  apiVersion?: string
  oidc: {
    clientId: string
    redirectUri: string
    scopes?: string
    storage?: SessionStorageLike
    pkceStorage?: SessionStorageLike
  }
}

export interface SessionStorageLike {
  getItem(key: string): string | null
  setItem(key: string, value: string): void
  removeItem(key: string): void
}

export interface AuthSession {
  accessToken: string
  refreshToken?: string
  expiresAt: number
}

export interface TenantContextResult {
  userId: string
  email: string
  tenantId?: string
  membershipId?: string
  tenantRoles: string[]
  platformRoles: string[]
  tenants: AuthTenantSummary[]
}

export interface AuthTenantSummary {
  tenantId: string
  tenantName: string
  tenantKey: string
  roles: string[]
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
}

export interface ProblemDetails {
  title?: string
  detail?: string
  status?: number
}
