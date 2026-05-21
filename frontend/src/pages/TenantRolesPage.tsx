import AddIcon from '@mui/icons-material/Add'
import EditOutlinedIcon from '@mui/icons-material/EditOutlined'
import { Button, IconButton, Stack, TableCell, TableRow, TextField, Tooltip } from '@mui/material'
import { useEffect, useState } from 'react'
import {
  CheckboxField,
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
import { createTenantRole, listTenantRoles, updateTenantRole } from '../services'
import type { TenantRole } from '../types'
import { getApiErrorMessage } from '../utils/apiError'

export function TenantRolesPage() {
  const { tenantId } = useTenant()
  const [roles, setRoles] = useState<TenantRole[]>([])
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)
  const [createOpen, setCreateOpen] = useState(false)
  const [editOpen, setEditOpen] = useState(false)
  const [loading, setLoading] = useState(false)

  const [key, setKey] = useState('')
  const [name, setName] = useState('')
  const [description, setDescription] = useState('')

  const [editingRole, setEditingRole] = useState<TenantRole | null>(null)
  const [roleName, setRoleName] = useState('')
  const [roleDescription, setRoleDescription] = useState('')
  const [roleIsActive, setRoleIsActive] = useState(true)

  useEffect(() => {
    if (!tenantId) {
      setRoles([])
      return
    }
    void loadRoles(tenantId)
  }, [tenantId])

  async function loadRoles(currentTenantId: string): Promise<void> {
    setError(null)
    try {
      const data = await listTenantRoles(currentTenantId, { includeInactive: true, page: 1, pageSize: 100 })
      setRoles(data.items)
    } catch (loadError) {
      setError(getApiErrorMessage(loadError))
    }
  }

  function openCreateDialog(): void {
    setKey('')
    setName('')
    setDescription('')
    setCreateOpen(true)
  }

  function openEditDialog(role: TenantRole): void {
    setEditingRole(role)
    setRoleName(role.name)
    setRoleDescription(role.description ?? '')
    setRoleIsActive(role.isActive)
    setEditOpen(true)
  }

  async function handleCreate(event: React.FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault()
    if (!tenantId) {
      setError('Selecione um tenant na tela de Tenants.')
      return
    }
    setLoading(true)
    setError(null)
    setSuccess(null)
    try {
      const result = await createTenantRole(tenantId, {
        key,
        name,
        description: description || null,
      })
      setSuccess(`Papel criado: ${result.id}`)
      setCreateOpen(false)
      await loadRoles(tenantId)
    } catch (createError) {
      setError(getApiErrorMessage(createError))
    } finally {
      setLoading(false)
    }
  }

  async function handleUpdate(event: React.FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault()
    if (!editingRole) {
      return
    }
    setLoading(true)
    setError(null)
    setSuccess(null)
    try {
      await updateTenantRole(editingRole.id, {
        name: roleName,
        description: roleDescription || null,
        isActive: roleIsActive,
      })
      setSuccess('Papel atualizado com sucesso.')
      setEditOpen(false)
      if (tenantId) {
        await loadRoles(tenantId)
      }
    } catch (updateError) {
      setError(getApiErrorMessage(updateError))
    } finally {
      setLoading(false)
    }
  }

  return (
    <Stack spacing={3}>
      <PageHeader
        title="Papéis do tenant"
        description="Papéis customizados para controle de acesso no tenant selecionado."
        actions={
          <Button startIcon={<AddIcon />} onClick={openCreateDialog} disabled={!tenantId}>
            Novo papel
          </Button>
        }
      />
      <FeedbackAlerts success={success} error={error} />
      <TenantScopeNotice />

      <SectionCard title="Papéis cadastrados">
        <DataTable
          columns={[
            { id: 'id', label: 'Id', minWidth: 120 },
            { id: 'key', label: 'Chave' },
            { id: 'name', label: 'Nome' },
            { id: 'description', label: 'Descrição' },
            { id: 'system', label: 'Sistema' },
            { id: 'active', label: 'Ativo' },
            { id: 'actions', label: 'Ações', align: 'right' },
          ]}
          rows={roles.map((role) => (
            <TableRow key={role.id} hover>
              <TableCell sx={{ fontFamily: 'monospace', fontSize: '0.75rem' }}>{role.id}</TableCell>
              <TableCell>{role.key}</TableCell>
              <TableCell>{role.name}</TableCell>
              <TableCell>{role.description ?? '-'}</TableCell>
              <TableCell>
                <StatusChip label={role.isSystem ? 'Sistema' : 'Custom'} variant={role.isSystem ? 'info' : 'default'} />
              </TableCell>
              <TableCell>
                <StatusChip label={role.isActive ? 'Ativo' : 'Inativo'} variant={role.isActive ? 'success' : 'default'} />
              </TableCell>
              <TableCell align="right">
                <Tooltip title="Editar papel">
                  <IconButton size="small" onClick={() => openEditDialog(role)} disabled={role.isSystem}>
                    <EditOutlinedIcon fontSize="small" />
                  </IconButton>
                </Tooltip>
              </TableCell>
            </TableRow>
          ))}
          emptyDescription={tenantId ? 'Nenhum papel cadastrado.' : 'Selecione um tenant em Tenants.'}
        />
      </SectionCard>

      <ResourceDialog
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        title="Novo papel"
        description="Defina um papel customizado para o tenant ativo."
        loading={loading}
        submitLabel="Criar"
        onSubmit={handleCreate}
        disableSubmit={!tenantId}
      >
        <FormSection title="Definição do papel" description="Chave única e nome exibido nas atribuições.">
          <FormGrid>
            <FormGridItem>
              <TextField label="Chave" value={key} onChange={(e) => setKey(e.target.value)} required fullWidth />
            </FormGridItem>
            <FormGridItem>
              <TextField label="Nome" value={name} onChange={(e) => setName(e.target.value)} required fullWidth />
            </FormGridItem>
            <FormGridItem xs={12} md={12}>
              <TextField label="Descrição" value={description} onChange={(e) => setDescription(e.target.value)} fullWidth />
            </FormGridItem>
          </FormGrid>
        </FormSection>
      </ResourceDialog>

      <ResourceDialog
        open={editOpen}
        onClose={() => setEditOpen(false)}
        title="Editar papel"
        description={editingRole ? `Chave: ${editingRole.key}` : undefined}
        loading={loading}
        submitLabel="Salvar"
        onSubmit={handleUpdate}
      >
        <FormSection title="Dados do papel">
          <FormGrid>
            <FormGridItem>
              <TextField label="Nome" value={roleName} onChange={(e) => setRoleName(e.target.value)} required fullWidth />
            </FormGridItem>
            <FormGridItem xs={12} md={12}>
              <TextField label="Descrição" value={roleDescription} onChange={(e) => setRoleDescription(e.target.value)} fullWidth />
            </FormGridItem>
          </FormGrid>
          <CheckboxField checked={roleIsActive} onCheckedChange={setRoleIsActive} label="Papel ativo" />
        </FormSection>
      </ResourceDialog>
    </Stack>
  )
}
