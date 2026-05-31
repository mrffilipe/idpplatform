import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { PLANS } from '../types/crm'
import { setOnboardingDraft } from '../utils/authStorage'

export function OnboardingPage() {
  const navigate = useNavigate()
  const [companyName, setCompanyName] = useState('')
  const [planCode, setPlanCode] = useState<string>('professional')
  const [error, setError] = useState<string | null>(null)

  function handleContinue() {
    if (!companyName.trim()) {
      setError('Informe o nome da empresa.')
      return
    }
    setOnboardingDraft(planCode, companyName.trim())
    navigate('/payment')
  }

  return (
    <div className="page">
      <h1>Escolha seu plano</h1>
      <p className="muted">Após o pagamento (mock), vinculamos sua organização à aplicação Pulse CRM na Kyvo.</p>

      <label className="field">
        <span>Nome da empresa</span>
        <input
          value={companyName}
          onChange={(e) => setCompanyName(e.target.value)}
          placeholder="Acme Corp"
        />
      </label>

      <div className="plan-grid">
        {PLANS.map((plan) => (
          <button
            key={plan.code}
            type="button"
            className={`plan-card ${planCode === plan.code ? 'selected' : ''}`}
            onClick={() => setPlanCode(plan.code)}
          >
            <strong>{plan.name}</strong>
            <span>{plan.price}</span>
            <small>{plan.description}</small>
          </button>
        ))}
      </div>

      {error && <p className="error">{error}</p>}
      <button type="button" className="btn-primary" onClick={handleContinue}>
        Continuar para pagamento
      </button>
    </div>
  )
}
