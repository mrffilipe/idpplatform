import type { OidcTokenResponse, SessionStorageLike } from '../types.js'
import { generatePkcePair } from './pkce.js'

const PKCE_KEY = 'idpplatform.oidc.verifier'
const STATE_KEY = 'idpplatform.oidc.state'
const CALLBACK_LOCK = 'idpplatform.oidc.lock'

export interface OidcClientOptions {
  authority: string
  clientId: string
  redirectUri: string
  scopes: string
  pkceStorage: SessionStorageLike
}

export class OidcClient {
  constructor(private readonly options: OidcClientOptions) { }

  private get origin(): string {
    return this.options.authority.replace(/\/$/, '')
  }

  tryAcquireCallbackLock(): boolean {
    if (this.options.pkceStorage.getItem(CALLBACK_LOCK)) return false
    this.options.pkceStorage.setItem(CALLBACK_LOCK, '1')
    return true
  }

  releaseCallbackLock(): void {
    this.options.pkceStorage.removeItem(CALLBACK_LOCK)
  }

  clearOidcRequest(): void {
    this.options.pkceStorage.removeItem(PKCE_KEY)
    this.options.pkceStorage.removeItem(STATE_KEY)
    this.releaseCallbackLock()
  }

  async signInRedirect(): Promise<void> {
    const { codeVerifier, codeChallenge } = await generatePkcePair()
    this.options.pkceStorage.setItem(PKCE_KEY, codeVerifier)

    const state = crypto.randomUUID()
    this.options.pkceStorage.setItem(STATE_KEY, state)

    const params = new URLSearchParams({
      client_id: this.options.clientId,
      redirect_uri: this.options.redirectUri,
      response_type: 'code',
      scope: this.options.scopes,
      code_challenge: codeChallenge,
      code_challenge_method: 'S256',
      state,
    })

    window.location.assign(`${this.origin}/connect/authorize?${params}`)
  }

  consumeState(returned: string | null): void {
    const expected = this.options.pkceStorage.getItem(STATE_KEY)
    this.options.pkceStorage.removeItem(STATE_KEY)
    if (!expected || !returned || expected !== returned) {
      throw new Error('OIDC state invalid or expired. Start login again.')
    }
  }

  consumeVerifier(): string {
    const verifier = this.options.pkceStorage.getItem(PKCE_KEY)
    this.options.pkceStorage.removeItem(PKCE_KEY)
    if (!verifier) throw new Error('PKCE verifier missing. Start login again.')
    return verifier
  }

  async handleCallback(code: string, state: string | null): Promise<OidcTokenResponse> {
    this.consumeState(state)
    const verifier = this.consumeVerifier()
    return this.exchangeCode(code, verifier)
  }

  async exchangeCode(code: string, verifier: string): Promise<OidcTokenResponse> {
    return this.postToken({
      grant_type: 'authorization_code',
      code,
      redirect_uri: this.options.redirectUri,
      client_id: this.options.clientId,
      code_verifier: verifier,
    })
  }

  async refresh(refreshToken: string): Promise<OidcTokenResponse> {
    return this.postToken({
      grant_type: 'refresh_token',
      refresh_token: refreshToken,
      client_id: this.options.clientId,
    })
  }

  buildLogoutUrl(postLogoutRedirectUri?: string): string {
    const redirect = postLogoutRedirectUri ?? `${window.location.origin}/login`
    const params = new URLSearchParams({
      client_id: this.options.clientId,
      post_logout_redirect_uri: redirect,
    })
    return `${this.origin}/connect/logout?${params}`
  }

  signOut(postLogoutRedirectUri?: string): void {
    window.location.assign(this.buildLogoutUrl(postLogoutRedirectUri))
  }

  async fetchUserInfo(accessToken: string): Promise<Record<string, unknown>> {
    const res = await fetch(`${this.origin}/connect/userinfo`, {
      headers: { Authorization: `Bearer ${accessToken}` },
    })
    if (!res.ok) throw new Error(await res.text())
    return (await res.json()) as Record<string, unknown>
  }

  private async postToken(body: Record<string, string>): Promise<OidcTokenResponse> {
    const res = await fetch(`${this.origin}/connect/token`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
      body: new URLSearchParams(body),
    })

    if (!res.ok) {
      const text = await res.text()
      throw new Error(text || `Token request failed (${res.status})`)
    }

    const json = (await res.json()) as Record<string, unknown>
    return normalizeOidcTokenResponse(json)
  }
}

export function normalizeOidcTokenResponse(json: Record<string, unknown>): OidcTokenResponse {
  const access = json.access_token ?? json.accessToken
  if (typeof access !== 'string') {
    throw new Error('Token response missing access_token')
  }

  return {
    access_token: access,
    token_type: typeof json.token_type === 'string' ? json.token_type : 'Bearer',
    expires_in: typeof json.expires_in === 'number' ? json.expires_in : 900,
    refresh_token: typeof json.refresh_token === 'string' ? json.refresh_token : undefined,
    id_token: typeof json.id_token === 'string' ? json.id_token : undefined,
    scope: typeof json.scope === 'string' ? json.scope : undefined,
  }
}
