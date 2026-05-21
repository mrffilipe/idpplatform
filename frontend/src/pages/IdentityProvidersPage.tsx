import AddIcon from '@mui/icons-material/Add'
import {
  Alert,
  Button,
  Chip,
  MenuItem,
  Stack,
  TableCell,
  TableRow,
  TextField,
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
} from '../components/ui'
import { useAuth } from '../contexts/AuthContext'
import {
  addIdentityProvider,
  disableIdentityProvider,
  enableIdentityProvider,
  listIdentityProviders,
  updateIdentityProvider,
} from '../services'
import {
  IdentityProviderType,
  type AddIdentityProviderBody,
  type IdentityProviderDto,
  type UpdateIdentityProviderBody,
} from '../types'
import { getApiErrorMessage } from '../utils/apiError'

const providerTypeOptions: Array<{ label: string; value: IdentityProviderType }> = [
  { label: 'Local', value: IdentityProviderType.Local },
  { label: 'Firebase', value: IdentityProviderType.Firebase },
  { label: 'Amazon Cognito', value: IdentityProviderType.Cognito },
  { label: 'Genérico', value: IdentityProviderType.Generic },
]

function providerTypeLabel(type: IdentityProviderType | undefined): string {
  return providerTypeOptions.find((o) => o.value === type)?.label ?? String(type ?? '—')
}

export function IdentityProvidersPage() {
  const { platformRoles } = useAuth()
  const isPlatformAdministrator = platformRoles.includes('plat_admin')

  const [items, setItems] = useState<IdentityProviderDto[]>([])
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  const [addOpen, setAddOpen] = useState(false)
  const [alias, setAlias] = useState('')
  const [displayName, setDisplayName] = useState('')
  const [providerType, setProviderType] = useState<IdentityProviderType>(IdentityProviderType.Firebase)
  const [configJson, setConfigJson] = useState('')

  const [editOpen, setEditOpen] = useState(false)
  const [editId, setEditId] = useState('')
  const [editDisplayName, setEditDisplayName] = useState('')
  const [editConfigJson, setEditConfigJson] = useState('')

  useEffect(() => {
    void loadProviders()
  }, [])

  async function loadProviders(): Promise<void> {
    setError(null)
    try {
      const result = await listIdentityProviders()
      setItems(result)
    } catch (loadError) {
      setError(getApiErrorMessage(loadError))
    }
  }

  function openAddDialog(): void {
    setAlias('')
    setDisplayName('')
    setProviderType(IdentityProviderType.Firebase)
    setConfigJson('')
    setAddOpen(true)
  }

  function openEditDialog(item: IdentityProviderDto): void {
    setEditId(item.id)
    setEditDisplayName(item.displayName)
    setEditConfigJson('')
    setEditOpen(true)
  }

  async function handleAdd(event: React.FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault()
    setLoading(true)
    setError(null)
    setSuccess(null)
    const body: AddIdentityProviderBody = {
      alias,
      displayName,
      providerType,
      configJson: configJson.trim() || null,
    }
    try {
      await addIdentityProvider(body)
      setSuccess('Identity provider adicionado.')
      setAddOpen(false)
      await loadProviders()
    } catch (addError) {
      setError(getApiErrorMessage(addError))
    } finally {
      setLoading(false)
    }
  }

  async function handleEdit(event: React.FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault()
    setLoading(true)
    setError(null)
    setSuccess(null)
    const body: UpdateIdentityProviderBody = {
      displayName: editDisplayName,
      configJson: editConfigJson.trim() || null,
    }
    try {
      await updateIdentityProvider(editId, body)
      setSuccess('Identity provider atualizado.')
      setEditOpen(false)
      await loadProviders()
    } catch (editError) {
      setError(getApiErrorMessage(editError))
    } finally {
      setLoading(false)
    }
  }

  async function handleToggle(item: IdentityProviderDto): Promise<void> {
    setError(null)
    setSuccess(null)
    try {
      if (item.enabled) {
        await disableIdentityProvider(item.id)
        setSuccess(`"${item.displayName}" desabilitado.`)
      } else {
        await enableIdentityProvider(item.id)
        setSuccess(`"${item.displayName}" habilitado.`)
      }
      await loadProviders()
    } catch (toggleError) {
      setError(getApiErrorMessage(toggleError))
    }
  }

  if (!isPlatformAdministrator) {
    return (
      <Stack spacing={3}>
        <PageHeader title="Identity Providers" description="Gerencie provedores de identidade da plataforma." />
        <Alert severity="warning">Apenas administradores de plataforma podem gerenciar identity providers.</Alert>
      </Stack>
    )
  }

  return (
    <Stack spacing={3}>
      <PageHeader
        title="Identity Providers"
        description="Gerencie os provedores de identidade habilitados na plataforma (Local, Firebase, Cognito, etc.)."
        actions={
          <Button startIcon={<AddIcon />} onClick={openAddDialog}>
            Adicionar IdP
          </Button>
        }
      />

      <FeedbackAlerts success={success} error={error} />

      <SectionCard title="Provedores cadastrados">
        <DataTable
          columns={[
            { id: 'alias', label: 'Alias' },
            { id: 'displayName', label: 'Nome' },
            { id: 'type', label: 'Tipo' },
            { id: 'status', label: 'Status' },
            { id: 'actions', label: 'Ações', align: 'right' },
          ]}
          rows={items.map((item) => (
            <TableRow key={item.id} hover>
              <TableCell sx={{ fontFamily: 'monospace', fontSize: '0.8rem' }}>{item.alias}</TableCell>
              <TableCell>{item.displayName}</TableCell>
              <TableCell>{providerTypeLabel(item.providerType)}</TableCell>
              <TableCell>
                <Chip
                  label={item.enabled ? 'Habilitado' : 'Desabilitado'}
                  size="small"
                  color={item.enabled ? 'success' : 'default'}
                  variant="outlined"
                />
              </TableCell>
              <TableCell align="right">
                <Stack direction="row" spacing={1} justifyContent="flex-end">
                  <Button size="small" onClick={() => openEditDialog(item)}>
                    Editar
                  </Button>
                  <Button
                    size="small"
                    color={item.enabled ? 'warning' : 'success'}
                    onClick={() => void handleToggle(item)}
                  >
                    {item.enabled ? 'Desabilitar' : 'Habilitar'}
                  </Button>
                </Stack>
              </TableCell>
            </TableRow>
          ))}
          emptyDescription="Nenhum identity provider cadastrado. Adicione ao menos um além do Local para federação."
        />
      </SectionCard>

      {/* Add dialog */}
      <ResourceDialog
        open={addOpen}
        onClose={() => setAddOpen(false)}
        title="Adicionar identity provider"
        description="Configure um novo provedor de identidade para federação."
        loading={loading}
        submitLabel="Adicionar"
        onSubmit={handleAdd}
      >
        <FormSection title="Identificação">
          <FormGrid>
            <FormGridItem>
              <TextField
                label="Alias"
                value={alias}
                onChange={(event) => setAlias(event.target.value)}
                required
                fullWidth
                helperText="Identificador único, letras minúsculas, números e hífens."
              />
            </FormGridItem>
            <FormGridItem>
              <TextField
                label="Nome de exibição"
                value={displayName}
                onChange={(event) => setDisplayName(event.target.value)}
                required
                fullWidth
              />
            </FormGridItem>
            <FormGridItem>
              <TextField
                select
                label="Tipo"
                value={providerType}
                onChange={(event) => setProviderType(event.target.value as IdentityProviderType)}
                fullWidth
              >
                {providerTypeOptions.map((option) => (
                  <MenuItem key={option.value} value={option.value}>
                    {option.label}
                  </MenuItem>
                ))}
              </TextField>
            </FormGridItem>
            <FormGridItem xs={12} md={12}>
              <TextField
                label="Configuração (JSON opcional)"
                value={configJson}
                onChange={(event) => setConfigJson(event.target.value)}
                fullWidth
                multiline
                minRows={3}
                helperText="JSON livre com configurações específicas do provedor (ex: projectId, region)."
              />
            </FormGridItem>
          </FormGrid>
        </FormSection>
      </ResourceDialog>

      {/* Edit dialog */}
      <ResourceDialog
        open={editOpen}
        onClose={() => setEditOpen(false)}
        title="Editar identity provider"
        description="Atualize o nome de exibição ou a configuração do provedor."
        loading={loading}
        submitLabel="Salvar"
        onSubmit={handleEdit}
      >
        <FormSection title="Identificação">
          <FormGrid>
            <FormGridItem xs={12} md={12}>
              <TextField
                label="Nome de exibição"
                value={editDisplayName}
                onChange={(event) => setEditDisplayName(event.target.value)}
                required
                fullWidth
              />
            </FormGridItem>
            <FormGridItem xs={12} md={12}>
              <TextField
                label="Configuração (JSON opcional)"
                value={editConfigJson}
                onChange={(event) => setEditConfigJson(event.target.value)}
                fullWidth
                multiline
                minRows={3}
                helperText="Deixe vazio para manter a configuração atual."
              />
            </FormGridItem>
          </FormGrid>
        </FormSection>
      </ResourceDialog>
    </Stack>
  )
}
