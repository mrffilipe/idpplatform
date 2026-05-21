import { useEffect, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import {
  clearOidcRequest,
  consumeState,
  consumeVerifier,
  exchangeCode,
  releaseCallbackLock,
  tryAcquireCallbackLock,
} from '../services/idpOidc'
import { saveSession } from '../utils/authStorage'
import { getMe } from '../services/crmApi'

export function AuthCallbackPage() {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    const oauthError = searchParams.get('error')
    if (oauthError) {
      setError(searchParams.get('error_description') ?? oauthError)
      return
    }

    const code = searchParams.get('code')
    if (!code) {
      navigate('/login', { replace: true })
      return
    }

    if (!tryAcquireCallbackLock()) return

    void (async () => {
      try {
        consumeState(searchParams.get('state'))
        const tokens = await exchangeCode(code, consumeVerifier())
        clearOidcRequest()
        saveSession(tokens)

        const me = await getMe()
        navigate(me.hasSubscription ? '/dashboard' : '/onboarding', { replace: true })
      } catch (e) {
        releaseCallbackLock()
        setError(e instanceof Error ? e.message : 'Callback failed')
      }
    })()
  }, [navigate, searchParams])

  return (
    <div className="center-page">
      <div className="card">
        <h2>Conectando…</h2>
        {error ? <p className="error">{error}</p> : <p className="muted">Finalizando login OIDC…</p>}
      </div>
    </div>
  )
}
