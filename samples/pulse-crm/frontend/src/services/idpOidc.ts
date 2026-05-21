import { env } from '../config/env'
import type { OidcTokenResponse } from '../types/oidc'
import { getSession, updateAccessToken } from '../utils/authStorage'
import { jwtHasTenantClaim } from '../utils/jwt'
import { normalizeOidcTokenResponse } from '../utils/oidcToken'
import { generatePkcePair } from '../utils/pkce'

const PKCE_KEY = 'pulsecrm.oidc.verifier'
const STATE_KEY = 'pulsecrm.oidc.state'
const CALLBACK_LOCK = 'pulsecrm.oidc.lock'

export function tryAcquireCallbackLock(): boolean {
  if (sessionStorage.getItem(CALLBACK_LOCK)) return false
  sessionStorage.setItem(CALLBACK_LOCK, '1')
  return true
}

export function releaseCallbackLock(): void {
  sessionStorage.removeItem(CALLBACK_LOCK)
}

export function clearOidcRequest(): void {
  sessionStorage.removeItem(PKCE_KEY)
  sessionStorage.removeItem(STATE_KEY)
  releaseCallbackLock()
}

export async function redirectToLogin(): Promise<void> {
  const { codeVerifier, codeChallenge } = await generatePkcePair()
  sessionStorage.setItem(PKCE_KEY, codeVerifier)

  const state = crypto.randomUUID()
  sessionStorage.setItem(STATE_KEY, state)

  const params = new URLSearchParams({
    client_id: env.idpClientId,
    redirect_uri: env.idpRedirectUri,
    response_type: 'code',
    scope: env.idpScopes,
    code_challenge: codeChallenge,
    code_challenge_method: 'S256',
    state,
  })

  window.location.assign(`${env.idpAuthority}/connect/authorize?${params}`)
}

export function consumeState(returned: string | null): void {
  const expected = sessionStorage.getItem(STATE_KEY)
  sessionStorage.removeItem(STATE_KEY)
  if (!expected || !returned || expected !== returned) {
    throw new Error('OIDC state invalid or expired. Start login again from /login.')
  }
}

export function consumeVerifier(): string {
  const verifier = sessionStorage.getItem(PKCE_KEY)
  sessionStorage.removeItem(PKCE_KEY)
  if (!verifier) throw new Error('PKCE verifier missing. Start login again.')
  return verifier
}

export async function exchangeCode(code: string, verifier: string): Promise<OidcTokenResponse> {
  const body = new URLSearchParams({
    grant_type: 'authorization_code',
    code,
    redirect_uri: env.idpRedirectUri,
    client_id: env.idpClientId,
    code_verifier: verifier,
  })

  const res = await fetch(`${env.idpAuthority}/connect/token`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body,
  })

  if (!res.ok) {
    throw new Error(await res.text())
  }

  return normalizeOidcTokenResponse(await res.json())
}

/** Renova o access token após subscribe/onboarding para incluir claims tid e mid. */
export async function refreshAccessTokenWithTenant(): Promise<OidcTokenResponse> {
  const session = getSession()
  if (!session?.refreshToken) {
    throw new Error(
      'Refresh token ausente. No painel IdP, o client pulse-crm-web precisa do scope offline_access e um novo login.',
    )
  }

  const tokens = await refreshTokens(session.refreshToken)
  updateAccessToken(tokens)

  if (!jwtHasTenantClaim(tokens.access_token)) {
    throw new Error(
      'O token ainda não contém tid. Conclua o pagamento/onboarding ou saia e entre de novo no Pulse CRM.',
    )
  }

  return tokens
}

export async function refreshTokens(refreshToken: string): Promise<OidcTokenResponse> {
  const body = new URLSearchParams({
    grant_type: 'refresh_token',
    refresh_token: refreshToken,
    client_id: env.idpClientId,
  })

  const res = await fetch(`${env.idpAuthority}/connect/token`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body,
  })

  if (!res.ok) {
    throw new Error(await res.text())
  }

  return normalizeOidcTokenResponse(await res.json())
}

export async function fetchUserInfo(accessToken: string): Promise<Record<string, unknown>> {
  const res = await fetch(`${env.idpAuthority}/connect/userinfo`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  })
  if (!res.ok) throw new Error(await res.text())
  return (await res.json()) as Record<string, unknown>
}
