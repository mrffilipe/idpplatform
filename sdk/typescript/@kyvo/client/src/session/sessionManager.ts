import type { AuthSession, OidcTokenResponse, SessionStorageLike } from '../types.js'

const SESSION_KEY = 'kyvo.client.session'

export class SessionManager {
  constructor(private readonly storage: SessionStorageLike) { }

  getSession(): AuthSession | null {
    const raw = this.storage.getItem(SESSION_KEY)
    if (!raw) return null
    try {
      const parsed = JSON.parse(raw) as AuthSession
      if (!parsed.accessToken) return null
      return parsed
    } catch {
      return null
    }
  }

  saveFromTokens(tokens: OidcTokenResponse): AuthSession {
    const session: AuthSession = {
      accessToken: tokens.access_token,
      refreshToken: tokens.refresh_token,
      expiresAt: Date.now() + tokens.expires_in * 1000,
    }
    this.storage.setItem(SESSION_KEY, JSON.stringify(session))
    return session
  }

  updateAccessToken(tokens: OidcTokenResponse): AuthSession {
    const existing = this.getSession()
    const session: AuthSession = {
      accessToken: tokens.access_token,
      refreshToken: tokens.refresh_token ?? existing?.refreshToken,
      expiresAt: Date.now() + tokens.expires_in * 1000,
    }
    this.storage.setItem(SESSION_KEY, JSON.stringify(session))
    return session
  }

  getAccessToken(): string | null {
    return this.getSession()?.accessToken ?? null
  }

  clear(): void {
    this.storage.removeItem(SESSION_KEY)
  }
}
