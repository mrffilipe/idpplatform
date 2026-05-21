import { Button, Stack, Typography } from '@mui/material'
import { useState } from 'react'
import { useLoaderData, useRevalidator } from 'react-router'
import { AuthLayout } from '../components/AuthLayout'
import { FeedbackAlerts } from '../components/ui'
import { env } from '../config'
import { bootstrapPlatform } from '../services/platformService'
import { redirectToOidcLogin } from '../services/oidcService'
import type { LoginLoaderData } from '../routes/loaders'
import { getApiErrorMessage } from '../utils/apiError'

export function LoginPage() {
  const { requiresBootstrap } = useLoaderData() as LoginLoaderData
  const revalidator = useRevalidator()
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

  async function handleBootstrap(): Promise<void> {
    setLoading(true)
    setError(null)
    try {
      await bootstrapPlatform()
      await revalidator.revalidate()
    } catch (bootstrapError) {
      setError(getApiErrorMessage(bootstrapError))
    } finally {
      setLoading(false)
    }
  }

  async function handleLogin(): Promise<void> {
    setLoading(true)
    setError(null)
    try {
      await redirectToOidcLogin()
    } catch (loginError) {
      setError(loginError instanceof Error ? loginError.message : 'Falha ao iniciar login.')
      setLoading(false)
    }
  }

  if (requiresBootstrap) {
    return (
      <AuthLayout
        title="Primeira configuração"
        subtitle="A plataforma ainda não foi inicializada"
      >
        <Stack spacing={2.5}>
          <Typography variant="body2" color="text.secondary">
            As credenciais do administrador raiz são definidas no backend (variáveis{' '}
            <code>Bootstrap__AdminEmail</code> / <code>Bootstrap__AdminPassword</code> ou seção{' '}
            <code>Bootstrap</code> no appsettings). Clique abaixo para criar o usuário admin, o
            client OAuth <strong>{env.oauthClientId}</strong> e o provedor local.
          </Typography>
          <FeedbackAlerts error={error} />
          <Button
            variant="contained"
            size="large"
            disabled={loading || revalidator.state === 'loading'}
            onClick={() => void handleBootstrap()}
            sx={{ py: 1.25 }}
          >
            {loading ? 'Inicializando...' : 'Inicializar plataforma'}
          </Button>
        </Stack>
      </AuthLayout>
    )
  }

  return (
    <AuthLayout title="Bem-vindo de volta" subtitle="Entre com suas credenciais via OpenID Connect">
      <Stack spacing={2.5}>
        <Typography variant="body2" color="text.secondary">
          Client <strong>{env.oauthClientId}</strong> — fluxo authorization code + PKCE contra{' '}
          <code>/connect/authorize</code>.
        </Typography>
        <FeedbackAlerts error={error} />
        <Button
          variant="contained"
          size="large"
          disabled={loading}
          onClick={() => void handleLogin()}
          sx={{ py: 1.25 }}
        >
          {loading ? 'Redirecionando...' : 'Entrar na plataforma'}
        </Button>
      </Stack>
    </AuthLayout>
  )
}
