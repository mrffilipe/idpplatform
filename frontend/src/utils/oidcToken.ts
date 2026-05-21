import type { OidcTokenResponse } from '../types/oidc'

/** API .NET serializa camelCase; o contrato OIDC usa snake_case — aceita ambos. */
export function normalizeOidcTokenResponse(raw: unknown): OidcTokenResponse {
  const record =
    raw !== null && typeof raw === 'object' && !Array.isArray(raw)
      ? (raw as Record<string, unknown>)
      : {}

  const accessToken = String(record.access_token ?? record.accessToken ?? '')
  const refreshToken = String(record.refresh_token ?? record.refreshToken ?? '')
  const expiresIn = Number(record.expires_in ?? record.expiresIn ?? 0)
  const tokenType = String(record.token_type ?? record.tokenType ?? 'Bearer')
  const idToken = record.id_token ?? record.idToken
  const scope = record.scope

  if (!accessToken) {
    throw new Error('Resposta de token inválida: access_token ausente.')
  }

  if (!Number.isFinite(expiresIn) || expiresIn <= 0) {
    throw new Error('Resposta de token inválida: expires_in ausente ou inválido.')
  }

  return {
    access_token: accessToken,
    refresh_token: refreshToken,
    expires_in: expiresIn,
    token_type: tokenType,
    id_token: typeof idToken === 'string' ? idToken : undefined,
    scope: typeof scope === 'string' ? scope : undefined,
  }
}

export function tokenExpiresAtIso(expiresIn: number, fallbackSeconds = 900): string {
  const seconds = Number.isFinite(expiresIn) && expiresIn > 0 ? expiresIn : fallbackSeconds
  return new Date(Date.now() + seconds * 1000).toISOString()
}
