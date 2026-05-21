import AddIcon from '@mui/icons-material/Add'
import DeleteIcon from '@mui/icons-material/Delete'
import EditOutlinedIcon from '@mui/icons-material/EditOutlined'
import { Button, IconButton, MenuItem, Stack, TableCell, TableRow, TextField, Tooltip } from '@mui/material'
import { useEffect, useState } from 'react'
import {
  DataTable,
  FeedbackAlerts,
  FormGrid,
  FormGridItem,
  FormSection,
  PageHeader,
  ResourceDialog,
  SectionCard,
  StatusChip,
  TenantScopeNotice,
} from '../components/ui'
import { useTenant } from '../contexts/TenantContext'
import { createMembership, listMembershipsByTenant, revokeMembership, updateMembershipRole } from '../services'
import type { Membership } from '../types'
import { getApiErrorMessage } from '../utils/apiError'

const roles = ['owner', 'admin', 'member', 'viewer']

export function MembershipsPage() {
  const { tenantId } = useTenant()
  const [items, setItems] = useState<Membership[]>([])
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)
  const [createOpen, setCreateOpen] = useState(false)
  const [editOpen, setEditOpen] = useState(false)
  const [loading, setLoading] = useState(false)

  const [userId, setUserId] = useState('')
  const [createRole, setCreateRole] = useState('member')

  const [editingMembership, setEditingMembership] = useState<Membership | null>(null)
  const [updateRole, setUpdateRole] = useState('viewer')

  useEffect(() => {
    if (!tenantId) {
      setItems([])
      return
    }
    void loadMemberships(tenantId)
  }, [tenantId])

  async function loadMemberships(currentTenantId: string): Promise<void> {
    setError(null)
    try {
      const result = await listMembershipsByTenant(currentTenantId, { page: 1, pageSize: 100 })
      setItems(result.items)
    } catch (loadError) {
      setError(getApiErrorMessage(loadError))
    }
  }

  function openCreateDialog(): void {
    setUserId('')
    setCreateRole('member')
    setCreateOpen(true)
  }

  function openEditDialog(membership: Membership): void {
    setEditingMembership(membership)
    setUpdateRole(membership.roles[0] ?? 'viewer')
    setEditOpen(true)
  }

  async function handleCreate(event: React.FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault()
    if (!tenantId) {
      setError('Selecione um tenant primeiro na tela de Tenants.')
      return
    }
    setLoading(true)
    setError(null)
    setSuccess(null)
    try {
      const created = await createMembership(tenantId, { userId, roles: [createRole] })
      setSuccess(`Membership criada: ${created.id}`)
      setCreateOpen(false)
      await loadMemberships(tenantId)
    } catch (createError) {
      setError(getApiErrorMessage(createError))
    } finally {
      setLoading(false)
    }
  }

  async function handleUpdate(event: React.FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault()
    if (!editingMembership) {
      return
    }
    setLoading(true)
    setError(null)
    setSuccess(null)
    try {
      await updateMembershipRole(editingMembership.id, { roles: [updateRole] })
      setSuccess('Papel da membership atualizado.')
      setEditOpen(false)
      if (tenantId) {
        await loadMemberships(tenantId)
      }
    } catch (updateError) {
      setError(getApiErrorMessage(updateError))
    } finally {
      setLoading(false)
    }
  }

  async function handleDelete(id: string): Promise<void> {
    setError(null)
    setSuccess(null)
    try {
      await revokeMembership(id)
      setSuccess('Membership revogada.')
      if (tenantId) {
        await loadMemberships(tenantId)
      }
    } catch (deleteError) {
      setError(getApiErrorMessage(deleteError))
    }
  }

  return (
    <Stack spacing={3}>
      <PageHeader
        title="Memberships"
        description="Membros e papéis vinculados ao tenant selecionado."
        actions={
          <Button startIcon={<AddIcon />} onClick={openCreateDialog} disabled={!tenantId}>
            Nova membership
          </Button>
        }
      />
      <FeedbackAlerts success={success} error={error} />
      <TenantScopeNotice />

      <SectionCard title="Membros do tenant">
        <DataTable
          columns={[
            { id: 'id', label: 'Id', minWidth: 120 },
            { id: 'userId', label: 'UserId', minWidth: 120 },
            { id: 'roles', label: 'Papéis' },
            { id: 'active', label: 'Ativo' },
            { id: 'actions', label: 'Ações', align: 'right' },
          ]}
          rows={items.map((item) => (
            <TableRow key={item.id} hover>
              <TableCell sx={{ fontFamily: 'monospace', fontSize: '0.75rem' }}>{item.id}</TableCell>
              <TableCell sx={{ fontFamily: 'monospace', fontSize: '0.75rem' }}>{item.userId}</TableCell>
              <TableCell>{item.roles.join(', ')}</TableCell>
              <TableCell>
                <StatusChip label={item.isActive ? 'Ativo' : 'Inativo'} variant={item.isActive ? 'success' : 'default'} />
              </TableCell>
              <TableCell align="right">
                <Tooltip title="Editar papel">
                  <IconButton size="small" onClick={() => openEditDialog(item)}>
                    <EditOutlinedIcon fontSize="small" />
                  </IconButton>
                </Tooltip>
                <Tooltip title="Revogar membership">
                  <IconButton color="error" size="small" onClick={() => void handleDelete(item.id)}>
                    <DeleteIcon fontSize="small" />
                  </IconButton>
                </Tooltip>
              </TableCell>
            </TableRow>
          ))}
          emptyDescription={tenantId ? 'Nenhuma membership neste tenant.' : 'Selecione um tenant em Tenants.'}
        />
      </SectionCard>

      <ResourceDialog
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        title="Nova membership"
        description="Vincule um usuário ao tenant ativo com um papel inicial."
        loading={loading}
        submitLabel="Criar"
        onSubmit={handleCreate}
        disableSubmit={!tenantId}
      >
        <FormSection title="Membro" description="Identificador do usuário e papel concedido.">
          <FormGrid>
            <FormGridItem>
              <TextField label="User Id" value={userId} onChange={(e) => setUserId(e.target.value)} required fullWidth />
            </FormGridItem>
            <FormGridItem>
              <TextField select label="Papel" value={createRole} onChange={(e) => setCreateRole(e.target.value)} fullWidth>
                {roles.map((role) => (
                  <MenuItem key={role} value={role}>
                    {role}
                  </MenuItem>
                ))}
              </TextField>
            </FormGridItem>
          </FormGrid>
        </FormSection>
      </ResourceDialog>

      <ResourceDialog
        open={editOpen}
        onClose={() => setEditOpen(false)}
        title="Atualizar membership"
        description={editingMembership ? `Membership ${editingMembership.id}` : undefined}
        loading={loading}
        submitLabel="Salvar"
        onSubmit={handleUpdate}
      >
        <FormSection title="Papel" description="Altere o papel principal desta membership.">
          <FormGrid>
            <FormGridItem xs={12} md={12}>
              <TextField
                label="User Id"
                value={editingMembership?.userId ?? ''}
                slotProps={{ input: { readOnly: true } }}
                fullWidth
              />
            </FormGridItem>
            <FormGridItem>
              <TextField select label="Papel" value={updateRole} onChange={(e) => setUpdateRole(e.target.value)} fullWidth>
                {roles.map((role) => (
                  <MenuItem key={role} value={role}>
                    {role}
                  </MenuItem>
                ))}
              </TextField>
            </FormGridItem>
          </FormGrid>
        </FormSection>
      </ResourceDialog>
    </Stack>
  )
}
