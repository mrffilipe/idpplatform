import type { OidcTokenResponse } from '../types/oidc'

const SESSION_KEY = 'pulsecrm.auth.session'

export interface AuthSession {
  accessToken: string
  refreshToken: string
  expiresAtIso: string
}

export function getSession(): AuthSession | null {
  const raw = localStorage.getItem(SESSION_KEY)
  if (!raw) return null
  try {
    return JSON.parse(raw) as AuthSession
  } catch {
    localStorage.removeItem(SESSION_KEY)
    return null
  }
}

export function saveSession(tokens: OidcTokenResponse): AuthSession {
  const session: AuthSession = {
    accessToken: tokens.access_token,
    refreshToken: tokens.refresh_token,
    expiresAtIso: new Date(Date.now() + tokens.expires_in * 1000).toISOString(),
  }
  localStorage.setItem(SESSION_KEY, JSON.stringify(session))
  return session
}

export function updateAccessToken(tokens: OidcTokenResponse): AuthSession {
  const current = getSession()
  const session: AuthSession = {
    accessToken: tokens.access_token,
    refreshToken: tokens.refresh_token || current?.refreshToken || '',
    expiresAtIso: new Date(Date.now() + tokens.expires_in * 1000).toISOString(),
  }
  localStorage.setItem(SESSION_KEY, JSON.stringify(session))
  return session
}

export function clearSession(): void {
  localStorage.removeItem(SESSION_KEY)
}

export function isLoggedIn(): boolean {
  return Boolean(getSession()?.accessToken)
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
