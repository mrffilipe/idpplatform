import type { OidcTokenResponse, ProblemDetails } from '../types.js'
import type { SessionManager } from '../session/sessionManager.js'

export class KyvoApiError extends Error {
  constructor(
    message: string,
    readonly status: number,
    readonly problem?: ProblemDetails,
    readonly rawBody?: string,
  ) {
    super(message)
    this.name = 'KyvoApiError'
  }
}

export interface HttpClientOptions {
  baseUrl: string
  getAccessToken: () => string | null
  refreshTokens: (refreshToken: string) => Promise<OidcTokenResponse>
  onSessionUpdated: (tokens: OidcTokenResponse) => void
  onSessionCleared: () => void
  getRefreshToken: () => string | undefined
}

export class HttpClient {
  private refreshPromise: Promise<OidcTokenResponse> | null = null

  constructor(private readonly options: HttpClientOptions) { }

  async request<T>(
    method: string,
    path: string,
    init?: { body?: unknown; params?: Record<string, string | number | boolean | undefined> },
  ): Promise<T> {
    const url = this.buildUrl(path, init?.params)
    const headers: Record<string, string> = {
      Accept: 'application/json',
    }

    const token = this.options.getAccessToken()
    if (token) headers.Authorization = `Bearer ${token}`

    let body: string | undefined
    if (init?.body !== undefined) {
      headers['Content-Type'] = 'application/json'
      body = JSON.stringify(init.body)
    }

    const response = await this.fetchWithRefresh(method, url, headers, body)
    if (response.status === 204) {
      return undefined as T
    }

    const text = await response.text()
    if (!response.ok) {
      throw await this.toApiError(response.status, text)
    }

    if (!text) return undefined as T
    return JSON.parse(text) as T
  }

  private async fetchWithRefresh(
    method: string,
    url: string,
    headers: Record<string, string>,
    body?: string,
    retried = false,
  ): Promise<Response> {
    const response = await fetch(url, { method, headers, body })

    if (response.status !== 401 || retried) {
      return response
    }

    const refreshToken = this.options.getRefreshToken()
    if (!refreshToken) {
      this.options.onSessionCleared()
      return response
    }

    try {
      if (!this.refreshPromise) {
        this.refreshPromise = this.options.refreshTokens(refreshToken)
      }
      const tokens = await this.refreshPromise
      this.options.onSessionUpdated(tokens)
      headers.Authorization = `Bearer ${tokens.access_token}`
      return this.fetchWithRefresh(method, url, headers, body, true)
    } catch {
      this.options.onSessionCleared()
      return response
    } finally {
      this.refreshPromise = null
    }
  }

  private buildUrl(path: string, params?: Record<string, string | number | boolean | undefined>): string {
    const base = this.options.baseUrl.replace(/\/$/, '')
    const url = new URL(path.startsWith('/') ? `${base}${path}` : `${base}/${path}`)
    if (params) {
      for (const [key, value] of Object.entries(params)) {
        if (value !== undefined) url.searchParams.set(key, String(value))
      }
    }
    return url.toString()
  }

  private async toApiError(status: number, text: string): Promise<KyvoApiError> {
    try {
      const problem = JSON.parse(text) as ProblemDetails
      return new KyvoApiError(problem.detail ?? problem.title ?? text, status, problem, text)
    } catch {
      return new KyvoApiError(text || `HTTP ${status}`, status, undefined, text)
    }
  }
}

export function createHttpClientFromSession(
  baseUrl: string,
  session: SessionManager,
  refreshTokens: (refreshToken: string) => Promise<OidcTokenResponse>,
): HttpClient {
  return new HttpClient({
    baseUrl,
    getAccessToken: () => session.getAccessToken(),
    getRefreshToken: () => session.getSession()?.refreshToken,
    refreshTokens,
    onSessionUpdated: (tokens) => session.updateAccessToken(tokens),
    onSessionCleared: () => session.clear(),
  })
}
