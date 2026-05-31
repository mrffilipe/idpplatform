import { useEffect, useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router-dom'
import { kyvoClient } from '../config/kyvoClient'
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

    if (!kyvoClient.oidc.tryAcquireCallbackLock()) return

    void (async () => {
      try {
        const tokens = await kyvoClient.oidc.handleCallback(code, searchParams.get('state'))
        kyvoClient.oidc.clearOidcRequest()
        saveSession(tokens)

        const me = await getMe()
        navigate(me.hasSubscription ? '/dashboard' : '/onboarding', { replace: true })
      } catch (e) {
        kyvoClient.oidc.releaseCallbackLock()
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
