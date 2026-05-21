import axios from 'axios'
import { env } from './env'
import { refreshOidcTokens } from '../services/oidcService'
import { clearAuthSession, getAuthSession, updateSessionFromOidcRefresh } from '../utils/authStorage'
import { parseApiBody } from '../utils/apiResponse'
import type { OidcTokenResponse } from '../types/oidc'
const baseURL = env.apiBaseUrl

const sharedConfig = {
  baseURL,
  timeout: env.apiTimeoutMs,
  headers: {
    Accept: 'application/json',
  },
  maxRedirects: 0,
}

export const publicApi = axios.create(sharedConfig)

export const api = axios.create(sharedConfig)

let refreshPromise: Promise<OidcTokenResponse> | null = null

api.interceptors.request.use((config) => {
  const session = getAuthSession()
  if (session?.accessToken) {
    config.headers.Authorization = `Bearer ${session.accessToken}`
  }
  return config
})

api.interceptors.response.use(
  (response) => {
    if (typeof response.data === 'string') {
      const trimmed = response.data.trim()
      if (trimmed.startsWith('<!DOCTYPE') || trimmed.startsWith('<html')) {
        return Promise.reject(
          new Error('A API redirecionou para login. Faça logout e entre novamente.'),
        )
      }
    }

    response.data = parseApiBody(response.data)
    return response
  },
  async (error) => {
    const originalRequest = error.config as (typeof error.config & { _retry?: boolean }) | undefined
    const statusCode = error.response?.status

    if (statusCode === 302 || statusCode === 301) {
      return Promise.reject(
        new Error('A API redirecionou para login. Faça logout e entre novamente.'),
      )
    }

    if (!originalRequest || statusCode !== 401 || originalRequest._retry) {
      return Promise.reject(error)
    }

    const session = getAuthSession()
    if (!session?.refreshToken) {
      clearAuthSession()
      return Promise.reject(error)
    }

    originalRequest._retry = true

    try {
      if (!refreshPromise) {
        refreshPromise = refreshOidcTokens(session.refreshToken)
      }

      const refreshed = await refreshPromise
      updateSessionFromOidcRefresh(refreshed)
      originalRequest.headers.Authorization = `Bearer ${refreshed.access_token}`
      return await api.request(originalRequest)
    } catch (refreshError) {
      clearAuthSession()
      return Promise.reject(refreshError)
    } finally {
      refreshPromise = null
    }
  },
)
