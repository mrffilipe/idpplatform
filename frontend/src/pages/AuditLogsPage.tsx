import { Button, Stack, TableCell, TableRow, TextField, Tooltip, Typography } from '@mui/material'
import { useState } from 'react'
import { DataTable, FeedbackAlerts, FormActions, FormGrid, FormGridItem, PageHeader, SectionCard } from '../components/ui'
import { listAuditLogs } from '../services'
import type { AuditLogItem } from '../types'
import { getApiErrorMessage } from '../utils/apiError'

export function AuditLogsPage() {
  const [items, setItems] = useState<AuditLogItem[]>([])
  const [error, setError] = useState<string | null>(null)
  const [action, setAction] = useState('')
  const [resourceType, setResourceType] = useState('')
  const [userId, setUserId] = useState('')

  async function handleSearch(event: React.FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault()
    setError(null)
    try {
      const data = await listAuditLogs({
        action: action || undefined,
        resourceType: resourceType || undefined,
        userId: userId || undefined,
        page: 1,
        pageSize: 100,
      })
      setItems(data.items)
    } catch (searchError) {
      setError(getApiErrorMessage(searchError))
    }
  }

  return (
    <Stack spacing={3}>
      <PageHeader title="Audit Logs" description="Consulte o histórico de ações registradas na plataforma." />
      <FeedbackAlerts error={error} />

      <SectionCard title="Filtros de busca">
        <Stack component="form" onSubmit={handleSearch} spacing={2}>
          <FormGrid>
            <FormGridItem>
              <TextField label="Ação" value={action} onChange={(e) => setAction(e.target.value)} fullWidth />
            </FormGridItem>
            <FormGridItem>
              <TextField label="Tipo de recurso" value={resourceType} onChange={(e) => setResourceType(e.target.value)} fullWidth />
            </FormGridItem>
            <FormGridItem>
              <TextField label="User Id" value={userId} onChange={(e) => setUserId(e.target.value)} fullWidth />
            </FormGridItem>
          </FormGrid>
          <FormActions>
            <Button type="submit">Buscar logs</Button>
          </FormActions>
        </Stack>
      </SectionCard>

      <SectionCard title="Resultados">
        <DataTable
          columns={[
            { id: 'id', label: 'Id', minWidth: 120 },
            { id: 'tenantId', label: 'TenantId', minWidth: 120 },
            { id: 'userId', label: 'UserId', minWidth: 120 },
            { id: 'action', label: 'Ação' },
            { id: 'resourceType', label: 'Recurso' },
            { id: 'createdAt', label: 'Criado em' },
          ]}
          rows={items.map((item) => (
            <TableRow key={item.id} hover>
              <TableCell sx={{ fontFamily: 'monospace', fontSize: '0.75rem' }}>{item.id}</TableCell>
              <TableCell sx={{ fontFamily: 'monospace', fontSize: '0.75rem' }}>
                <Tooltip title={item.tenantId}>
                  <Typography variant="body2" noWrap sx={{ maxWidth: 120 }}>
                    {item.tenantId}
                  </Typography>
                </Tooltip>
              </TableCell>
              <TableCell>{item.userId ?? '-'}</TableCell>
              <TableCell>{item.action}</TableCell>
              <TableCell>{item.resourceType}</TableCell>
              <TableCell>{new Date(item.createdAt).toLocaleString('pt-BR')}</TableCell>
            </TableRow>
          ))}
          emptyDescription="Use os filtros acima para buscar registros de auditoria."
        />
      </SectionCard>
    </Stack>
  )
}
