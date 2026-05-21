import { CircularProgress, Stack, Typography } from '@mui/material'
import { useEffect, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router'
import { AuthLayout } from '../components/AuthLayout'
import { FeedbackAlerts } from '../components/ui'
import { useAuth } from '../contexts/AuthContext'
import { useTenant } from '../contexts/TenantContext'
import { getMe } from '../services/usersService'
import { enrichSessionFromUser, saveSessionFromOidcTokens } from '../utils/authStorage'
import {
  clearOidcLoginRequest,
  consumeOidcState,
  consumePkceVerifier,
  redeemAuthorizationCode,
  releaseOidcCallbackLock,
  tryAcquireOidcCallbackLock,
} from '../services/oidcService'
import { getApiErrorMessage } from '../utils/apiError'

export function AuthCallbackPage() {
  const navigate = useNavigate()
  const [searchParams] = useSearchParams()
  const { applyOidcLogin } = useAuth()
  const { selectTenant } = useTenant()
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const code = searchParams.get('code')
    const state = searchParams.get('state')
    const oauthError = searchParams.get('error')

    if (oauthError) {
      setError(searchParams.get('error_description') ?? oauthError)
      return
    }

    if (!code) {
      navigate('/login', { replace: true })
      return
    }

    if (!tryAcquireOidcCallbackLock()) {
      return
    }

    void (async () => {
      try {
        consumeOidcState(state)
        const verifier = consumePkceVerifier()
        const tokens = await redeemAuthorizationCode(code, verifier)
        clearOidcLoginRequest()

        const session = saveSessionFromOidcTokens(tokens)
        applyOidcLogin(tokens, session.tenants)

        try {
          const profile = await getMe()
          const enriched = enrichSessionFromUser(profile)
          applyOidcLogin(tokens, enriched.tenants)
          const preferredTenantId =
            enriched.tenantId ?? profile.memberships[0]?.tenantId ?? null
          if (preferredTenantId) {
            selectTenant(preferredTenantId)
          }
        } catch {
          /* claims do JWT já permitem navegar; perfil completa depois */
        }

        navigate('/', { replace: true })
      } catch (callbackError) {
        releaseOidcCallbackLock()
        setError(getApiErrorMessage(callbackError))
      }
    })()
  }, [applyOidcLogin, navigate, searchParams, selectTenant])

  return (
    <AuthLayout title="Conectando…" subtitle="Finalizando login OIDC">
      <Stack spacing={2} sx={{ alignItems: 'center' }}>
        {error ? (
          <FeedbackAlerts error={error} />
        ) : (
          <>
            <CircularProgress size={32} />
            <Typography variant="body2" color="text.secondary">
              Trocando authorization code por tokens…
            </Typography>
          </>
        )}
      </Stack>
    </AuthLayout>
  )
}
