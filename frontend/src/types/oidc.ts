export interface OidcTokenResponse {
  access_token: string
  refresh_token: string
  expires_in: number
  token_type: string
  id_token?: string
  scope?: string
}

export interface TenantContextResult {
  userId: string
  email: string
  tenantId?: string | null
  membershipId?: string | null
  tenantRoles: string[]
  platformRoles: string[]
  tenants: import('./auth').AuthTenantSummary[]
}
