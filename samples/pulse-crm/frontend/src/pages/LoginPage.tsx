import { useState } from 'react'
import { Navigate } from 'react-router-dom'
import { redirectToLogin } from '../services/idpOidc'
import { isLoggedIn } from '../utils/authStorage'
import { getMe } from '../services/crmApi'
import { useEffect } from 'react'

export function LoginPage() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [redirectTo, setRedirectTo] = useState<string | null>(null)

  useEffect(() => {
    if (!isLoggedIn()) return
    void getMe()
      .then((me) => setRedirectTo(me.hasSubscription ? '/dashboard' : '/onboarding'))
      .catch(() => setRedirectTo('/onboarding'))
  }, [])

  if (redirectTo) {
    return <Navigate to={redirectTo} replace />
  }

  async function handleLogin() {
    setLoading(true)
    setError(null)
    try {
      await redirectToLogin()
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Login failed')
      setLoading(false)
    }
  }

  return (
    <div className="center-page">
      <div className="card auth-card">
        <h1>PulseCRM</h1>
        <p className="muted">Sample SaaS integrado à IdP Platform (OIDC + PKCE)</p>
        {error && <p className="error">{error}</p>}
        <button type="button" className="btn-primary" disabled={loading} onClick={() => void handleLogin()}>
          {loading ? 'Redirecionando…' : 'Entrar com IdP Platform'}
        </button>
        <p className="hint">
          Use uma conta já existente no IdP (ex.: admin do bootstrap). Cadastro público não está neste sample.
        </p>
      </div>
    </div>
  )
}
