import type { AuthTenantSummary } from '../types'
import type { OidcTokenResponse, TenantContextResult } from '../types/oidc'
import { tryParseJwtPayload } from './jwt'
import { tokenExpiresAtIso } from './oidcToken'

const SESSION_STORAGE_KEY = 'idp.auth.session'

export interface AuthSessionStorage {
  accessToken: string
  refreshToken: string
  expiresAtIso: string
  userId: string
  email: string
  tenantId?: string | null
  membershipId?: string | null
  tenantRoles: string[]
  platformRoles: string[]
  tenants: AuthTenantSummary[]
}

interface AccessTokenClaims extends Record<string, unknown> {
  sub?: string
  uid?: string
  email?: string
  tid?: string
  mid?: string
  trole?: string | string[]
  prole?: string | string[]
}

function isBrowser(): boolean {
  return typeof window !== 'undefined'
}

function normalizeRoles(value: string | string[] | undefined): string[] {
  if (!value) {
    return []
  }
  return Array.isArray(value) ? value : [value]
}

export function getAuthSession(): AuthSessionStorage | null {
  if (!isBrowser()) {
    return null
  }

  const raw = localStorage.getItem(SESSION_STORAGE_KEY)
  if (!raw) {
    return null
  }

  try {
    return JSON.parse(raw) as AuthSessionStorage
  } catch {
    localStorage.removeItem(SESSION_STORAGE_KEY)
    return null
  }
}

function persist(session: AuthSessionStorage): AuthSessionStorage {
  if (isBrowser()) {
    localStorage.setItem(SESSION_STORAGE_KEY, JSON.stringify(session))
  }
  return session
}

function readClaimsFromOidcTokens(tokens: OidcTokenResponse): AccessTokenClaims | null {
  return tryParseJwtPayload<AccessTokenClaims>(tokens.id_token ?? tokens.access_token)
}

export function saveSessionFromOidcTokens(
  tokens: OidcTokenResponse,
  tenants: AuthTenantSummary[] = [],
): AuthSessionStorage {
  const claims = readClaimsFromOidcTokens(tokens)
  const session: AuthSessionStorage = {
    accessToken: tokens.access_token,
    refreshToken: tokens.refresh_token,
    expiresAtIso: tokenExpiresAtIso(tokens.expires_in),
    userId: String(claims?.uid ?? claims?.sub ?? ''),
    email: String(claims?.email ?? ''),
    tenantId: claims?.tid ? String(claims.tid) : null,
    membershipId: claims?.mid ? String(claims.mid) : null,
    tenantRoles: normalizeRoles(claims?.trole),
    platformRoles: normalizeRoles(claims?.prole),
    tenants,
  }
  return persist(session)
}

export function enrichSessionFromUser(
  user: { id: string; email: string; memberships?: { tenantId: string; tenantName: string; tenantKey: string; roles: string[] }[] },
): AuthSessionStorage {
  const current = getAuthSession()
  if (!current) {
    throw new Error('Sessão não encontrada.')
  }

  return persist({
    ...current,
    userId: user.id,
    email: user.email,
    tenants:
      user.memberships?.map((m) => ({
        tenantId: m.tenantId,
        tenantName: m.tenantName,
        tenantKey: m.tenantKey,
        roles: m.roles,
      })) ?? current.tenants,
  })
}

export function applyTenantContext(context: TenantContextResult): AuthSessionStorage {
  const current = getAuthSession()
  if (!current) {
    throw new Error('Sessão não encontrada.')
  }

  return persist({
    ...current,
    userId: context.userId,
    email: context.email,
    tenantId: context.tenantId,
    membershipId: context.membershipId,
    tenantRoles: context.tenantRoles,
    platformRoles: context.platformRoles,
    tenants: context.tenants,
  })
}

export function updateSessionFromOidcRefresh(tokens: OidcTokenResponse): AuthSessionStorage {
  const current = getAuthSession()
  const claims = readClaimsFromOidcTokens(tokens)
  const updated: AuthSessionStorage = {
    accessToken: tokens.access_token,
    refreshToken: tokens.refresh_token,
    expiresAtIso: tokenExpiresAtIso(tokens.expires_in),
    userId: String(claims?.uid ?? claims?.sub ?? current?.userId ?? ''),
    email: String(claims?.email ?? current?.email ?? ''),
    tenantId: claims?.tid ? String(claims.tid) : current?.tenantId ?? null,
    membershipId: claims?.mid ? String(claims.mid) : current?.membershipId ?? null,
    tenantRoles: normalizeRoles(claims?.trole),
    platformRoles: normalizeRoles(claims?.prole).length > 0
      ? normalizeRoles(claims?.prole)
      : current?.platformRoles ?? [],
    tenants: current?.tenants ?? [],
  }
  return persist(updated)
}

export function clearAuthSession(): void {
  if (isBrowser()) {
    localStorage.removeItem(SESSION_STORAGE_KEY)
  }
}

export function isAuthenticated(): boolean {
  const session = getAuthSession()
  return Boolean(session?.accessToken)
}
