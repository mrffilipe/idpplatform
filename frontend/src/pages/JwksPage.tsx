import ContentCopyIcon from '@mui/icons-material/ContentCopy'
import { Alert, Box, Button, IconButton, Stack, Tooltip } from '@mui/material'
import { useState } from 'react'
import { FeedbackAlerts, PageHeader, SectionCard } from '../components/ui'
import { getJwks } from '../services'
import type { JwksResponse } from '../types'
import { getApiErrorMessage } from '../utils/apiError'

export function JwksPage() {
  const [data, setData] = useState<JwksResponse | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)
  const [copied, setCopied] = useState(false)

  async function handleLoad(): Promise<void> {
    setError(null)
    setLoading(true)
    try {
      const response = await getJwks()
      setData(response)
    } catch (loadError) {
      setError(getApiErrorMessage(loadError))
    } finally {
      setLoading(false)
    }
  }

  const jsonText = data ? JSON.stringify(data, null, 2) : ''

  async function handleCopy(): Promise<void> {
    if (!jsonText) {
      return
    }
    await navigator.clipboard.writeText(jsonText)
    setCopied(true)
    setTimeout(() => setCopied(false), 2000)
  }

  return (
    <Stack spacing={3}>
      <PageHeader
        title="JWKS"
        description="Chaves públicas usadas para validar tokens JWT emitidos pela plataforma."
        actions={
          <Button onClick={() => void handleLoad()} disabled={loading}>
            {loading ? 'Carregando...' : 'Carregar JWKS'}
          </Button>
        }
      />
      <FeedbackAlerts error={error} />
      <SectionCard title="Documento JWKS" subtitle="Resposta de /.well-known/jwks.json">
        {data ? (
          <Stack spacing={1}>
            <Stack direction="row" sx={{ justifyContent: 'flex-end' }}>
              <Tooltip title={copied ? 'Copiado!' : 'Copiar JSON'}>
                <IconButton onClick={() => void handleCopy()} size="small">
                  <ContentCopyIcon fontSize="small" />
                </IconButton>
              </Tooltip>
            </Stack>
            <Box
              component="pre"
              sx={{
                m: 0,
                p: 2,
                borderRadius: 2,
                bgcolor: 'action.hover',
                overflow: 'auto',
                fontFamily: '"Roboto Mono", monospace',
                fontSize: '0.8125rem',
                lineHeight: 1.6,
                maxHeight: 480,
              }}
            >
              {jsonText}
            </Box>
          </Stack>
        ) : (
          <Alert severity="info">Clique em &quot;Carregar JWKS&quot; para buscar as chaves públicas.</Alert>
        )}
      </SectionCard>
    </Stack>
  )
}
