import { env } from '../config'
import type { OidcTokenResponse } from '../types/oidc'
import { apiPaths } from './httpPaths'
import { generatePkcePair } from '../utils/pkce'

const OIDC_SCOPES = 'openid profile email offline_access'
const PKCE_VERIFIER_KEY = 'oidc.code_verifier'
const OIDC_STATE_KEY = 'oidc.state'
const OIDC_CALLBACK_LOCK_KEY = 'oidc.callback.lock'

/** Evita processar o callback duas vezes (React Strict Mode em dev). */
export function tryAcquireOidcCallbackLock(): boolean {
  if (sessionStorage.getItem(OIDC_CALLBACK_LOCK_KEY)) {
    return false
  }

  sessionStorage.setItem(OIDC_CALLBACK_LOCK_KEY, '1')
  return true
}

export function releaseOidcCallbackLock(): void {
  sessionStorage.removeItem(OIDC_CALLBACK_LOCK_KEY)
}

export function clearOidcLoginRequest(): void {
  sessionStorage.removeItem(PKCE_VERIFIER_KEY)
  sessionStorage.removeItem(OIDC_STATE_KEY)
  releaseOidcCallbackLock()
}

function getOidcOrigin(): string {
  return env.apiBaseUrl.replace(/\/$/, '')
}

export async function redirectToOidcLogin(): Promise<void> {
  const { codeVerifier, codeChallenge } = await generatePkcePair()
  sessionStorage.setItem(PKCE_VERIFIER_KEY, codeVerifier)

  const state = crypto.randomUUID()
  sessionStorage.setItem(OIDC_STATE_KEY, state)

  const params = new URLSearchParams({
    client_id: env.oauthClientId,
    redirect_uri: env.oauthRedirectUri,
    response_type: 'code',
    scope: OIDC_SCOPES,
    code_challenge: codeChallenge,
    code_challenge_method: 'S256',
    state,
  })

  window.location.assign(`${getOidcOrigin()}${apiPaths.connectAuthorize}?${params.toString()}`)
}

export function consumeOidcState(returnedState: string | null): void {
  const expected = sessionStorage.getItem(OIDC_STATE_KEY)
  sessionStorage.removeItem(OIDC_STATE_KEY)
  if (!expected || !returnedState) {
    throw new Error('Sessão de login expirada. Volte em /login e inicie o fluxo novamente (não atualize esta página).')
  }

  if (expected !== returnedState) {
    throw new Error('State OIDC inválido (possível CSRF).')
  }
}

export function consumePkceVerifier(): string {
  const verifier = sessionStorage.getItem(PKCE_VERIFIER_KEY)
  sessionStorage.removeItem(PKCE_VERIFIER_KEY)
  if (!verifier) {
    throw new Error('PKCE verifier ausente. Inicie o login novamente.')
  }
  return verifier
}

/** Troca authorization code por tokens em POST /connect/token (OIDC). */
export async function redeemAuthorizationCode(code: string, codeVerifier: string): Promise<OidcTokenResponse> {
  const body = new URLSearchParams({
    grant_type: 'authorization_code',
    code,
    redirect_uri: env.oauthRedirectUri,
    client_id: env.oauthClientId,
    code_verifier: codeVerifier,
  })

  const response = await fetch(`${getOidcOrigin()}${apiPaths.connectToken}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body,
  })

  if (!response.ok) {
    const text = await response.text()
    throw new Error(text || `Falha ao trocar authorization code (${response.status}).`)
  }

  return (await response.json()) as OidcTokenResponse
}

export async function refreshOidcTokens(refreshToken: string): Promise<OidcTokenResponse> {
  const body = new URLSearchParams({
    grant_type: 'refresh_token',
    refresh_token: refreshToken,
    client_id: env.oauthClientId,
  })

  const response = await fetch(`${getOidcOrigin()}${apiPaths.connectToken}`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
    body,
  })

  if (!response.ok) {
    const text = await response.text()
    throw new Error(text || `Falha ao renovar token (${response.status}).`)
  }

  return (await response.json()) as OidcTokenResponse
}

export function buildLogoutUrl(postLogoutRedirectUri?: string): string {
  const redirect = postLogoutRedirectUri ?? `${window.location.origin}/login`
  const params = new URLSearchParams({
    client_id: env.oauthClientId,
    post_logout_redirect_uri: redirect,
  })
  return `${getOidcOrigin()}${apiPaths.connectLogout}?${params.toString()}`
}
