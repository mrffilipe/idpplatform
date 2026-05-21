import AddIcon from '@mui/icons-material/Add'
import EditOutlinedIcon from '@mui/icons-material/EditOutlined'
import MailOutlineIcon from '@mui/icons-material/MailOutlineOutlined'
import SearchIcon from '@mui/icons-material/Search'
import {
  Button,
  IconButton,
  MenuItem,
  Stack,
  TableCell,
  TableRow,
  TextField,
  Tooltip,
} from '@mui/material'
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
} from '../components/ui'
import { useAuth } from '../contexts/AuthContext'
import { useTenant } from '../contexts/TenantContext'
import { createTenant, getTenantById, inviteMember, listTenants, switchTenant, updateTenant } from '../services'
import { refreshOidcTokens } from '../services/oidcService'
import type { Tenant } from '../types'
import { getAuthSession, updateSessionFromOidcRefresh } from '../utils/authStorage'
import { getApiErrorMessage } from '../utils/apiError'

const roleOptions = ['owner', 'admin', 'member', 'viewer']

export function TenantsPage() {
  const { applyTenantSwitch, platformRoles, tenantRoles } = useAuth()
  const { tenantId: selectedTenantId, selectTenant } = useTenant()
  const isPlatformAdministrator = platformRoles.includes('plat_admin')
  const hasTenantAdministrativeRole = tenantRoles.includes('owner') || tenantRoles.includes('admin')
  const canManageTenant = isPlatformAdministrator || hasTenantAdministrativeRole

  const [tenants, setTenants] = useState<Tenant[]>([])
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  const [createOpen, setCreateOpen] = useState(false)
  const [editOpen, setEditOpen] = useState(false)
  const [inviteOpen, setInviteOpen] = useState(false)
  const [lookupOpen, setLookupOpen] = useState(false)

  const [newTenantName, setNewTenantName] = useState('')
  const [newTenantKey, setNewTenantKey] = useState('')
  const [newTenantInitialAdministratorUserId, setNewTenantInitialAdministratorUserId] = useState('')

  const [editingTenant, setEditingTenant] = useState<Tenant | null>(null)
  const [editingTenantName, setEditingTenantName] = useState('')

  const [inviteTenantId, setInviteTenantId] = useState('')
  const [inviteEmail, setInviteEmail] = useState('')
  const [inviteRole, setInviteRole] = useState('member')

  const [lookupTenantId, setLookupTenantId] = useState('')
  const [lookupTenantName, setLookupTenantName] = useState('')

  useEffect(() => {
    void loadTenants()
  }, [])

  async function loadTenants(): Promise<void> {
    setError(null)
    try {
      const data = await listTenants({ page: 1, pageSize: 100 })
      setTenants(data.items)
      if (data.items.length > 0 && !inviteTenantId) {
        setInviteTenantId(data.items[0].id)
      }
    } catch (loadError) {
      setError(getApiErrorMessage(loadError))
    }
  }

  function openEditDialog(tenant: Tenant): void {
    setEditingTenant(tenant)
    setEditingTenantName(tenant.name)
    setEditOpen(true)
  }

  async function handleCreate(event: React.FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault()
    setLoading(true)
    setError(null)
    setSuccess(null)
    try {
      const created = await createTenant({
        name: newTenantName,
        key: newTenantKey,
        initialAdministratorUserId: newTenantInitialAdministratorUserId.trim() || null,
      })
      setSuccess(`Tenant criado: ${created.id}`)
      setCreateOpen(false)
      setNewTenantName('')
      setNewTenantKey('')
      setNewTenantInitialAdministratorUserId('')
      await loadTenants()
    } catch (submitError) {
      setError(getApiErrorMessage(submitError))
    } finally {
      setLoading(false)
    }
  }

  async function handleUpdate(event: React.FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault()
    if (!editingTenant) {
      return
    }
    setLoading(true)
    setError(null)
    setSuccess(null)
    try {
      await updateTenant(editingTenant.id, { name: editingTenantName })
      setSuccess('Tenant atualizado com sucesso.')
      setEditOpen(false)
      await loadTenants()
    } catch (submitError) {
      setError(getApiErrorMessage(submitError))
    } finally {
      setLoading(false)
    }
  }

  async function handleInvite(event: React.FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault()
    if (!inviteTenantId) {
      return
    }
    setLoading(true)
    setError(null)
    setSuccess(null)
    try {
      const result = await inviteMember(inviteTenantId, { email: inviteEmail, roles: [inviteRole] })
      setSuccess(`Convite criado: ${result.id}`)
      setInviteOpen(false)
      setInviteEmail('')
    } catch (submitError) {
      setError(getApiErrorMessage(submitError))
    } finally {
      setLoading(false)
    }
  }

  async function handleLookup(event: React.FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault()
    setLoading(true)
    setError(null)
    setSuccess(null)
    try {
      const found = await getTenantById(lookupTenantId)
      setLookupTenantName(found.name)
      setSuccess(`Tenant encontrado: ${found.key}`)
    } catch (lookupError) {
      setLookupTenantName('')
      setError(getApiErrorMessage(lookupError))
    } finally {
      setLoading(false)
    }
  }

  async function handleSelectTenant(tenantId: string): Promise<void> {
    setError(null)
    setSuccess(null)
    try {
      const context = await switchTenant(tenantId)
      const session = getAuthSession()
      if (session?.refreshToken) {
        const tokens = await refreshOidcTokens(session.refreshToken)
        updateSessionFromOidcRefresh(tokens)
      }
      applyTenantSwitch(context)
      selectTenant(tenantId)
      setSuccess('Tenant selecionado e sessão atualizada.')
    } catch (selectError) {
      setError(getApiErrorMessage(selectError))
    }
  }

  return (
    <Stack spacing={3}>
      <PageHeader
        title="Tenants"
        description="Organizações isoladas na plataforma. Selecione um tenant para operações contextuais."
        actions={
          <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap' }}>
            {canManageTenant ? (
              <Button startIcon={<SearchIcon />} onClick={() => setLookupOpen(true)}>
                Buscar por ID
              </Button>
            ) : null}
            {canManageTenant ? (
              <Button startIcon={<MailOutlineIcon />} onClick={() => setInviteOpen(true)}>
                Convidar membro
              </Button>
            ) : null}
            {isPlatformAdministrator ? (
              <Button startIcon={<AddIcon />} onClick={() => setCreateOpen(true)}>
                Novo tenant
              </Button>
            ) : null}
          </Stack>
        }
      />
      <FeedbackAlerts success={success} error={error} />

      <SectionCard title="Tenants cadastrados">
        <DataTable
          columns={[
            { id: 'id', label: 'Id', minWidth: 120 },
            { id: 'name', label: 'Nome' },
            { id: 'key', label: 'Chave' },
            { id: 'actions', label: 'Ações', align: 'right' },
          ]}
          rows={tenants.map((tenant) => (
            <TableRow key={tenant.id} hover>
              <TableCell sx={{ fontFamily: 'monospace', fontSize: '0.75rem' }}>{tenant.id}</TableCell>
              <TableCell>{tenant.name}</TableCell>
              <TableCell>{tenant.key}</TableCell>
              <TableCell align="right">
                <Stack direction="row" spacing={0.5} sx={{ justifyContent: 'flex-end', alignItems: 'center' }}>
                  {canManageTenant ? (
                    <Tooltip title="Editar tenant">
                      <IconButton size="small" onClick={() => openEditDialog(tenant)}>
                        <EditOutlinedIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  ) : null}
                  {selectedTenantId === tenant.id ? (
                    <StatusChip label="Selecionado" variant="success" />
                  ) : (
                    <Button size="small" onClick={() => void handleSelectTenant(tenant.id)}>
                      Selecionar
                    </Button>
                  )}
                </Stack>
              </TableCell>
            </TableRow>
          ))}
          emptyDescription="Nenhum tenant cadastrado."
        />
      </SectionCard>

      <ResourceDialog
        open={createOpen}
        onClose={() => setCreateOpen(false)}
        title="Novo tenant"
        description="Crie uma nova organização na plataforma."
        loading={loading}
        submitLabel="Criar"
        onSubmit={handleCreate}
      >
        <FormSection title="Organização" description="Nome e chave única do tenant.">
          <FormGrid>
            <FormGridItem>
              <TextField label="Nome" value={newTenantName} onChange={(e) => setNewTenantName(e.target.value)} required fullWidth />
            </FormGridItem>
            <FormGridItem>
              <TextField label="Chave" value={newTenantKey} onChange={(e) => setNewTenantKey(e.target.value)} required fullWidth />
            </FormGridItem>
          </FormGrid>
        </FormSection>
        <FormSection title="Administrador inicial" description="Opcional: vincule o owner na criação.">
          <FormGrid>
            <FormGridItem xs={12} md={12}>
              <TextField
                label="Administrador inicial (UserId opcional)"
                helperText="Se vazio, o administrador global autenticado vira owner."
                value={newTenantInitialAdministratorUserId}
                onChange={(e) => setNewTenantInitialAdministratorUserId(e.target.value)}
                fullWidth
              />
            </FormGridItem>
          </FormGrid>
        </FormSection>
      </ResourceDialog>

      <ResourceDialog
        open={editOpen}
        onClose={() => setEditOpen(false)}
        title="Editar tenant"
        description={editingTenant ? `Chave: ${editingTenant.key}` : undefined}
        loading={loading}
        submitLabel="Salvar"
        onSubmit={handleUpdate}
      >
        <FormSection title="Identificação">
          <FormGrid>
            <FormGridItem xs={12} md={12}>
              <TextField
                label="Tenant Id"
                value={editingTenant?.id ?? ''}
                slotProps={{ input: { readOnly: true } }}
                fullWidth
              />
            </FormGridItem>
            <FormGridItem>
              <TextField
                label="Nome"
                value={editingTenantName}
                onChange={(e) => setEditingTenantName(e.target.value)}
                required
                fullWidth
              />
            </FormGridItem>
          </FormGrid>
        </FormSection>
      </ResourceDialog>

      <ResourceDialog
        open={inviteOpen}
        onClose={() => setInviteOpen(false)}
        title="Convidar membro"
        description="O fluxo de aceite anônimo está em /accept-invite."
        loading={loading}
        submitLabel="Enviar convite"
        onSubmit={handleInvite}
      >
        <FormSection title="Convite" description="E-mail e papel do novo membro.">
          <FormGrid>
            <FormGridItem>
              <TextField select label="Tenant" value={inviteTenantId} onChange={(e) => setInviteTenantId(e.target.value)} fullWidth>
                {tenants.map((tenant) => (
                  <MenuItem key={tenant.id} value={tenant.id}>
                    {tenant.name}
                  </MenuItem>
                ))}
              </TextField>
            </FormGridItem>
            <FormGridItem>
              <TextField label="E-mail do convidado" value={inviteEmail} onChange={(e) => setInviteEmail(e.target.value)} required fullWidth />
            </FormGridItem>
            <FormGridItem>
              <TextField select label="Papel" value={inviteRole} onChange={(e) => setInviteRole(e.target.value)} fullWidth>
                {roleOptions.map((role) => (
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
        open={lookupOpen}
        onClose={() => setLookupOpen(false)}
        title="Buscar tenant por ID"
        loading={loading}
        submitLabel="Buscar"
        onSubmit={handleLookup}
      >
        <FormSection title="Consulta">
          <FormGrid>
            <FormGridItem>
              <TextField label="Tenant Id" value={lookupTenantId} onChange={(e) => setLookupTenantId(e.target.value)} required fullWidth />
            </FormGridItem>
            <FormGridItem>
              <TextField label="Nome encontrado" value={lookupTenantName} slotProps={{ input: { readOnly: true } }} fullWidth />
            </FormGridItem>
          </FormGrid>
        </FormSection>
      </ResourceDialog>
    </Stack>
  )
}
