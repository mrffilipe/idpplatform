import { Button, Stack, TextField, Typography } from '@mui/material'
import { useState } from 'react'
import { useNavigate, useSearchParams } from 'react-router'
import { FeedbackAlerts, FormActions, PageHeader, SectionCard } from '../components/ui'
import { acceptInvite } from '../services'
import { getApiErrorMessage } from '../utils/apiError'

export function AcceptInvitePage() {
  const [searchParams] = useSearchParams()
  const navigate = useNavigate()
  const [token, setToken] = useState(searchParams.get('token') ?? '')
  const [success, setSuccess] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault()
    setSuccess(null)
    setError(null)
    setLoading(true)
    try {
      const data = await acceptInvite({ token })
      setSuccess(`Convite aceito com sucesso. Membership: ${data.membershipId}`)
      setTimeout(() => {
        void navigate('/tenants')
      }, 1500)
    } catch (submitError) {
      setError(getApiErrorMessage(submitError))
    } finally {
      setLoading(false)
    }
  }

  return (
    <Stack spacing={3}>
      <PageHeader
        title="Aceitar convite"
        description="Use o token recebido por e-mail para ingressar no tenant."
      />

      <SectionCard title="Token do convite">
        <Stack spacing={2.5} component="form" onSubmit={handleSubmit}>
          <Typography variant="body2" color="text.secondary">
            Informe o token enviado por e-mail. Se você chegou aqui pelo link do convite, o campo já
            está preenchido.
          </Typography>
          <FeedbackAlerts success={success} error={error} />
          <TextField
            label="Token do convite"
            value={token}
            onChange={(event) => setToken(event.target.value)}
            required
            fullWidth
            autoFocus={!token}
          />
          <FormActions>
            <Button type="submit" size="large" disabled={loading || !token.trim()} sx={{ py: 1.25, px: 3 }}>
              {loading ? 'Processando...' : 'Aceitar convite'}
            </Button>
          </FormActions>
        </Stack>
      </SectionCard>
    </Stack>
  )
}
