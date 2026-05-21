import { createContext, useCallback, useContext, useMemo, useState } from 'react'
import type { PropsWithChildren } from 'react'
import type { AuthTenantSummary } from '../types'
import type { OidcTokenResponse, TenantContextResult } from '../types/oidc'
import {
  applyTenantContext,
  clearAuthSession,
  getAuthSession,
  saveSessionFromOidcTokens,
  type AuthSessionStorage,
} from '../utils/authStorage'

interface AuthContextValue {
  isAuthenticated: boolean
  userId?: string
  email?: string
  tenantId?: string | null
  tenantRoles: string[]
  platformRoles: string[]
  tenants: AuthTenantSummary[]
  applyOidcLogin: (tokens: OidcTokenResponse, tenants?: AuthTenantSummary[]) => void
  applyTenantSwitch: (context: TenantContextResult) => void
  logoutLocal: () => void
}

const AuthContext = createContext<AuthContextValue | undefined>(undefined)

export function AuthProvider({ children }: PropsWithChildren) {
  const [session, setSession] = useState<AuthSessionStorage | null>(getAuthSession())

  const applyOidcLogin = useCallback((tokens: OidcTokenResponse, tenants: AuthTenantSummary[] = []) => {
    const saved = saveSessionFromOidcTokens(tokens, tenants)
    setSession(saved)
  }, [])

  const applyTenantSwitch = useCallback((context: TenantContextResult) => {
    const saved = applyTenantContext(context)
    setSession(saved)
  }, [])

  const logoutLocal = useCallback(() => {
    clearAuthSession()
    setSession(null)
  }, [])

  const value = useMemo<AuthContextValue>(
    () => ({
      isAuthenticated: Boolean(session?.accessToken),
      userId: session?.userId,
      email: session?.email,
      tenantId: session?.tenantId,
      tenantRoles: session?.tenantRoles ?? [],
      platformRoles: session?.platformRoles ?? [],
      tenants: session?.tenants ?? [],
      applyOidcLogin,
      applyTenantSwitch,
      logoutLocal,
    }),
    [applyOidcLogin, applyTenantSwitch, logoutLocal, session],
  )

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within AuthProvider')
  }
  return context
}
