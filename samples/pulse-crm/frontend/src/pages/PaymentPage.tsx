import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { completeOnboarding } from '../services/crmApi'
import { refreshTokens } from '../services/idpOidc'
import { clearOnboardingDraft, getOnboardingDraft, getSession, updateAccessToken } from '../utils/authStorage'
import { PLANS } from '../types/crm'

export function PaymentPage() {
  const navigate = useNavigate()
  const draft = getOnboardingDraft()
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  if (!draft) {
    navigate('/onboarding', { replace: true })
    return null
  }

  const plan = PLANS.find((p) => p.code === draft.planCode)

  async function handlePay() {
    if (!draft) return
    setLoading(true)
    setError(null)
    try {
      const result = await completeOnboarding({
        companyName: draft.companyName,
        planCode: draft.planCode,
        paymentReference: `pay_mock_${Date.now()}`,
      })

      if (result.requiresTokenRefresh) {
        const session = getSession()
        if (session?.refreshToken) {
          const tokens = await refreshTokens(session.refreshToken)
          updateAccessToken(tokens)
        }
      }

      clearOnboardingDraft()
      navigate('/dashboard', { replace: true })
    } catch (e) {
      const msg =
        axiosMessage(e) ??
        (e instanceof Error ? e.message : 'Payment/onboarding failed')
      setError(msg)
    } finally {
      setLoading(false)
    }
  }

  return (
    <div className="page">
      <h1>Pagamento (mock)</h1>
      <div className="card">
        <p>
          <strong>Empresa:</strong> {draft.companyName}
        </p>
        <p>
          <strong>Plano:</strong> {plan?.name ?? draft.planCode} — {plan?.price}
        </p>
        <p className="muted">
          Ao confirmar, a API PulseCRM chama <code>POST /v1.0/auth/subscribe</code> na IdP Platform e grava o
          vínculo application ↔ tenant com <code>planCode</code>.
        </p>
      </div>
      {error && <p className="error">{error}</p>}
      <button type="button" className="btn-primary" disabled={loading} onClick={() => void handlePay()}>
        {loading ? 'Processando…' : 'Pagar e ativar (mock aprovado)'}
      </button>
    </div>
  )
}

function axiosMessage(e: unknown): string | null {
  if (typeof e === 'object' && e !== null && 'response' in e) {
    const data = (e as { response?: { data?: { message?: string } } }).response?.data
    if (data?.message) return data.message
  }
  return null
}
