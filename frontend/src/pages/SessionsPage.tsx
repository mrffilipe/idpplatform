import DeleteIcon from '@mui/icons-material/Delete'
import RefreshIcon from '@mui/icons-material/Refresh'
import { IconButton, Stack, TableCell, TableRow, Tooltip, Typography } from '@mui/material'
import { useEffect, useState } from 'react'
import { DataTable, FeedbackAlerts, PageHeader, SectionCard } from '../components/ui'
import { listActiveSessions, revokeSession } from '../services'
import type { AuthSession } from '../types'
import { getApiErrorMessage } from '../utils/apiError'
import { sessionStatusLabel } from '../utils/enumLabels'

export function SessionsPage() {
  const [sessions, setSessions] = useState<AuthSession[]>([])
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)

  useEffect(() => {
    void loadSessions()
  }, [])

  async function loadSessions(): Promise<void> {
    setError(null)
    try {
      const data = await listActiveSessions()
      setSessions(data)
    } catch (loadError) {
      setError(getApiErrorMessage(loadError))
    }
  }

  async function handleRevoke(sessionId: string): Promise<void> {
    setError(null)
    setSuccess(null)
    try {
      await revokeSession(sessionId)
      setSuccess('Sessão revogada com sucesso.')
      await loadSessions()
    } catch (revokeError) {
      setError(getApiErrorMessage(revokeError))
    }
  }

  return (
    <Stack spacing={3}>
      <PageHeader
        title="Sessões ativas"
        description="Visualize e revogue sessões de autenticação da sua conta."
        actions={
          <Tooltip title="Recarregar sessões">
            <IconButton onClick={() => void loadSessions()} color="primary">
              <RefreshIcon />
            </IconButton>
          </Tooltip>
        }
      />
      <FeedbackAlerts success={success} error={error} />

      <SectionCard title="Sessões">
        <DataTable
          columns={[
            { id: 'sessionId', label: 'Session Id', minWidth: 140 },
            { id: 'status', label: 'Status' },
            { id: 'tenant', label: 'Tenant' },
            { id: 'ip', label: 'IP' },
            { id: 'userAgent', label: 'User Agent', minWidth: 160 },
            { id: 'expires', label: 'Expira em' },
            { id: 'actions', label: 'Ações', align: 'right' },
          ]}
          rows={sessions.map((session) => (
            <TableRow key={session.sessionId} hover>
              <TableCell sx={{ fontFamily: 'monospace', fontSize: '0.75rem' }}>{session.sessionId}</TableCell>
              <TableCell>{sessionStatusLabel(session.status)}</TableCell>
              <TableCell>{session.tenantId ?? '-'}</TableCell>
              <TableCell>{session.ipAddress ?? '-'}</TableCell>
              <TableCell>
                <Tooltip title={session.userAgent ?? '-'}>
                  <Typography variant="body2" noWrap sx={{ maxWidth: 200 }}>
                    {session.userAgent ?? '-'}
                  </Typography>
                </Tooltip>
              </TableCell>
              <TableCell>{new Date(session.expiresAt).toLocaleString('pt-BR')}</TableCell>
              <TableCell align="right">
                <Tooltip title="Revogar sessão">
                  <IconButton color="error" size="small" onClick={() => void handleRevoke(session.sessionId)}>
                    <DeleteIcon fontSize="small" />
                  </IconButton>
                </Tooltip>
              </TableCell>
            </TableRow>
          ))}
          emptyDescription="Nenhuma sessão ativa encontrada."
        />
      </SectionCard>
    </Stack>
  )
}
