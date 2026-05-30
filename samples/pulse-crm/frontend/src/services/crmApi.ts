import axios, { type AxiosError } from 'axios'
import { env } from '../config/env'
import { idpClient } from '../config/idpClient'
import type { Contact, MeResponse, OnboardingCompleteResponse } from '../types/crm'
import { getSession, updateAccessToken } from '../utils/authStorage'
import { hasTenant } from '@idpplatform/client'

export const crmApi = axios.create({
  baseURL: env.crmApiUrl,
  headers: { Accept: 'application/json' },
})

crmApi.interceptors.request.use((config) => {
  const token = idpClient.getAccessToken()
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

let refreshPromise: ReturnType<typeof idpClient.oidc.refresh> | null = null
let tenantRefreshPromise: ReturnType<typeof idpClient.refreshAccessTokenWithTenant> | null = null

function isMissingTenantError(error: AxiosError): boolean {
  const message = (error.response?.data as { message?: string } | undefined)?.message
  return (
    error.response?.status === 400 &&
    typeof message === 'string' &&
    message.toLowerCase().includes('tenant')
  )
}

crmApi.interceptors.response.use(
  (r) => r,
  async (error: AxiosError) => {
    const original = error.config as typeof error.config & {
      _retry?: boolean
      _tenantRetry?: boolean
    }
    if (!original) {
      return Promise.reject(error)
    }

    if (isMissingTenantError(error) && !original._tenantRetry) {
      original._tenantRetry = true
      try {
        if (!tenantRefreshPromise) tenantRefreshPromise = idpClient.refreshAccessTokenWithTenant()
        const tokens = await tenantRefreshPromise
        original.headers.Authorization = `Bearer ${tokens.access_token}`
        return crmApi.request(original)
      } catch {
        return Promise.reject(error)
      } finally {
        tenantRefreshPromise = null
      }
    }

    if (error.response?.status !== 401 || original._retry) {
      return Promise.reject(error)
    }

    const session = getSession()
    if (!session?.refreshToken) return Promise.reject(error)

    original._retry = true
    try {
      if (!refreshPromise) refreshPromise = idpClient.oidc.refresh(session.refreshToken)
      const tokens = await refreshPromise
      updateAccessToken(tokens)
      original.headers.Authorization = `Bearer ${tokens.access_token}`
      return crmApi.request(original)
    } finally {
      refreshPromise = null
    }
  },
)

export async function ensureTenantAccessToken(): Promise<void> {
  const session = getSession()
  if (!session?.accessToken) {
    throw new Error('Sessão ausente. Faça login novamente.')
  }
  if (hasTenant(session.accessToken)) {
    return
  }
  try {
    await idpClient.refreshAccessTokenWithTenant()
  } catch {
    /* CRM resolves tenantId from subscription when JWT lacks tid */
  }
}

export async function getMe(): Promise<MeResponse> {
  const { data } = await crmApi.get<MeResponse>('/api/me')
  return data
}

export async function completeOnboarding(body: {
  companyName: string
  planCode: string
  paymentReference?: string
}): Promise<OnboardingCompleteResponse> {
  const { data } = await crmApi.post<OnboardingCompleteResponse>('/api/onboarding/complete', body)
  return data
}

export async function listContacts(): Promise<Contact[]> {
  const { data } = await crmApi.get<Contact[]>('/api/contacts')
  return data
}

export async function createContact(body: {
  name: string
  email: string
  phone?: string
}): Promise<Contact> {
  const { data } = await crmApi.post<Contact>('/api/contacts', body)
  return data
}

export async function updateContact(
  id: string,
  body: { name: string; email: string; phone?: string },
): Promise<Contact> {
  const { data } = await crmApi.put<Contact>(`/api/contacts/${id}`, body)
  return data
}

export async function deleteContact(id: string): Promise<void> {
  await crmApi.delete(`/api/contacts/${id}`)
}
