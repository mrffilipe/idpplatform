export interface IdPAccessTokenClaims {
  sub?: string
  tid?: string
  mid?: string
  email?: string
  trole: string[]
  prole: string[]
}

function decodeBase64Url(input: string): string {
  const padded = input.replace(/-/g, '+').replace(/_/g, '/')
  const pad = padded.length % 4 === 0 ? '' : '='.repeat(4 - (padded.length % 4))
  return atob(padded + pad)
}

export function parseAccessTokenClaims(accessToken: string): IdPAccessTokenClaims {
  const parts = accessToken.split('.')
  if (parts.length < 2) {
    return { trole: [], prole: [] }
  }

  try {
    const payload = JSON.parse(decodeBase64Url(parts[1])) as Record<string, unknown>
    const trole = normalizeRoleClaim(payload.trole ?? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'])
    const prole = normalizeRoleClaim(payload.prole)

    return {
      sub: typeof payload.sub === 'string' ? payload.sub : undefined,
      tid: typeof payload.tid === 'string' ? payload.tid : undefined,
      mid: typeof payload.mid === 'string' ? payload.mid : undefined,
      email: typeof payload.email === 'string' ? payload.email : undefined,
      trole,
      prole,
    }
  } catch {
    return { trole: [], prole: [] }
  }
}

function normalizeRoleClaim(value: unknown): string[] {
  if (typeof value === 'string') return [value]
  if (Array.isArray(value)) return value.filter((v): v is string => typeof v === 'string')
  return []
}

export function hasTenant(accessToken: string): boolean {
  return Boolean(parseAccessTokenClaims(accessToken).tid)
}

export function requiresOnboarding(accessToken: string): boolean {
  return !hasTenant(accessToken)
}

export function hasTenantRole(accessToken: string, ...roles: string[]): boolean {
  const claims = parseAccessTokenClaims(accessToken)
  if (!claims.tid) return false
  const set = new Set(claims.trole.map((r) => r.toLowerCase()))
  return roles.some((r) => set.has(r.toLowerCase()))
}
