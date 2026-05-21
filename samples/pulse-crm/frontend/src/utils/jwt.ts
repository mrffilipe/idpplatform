export function parseJwtPayload(token: string): Record<string, unknown> {
  const parts = token.split('.')
  if (parts.length !== 3) {
    throw new Error('Invalid JWT')
  }

  const base64 = parts[1].replace(/-/g, '+').replace(/_/g, '/')
  const json = decodeURIComponent(
    atob(base64)
      .split('')
      .map((c) => `%${`00${c.charCodeAt(0).toString(16)}`.slice(-2)}`)
      .join(''),
  )

  return JSON.parse(json) as Record<string, unknown>
}

export function jwtHasTenantClaim(token: string): boolean {
  try {
    const payload = parseJwtPayload(token)
    const tid = payload.tid
    return typeof tid === 'string' && tid.length > 0
  } catch {
    return false
  }
}
