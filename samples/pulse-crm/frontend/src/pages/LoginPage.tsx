import { useState, useEffect } from 'react'
import { Navigate } from 'react-router-dom'
import { idpClient } from '../config/idpClient'
import { isLoggedIn } from '../utils/authStorage'
import { getMe } from '../services/crmApi'

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
      await idpClient.oidc.signInRedirect()
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
          Você será redirecionado ao IdP Platform para entrar ou criar uma conta. Este sample não possui tela
          própria de cadastro. Novos usuários seguem para o onboarding após o primeiro login (claim{' '}
          <code>tid</code> ausente).
        </p>
      </div>
    </div>
  )
}
