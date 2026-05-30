import { useEffect, useState } from 'react'
import { idpClient } from '../config/idpClient'
import { getMe } from '../services/crmApi'
import type { MeResponse } from '../types/crm'

export function DashboardPage() {
  const [me, setMe] = useState<MeResponse | null>(null)
  const [userInfo, setUserInfo] = useState<Record<string, unknown> | null>(null)
  const [error, setError] = useState<string | null>(null)

  useEffect(() => {
    void (async () => {
      try {
        const profile = await getMe()
        setMe(profile)
        const token = idpClient.getAccessToken()
        if (token) {
          setUserInfo(await idpClient.oidc.fetchUserInfo(token))
        }
      } catch (e) {
        setError(e instanceof Error ? e.message : 'Failed to load profile')
      }
    })()
  }, [])

  if (error) {
    return <p className="error">{error}</p>
  }

  if (!me) {
    return <p className="muted">Carregando…</p>
  }

  return (
    <div className="page">
      <h1>Dashboard</h1>
      <div className="card">
        <p>
          <strong>E-mail:</strong> {me.email ?? '—'}
        </p>
        <p>
          <strong>Tenant (efetivo):</strong> {me.tenantId ?? '—'}
        </p>
        <p>
          <strong>Assinatura CRM:</strong> {me.hasSubscription ? me.subscription?.companyName : 'Pendente'}
        </p>
        {userInfo && (
          <details>
            <summary>UserInfo OIDC</summary>
            <pre>{JSON.stringify(userInfo, null, 2)}</pre>
          </details>
        )}
      </div>
    </div>
  )
}
