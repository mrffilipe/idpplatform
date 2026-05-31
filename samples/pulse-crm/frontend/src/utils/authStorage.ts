import { normalizeOidcTokenResponse } from '@kyvo/client'
import type { OidcTokenResponse } from '../types/oidc'
import { kyvoClient } from '../config/kyvoClient'

export interface AuthSession {
  accessToken: string
  refreshToken: string
  expiresAtIso: string
}

export function getSession(): AuthSession | null {
  const s = kyvoClient.session.getSession()
  if (!s) return null
  return {
    accessToken: s.accessToken,
    refreshToken: s.refreshToken ?? '',
    expiresAtIso: new Date(s.expiresAt).toISOString(),
  }
}

export function saveSession(tokens: OidcTokenResponse): AuthSession {
  kyvoClient.session.saveFromTokens(tokens)
  return getSession()!
}

export function updateAccessToken(tokens: OidcTokenResponse): AuthSession {
  kyvoClient.session.updateAccessToken(tokens)
  return getSession()!
}

export function clearSession(): void {
  kyvoClient.session.clear()
}

export function isLoggedIn(): boolean {
  return Boolean(kyvoClient.getAccessToken())
}

const ONBOARDING_PLAN_KEY = 'pulsecrm.onboarding.plan'
const ONBOARDING_COMPANY_KEY = 'pulsecrm.onboarding.company'

export function setOnboardingDraft(planCode: string, companyName: string): void {
  sessionStorage.setItem(ONBOARDING_PLAN_KEY, planCode)
  sessionStorage.setItem(ONBOARDING_COMPANY_KEY, companyName)
}

export function getOnboardingDraft(): { planCode: string; companyName: string } | null {
  const planCode = sessionStorage.getItem(ONBOARDING_PLAN_KEY)
  const companyName = sessionStorage.getItem(ONBOARDING_COMPANY_KEY)
  if (!planCode || !companyName) return null
  return { planCode, companyName }
}

export function clearOnboardingDraft(): void {
  sessionStorage.removeItem(ONBOARDING_PLAN_KEY)
  sessionStorage.removeItem(ONBOARDING_COMPANY_KEY)
}

export { normalizeOidcTokenResponse }
