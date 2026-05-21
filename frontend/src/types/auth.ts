/** Alinhado a `SessionStatus` do backend (serializado como string). */
export const SessionStatus = {
  Active: 'Active',
  Revoked: 'Revoked',
  Expired: 'Expired',
} as const

export type SessionStatus = (typeof SessionStatus)[keyof typeof SessionStatus]

export interface AuthTenantSummary {
  tenantId: string
  tenantName: string
  tenantKey: string
  roles: string[]
}

export interface AuthSession {
  sessionId: string
  tenantId?: string | null
  membershipId?: string | null
  clientId?: string | null
  status: SessionStatus
  userAgent?: string | null
  ipAddress?: string | null
  expiresAt: string
  lastActivityAt: string
}

/** `SubscribeTenantBody` — POST /v{version}/auth/subscribe */
export interface SubscribeTenantBody {
  tenantName: string
  tenantKey: string
  planCode?: string | null
  externalCustomerId?: string | null
}

/** `SwitchTenantBody` — POST /v{version}/auth/switch-tenant */
export interface SwitchTenantBody {
  tenantId: string
}
