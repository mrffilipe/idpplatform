import { api } from '../config'
import type { AuthSession, SubscribeTenantBody } from '../types'
import type { TenantContextResult } from '../types/oidc'
import { normalizeAuthSession } from '../utils/apiMappers'
import { unwrapArray } from '../utils/apiResponse'
import { apiPaths } from './httpPaths'

export async function subscribeTenant(body: SubscribeTenantBody): Promise<TenantContextResult> {
  const { data } = await api.post<TenantContextResult>(`${apiPaths.auth}/subscribe`, body)
  return data
}

export async function switchTenant(tenantId: string): Promise<TenantContextResult> {
  const { data } = await api.post<TenantContextResult>(`${apiPaths.auth}/switch-tenant`, { tenantId })
  return data
}

export async function listActiveSessions(): Promise<AuthSession[]> {
  const { data } = await api.get(`${apiPaths.auth}/sessions`)
  return unwrapArray(data, normalizeAuthSession)
}

export async function revokeSession(sessionId: string): Promise<void> {
  await api.delete(`${apiPaths.auth}/sessions/${sessionId}`)
}
