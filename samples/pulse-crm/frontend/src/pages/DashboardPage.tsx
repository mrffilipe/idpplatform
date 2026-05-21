import { useEffect, useState } from 'react'
import { getMe } from '../services/crmApi'
import { fetchUserInfo } from '../services/idpOidc'
import type { MeResponse } from '../types/crm'
import { getSession } from '../utils/authStorage'
import { parseJwtPayload } from '../utils/jwt'
import { PLANS } from '../types/crm'

export function DashboardPage() {
  const [me, setMe] = useState<MeResponse | null>(null)
  const [tokenClaims, setTokenClaims] = useState<Record<string, unknown> | null>(null)
  const [userinfo, setUserinfo] = useState<Record<string, unknown> | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [loadingUserinfo, setLoadingUserinfo] = useState(false)

  useEffect(() => {
    void getMe()
      .then(setMe)
      .catch((e) => setError(e instanceof Error ? e.message : 'Failed to load profile'))

    const session = getSession()
    if (session?.accessToken) {
      try {
        setTokenClaims(parseJwtPayload(session.accessToken))
      } catch {
        setTokenClaims(null)
      }
    }
  }, [])

  async function loadUserinfo() {
    const session = getSession()
    if (!session?.accessToken) return
    setLoadingUserinfo(true)
    try {
      setUserinfo(await fetchUserInfo(session.accessToken))
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Userinfo failed')
    } finally {
      setLoadingUserinfo(false)
    }
  }

  const planLabel = me?.subscription
    ? PLANS.find((p) => p.code === me.subscription?.planCode)?.name ?? me.subscription.planCode
    : '—'

  return (
    <div className="page">
      <h1>Dashboard</h1>
      {error && <p className="error">{error}</p>}

      {me?.subscription && (
        <section className="card">
          <h2>Assinatura PulseCRM</h2>
          <dl className="kv">
            <dt>Empresa</dt>
            <dd>{me.subscription.companyName}</dd>
            <dt>Tenant key</dt>
            <dd>
              <code>{me.subscription.tenantKey}</code>
            </dd>
            <dt>Plano</dt>
            <dd>
              {planLabel} (<code>{me.subscription.planCode}</code>)
            </dd>
            <dt>Tenant ID (IdP)</dt>
            <dd>
              <code>{me.subscription.tenantId}</code>
            </dd>
            <dt>External customer</dt>
            <dd>
              <code>{me.subscription.externalCustomerId}</code>
            </dd>
            <dt>Pago em</dt>
            <dd>{new Date(me.subscription.paidAt).toLocaleString()}</dd>
          </dl>
        </section>
      )}

      <section className="card">
        <h2>Perfil (API CRM /api/me)</h2>
        <pre>{JSON.stringify(me, null, 2)}</pre>
      </section>

      <section className="card">
        <h2>Access token (JWT decodificado)</h2>
        {tokenClaims && !tokenClaims.tid && me?.subscription?.tenantId && (
          <p className="error">
            O JWT ainda não tem <code>tid</code>, mas a assinatura local tem tenant{' '}
            <code>{me.subscription.tenantId}</code>. Contatos usam o tenant da assinatura; para o claim no token,
            refaça o pagamento mock ou saia e entre de novo.
          </p>
        )}
        <pre>{JSON.stringify(tokenClaims, null, 2)}</pre>
      </section>

      <section className="card">
        <h2>OIDC UserInfo</h2>
        <button type="button" className="btn-secondary" disabled={loadingUserinfo} onClick={() => void loadUserinfo()}>
          {loadingUserinfo ? 'Carregando…' : 'GET /connect/userinfo'}
        </button>
        {userinfo && <pre>{JSON.stringify(userinfo, null, 2)}</pre>}
      </section>
    </div>
  )
}
