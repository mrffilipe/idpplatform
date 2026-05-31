import type { HttpClient } from '../api/httpClient.js'
import type { ApiPaths } from '../api/paths.js'
import type { TenantContextResult } from '../types.js'

export function createAuthResource(http: HttpClient, paths: ApiPaths) {
  return {
    switchTenant(tenantId: string): Promise<TenantContextResult> {
      return http.request('POST', `${paths.auth}/switch-tenant`, { body: { tenantId } })
    },

    listSessions(): Promise<AuthSessionDto[]> {
      return http.request('GET', `${paths.auth}/sessions`)
    },

    revokeSession(sessionId: string): Promise<void> {
      return http.request('DELETE', `${paths.auth}/sessions/${sessionId}`)
    },
  }
}

export interface AuthSessionDto {
  id: string
  createdAt: string
  lastSeenAt?: string
  userAgent?: string
  ipAddress?: string
  isCurrent: boolean
}

// subscribe intentionally omitted — BFF / .NET only
