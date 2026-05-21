import { Button, Stack, Typography } from '@mui/material'
import { useState } from 'react'
import { AuthLayout } from '../components/AuthLayout'
import { FeedbackAlerts } from '../components/ui'
import { env } from '../config'
import { redirectToOidcLogin } from '../services/oidcService'

export function LoginPage() {
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)

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
