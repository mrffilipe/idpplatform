import { Alert } from '@mui/material'
import { useTenant } from '../../contexts/TenantContext'

export function TenantScopeNotice() {
  const { tenantId } = useTenant()

  if (!tenantId) {
    return <Alert severity="warning">Selecione um tenant na tela de Tenants para operar neste módulo.</Alert>
  }

  return (
    <Alert severity="info" sx={{ fontFamily: 'monospace', fontSize: '0.8125rem' }}>
      Tenant ativo: {tenantId}
    </Alert>
  )
}
