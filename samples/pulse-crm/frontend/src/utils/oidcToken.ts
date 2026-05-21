import type { OidcTokenResponse } from '../types/oidc'

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
    throw new Error('Invalid token response: missing access_token')
  }

  if (!Number.isFinite(expiresIn) || expiresIn <= 0) {
    throw new Error('Invalid token response: missing expires_in')
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
