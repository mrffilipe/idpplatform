import AddIcon from '@mui/icons-material/Add'
import {
  Alert,
  Box,
  Button,
  Checkbox,
  Chip,
  FormControlLabel,
  FormGroup,
  MenuItem,
  Stack,
  TableCell,
  TableRow,
  TextField,
  Typography,
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
import { FirebaseConfigHelp } from '../components/identityProviders/FirebaseConfigHelp'
import { FIREBASE_IDP_CONFIG_EXAMPLE } from '../content/firebaseIdpConfig'
import {
  IdentityProviderType,
  IdpCapability,
  type AddIdentityProviderBody,
  type IdentityProviderDto,
  type UpdateIdentityProviderBody,
} from '../types'
import { getApiErrorMessage } from '../utils/apiError'

const providerTypeOptions: Array<{ label: string; value: IdentityProviderType }> = [
  { label: 'Local', value: IdentityProviderType.Local },
  { label: 'Firebase', value: IdentityProviderType.Firebase },
  { label: 'Amazon Cognito', value: IdentityProviderType.Cognito },
  { label: 'Generic OIDC', value: IdentityProviderType.Generic },
]

const capabilityOptions: Array<{ value: IdpCapability; label: string; locked?: IdentityProviderType }> = [
  { value: IdpCapability.LocalPassword, label: 'Email + password (local)', locked: IdentityProviderType.Local },
  { value: IdpCapability.GoogleSocial, label: 'Sign in with Google' },
  { value: IdpCapability.MicrosoftSocial, label: 'Sign in with Microsoft' },
  { value: IdpCapability.AppleSocial, label: 'Sign in with Apple' },
  { value: IdpCapability.GenericOidc, label: 'Generic OIDC' },
]

function defaultCapabilitiesFor(type: IdentityProviderType): IdpCapability[] {
  switch (type) {
    case IdentityProviderType.Local:
      return [IdpCapability.LocalPassword]
    case IdentityProviderType.Firebase:
      return [IdpCapability.GoogleSocial]
    case IdentityProviderType.Cognito:
    case IdentityProviderType.Generic:
      return [IdpCapability.GenericOidc]
    default:
      return []
  }
}

function isCapabilityAllowed(type: IdentityProviderType, capability: IdpCapability): boolean {
  if (capability === IdpCapability.LocalPassword) {
    return type === IdentityProviderType.Local
  }
  return type !== IdentityProviderType.Local
}

function providerTypeLabel(type: IdentityProviderType | undefined): string {
  return providerTypeOptions.find((o) => o.value === type)?.label ?? String(type ?? '—')
}

function capabilityLabel(capability: IdpCapability): string {
  return capabilityOptions.find((o) => o.value === capability)?.label ?? capability
}

const CONFIG_SCHEMA_HINTS: Record<IdentityProviderType, string> = {
  [IdentityProviderType.Local]: 'Local does not require ConfigJson.',
  [IdentityProviderType.Firebase]:
    'Build a JSON with three parts: projectId and webApiKey (Project settings → General) + serviceAccount (Admin SDK service account .json file). See the guide below.',
  [IdentityProviderType.Cognito]:
    'Required: userPoolId, region, clientId. Cognito login is not yet implemented — registration only.',
  [IdentityProviderType.Generic]:
    'Required: issuer, jwksUri, audience. Generic OIDC login is not yet implemented — registration only.',
}

const CONFIG_EXAMPLES: Partial<Record<IdentityProviderType, string>> = {
  [IdentityProviderType.Firebase]: FIREBASE_IDP_CONFIG_EXAMPLE,
  [IdentityProviderType.Cognito]: `{
  "userPoolId": "us-east-1_XXXXX",
  "region": "us-east-1",
  "clientId": "your-app-client-id"
}`,
  [IdentityProviderType.Generic]: `{
  "issuer": "https://idp.example.com",
  "jwksUri": "https://idp.example.com/.well-known/jwks.json",
  "audience": "your-audience"
}`,
}

function validateConfigJson(type: IdentityProviderType, json: string): string | null {
  if (type === IdentityProviderType.Local) {
    return null
  }
  const trimmed = json.trim()
  if (!trimmed) {
    return 'ConfigJson is required for this provider type.'
  }
  try {
    JSON.parse(trimmed)
    return null
  } catch {
    return 'Invalid ConfigJson: check the JSON syntax.'
  }
}

function configJsonRequired(type: IdentityProviderType): boolean {
  return type !== IdentityProviderType.Local
}

export function IdentityProvidersPage() {
  const { platformRoles } = useAuth()
  const isPlatformAdministrator = platformRoles.includes('plat_admin')

  const [items, setItems] = useState<IdentityProviderDto[]>([])
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)
  const [warnings, setWarnings] = useState<string[]>([])
  const [loading, setLoading] = useState(false)

  const [addOpen, setAddOpen] = useState(false)
  const [alias, setAlias] = useState('')
  const [displayName, setDisplayName] = useState('')
  const [providerType, setProviderType] = useState<IdentityProviderType>(IdentityProviderType.Firebase)
  const [capabilities, setCapabilities] = useState<IdpCapability[]>(defaultCapabilitiesFor(IdentityProviderType.Firebase))
  const [configJson, setConfigJson] = useState('')

  const [editOpen, setEditOpen] = useState(false)
  const [editId, setEditId] = useState('')
  const [editDisplayName, setEditDisplayName] = useState('')
  const [editConfigJson, setEditConfigJson] = useState('')
  const [editCapabilities, setEditCapabilities] = useState<IdpCapability[]>([])
  const [editProviderType, setEditProviderType] = useState<IdentityProviderType>(IdentityProviderType.Firebase)

  useEffect(() => {
    void loadProviders()
  }, [])

  useEffect(() => {
    if (!addOpen) {
      return
    }
    const example = CONFIG_EXAMPLES[providerType]
    setConfigJson(example ?? '')
    setCapabilities(defaultCapabilitiesFor(providerType))
  }, [providerType, addOpen])

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
    setCapabilities(defaultCapabilitiesFor(IdentityProviderType.Firebase))
    setConfigJson('')
    setAddOpen(true)
  }

  function openEditDialog(item: IdentityProviderDto): void {
    setEditId(item.id)
    setEditDisplayName(item.displayName)
    setEditProviderType(item.providerType)
    setEditCapabilities(item.capabilities ?? [])
    setEditConfigJson('')
    setEditOpen(true)
  }

  function toggleCapability(
    list: IdpCapability[],
    setList: (next: IdpCapability[]) => void,
    capability: IdpCapability,
    checked: boolean,
  ): void {
    if (checked && !list.includes(capability)) {
      setList([...list, capability])
    } else if (!checked) {
      setList(list.filter((c) => c !== capability))
    }
  }

  async function handleAdd(event: React.FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault()
    const configError = validateConfigJson(providerType, configJson)
    if (configError) {
      setError(configError)
      return
    }
    if (capabilities.length === 0 && providerType !== IdentityProviderType.Local) {
      setError('Select at least one capability advertised by this provider.')
      return
    }
    setLoading(true)
    setError(null)
    setSuccess(null)
    setWarnings([])
    const body: AddIdentityProviderBody = {
      alias,
      displayName,
      providerType,
      capabilities,
      configJson: configJson.trim() || null,
    }
    try {
      const result = await addIdentityProvider(body)
      setSuccess('Identity provider added.')
      setWarnings(result.warnings ?? [])
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
    if (editConfigJson.trim()) {
      const configError = validateConfigJson(editProviderType, editConfigJson)
      if (configError) {
        setError(configError)
        return
      }
    }
    setLoading(true)
    setError(null)
    setSuccess(null)
    const body: UpdateIdentityProviderBody = {
      displayName: editDisplayName,
      capabilities: editCapabilities,
      configJson: editConfigJson.trim() || null,
    }
    try {
      await updateIdentityProvider(editId, body)
      setSuccess('Identity provider updated.')
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
        setSuccess(`"${item.displayName}" disabled.`)
      } else {
        await enableIdentityProvider(item.id)
        setSuccess(`"${item.displayName}" enabled.`)
      }
      await loadProviders()
    } catch (toggleError) {
      setError(getApiErrorMessage(toggleError))
    }
  }

  if (!isPlatformAdministrator) {
    return (
      <Stack spacing={3}>
        <PageHeader title="Identity Providers" description="Manage the platform identity providers." />
        <Alert severity="warning">Only platform administrators can manage identity providers.</Alert>
      </Stack>
    )
  }

  function renderCapabilityCheckboxes(
    selectedType: IdentityProviderType,
    selected: IdpCapability[],
    onChange: (next: IdpCapability[]) => void,
  ) {
    return (
      <FormGroup>
        {capabilityOptions.map((option) => {
          const allowed = isCapabilityAllowed(selectedType, option.value)
          const isLocal = option.value === IdpCapability.LocalPassword
          const lockedOn = isLocal && selectedType === IdentityProviderType.Local
          return (
            <FormControlLabel
              key={option.value}
              control={
                <Checkbox
                  checked={selected.includes(option.value) || lockedOn}
                  disabled={!allowed || lockedOn}
                  onChange={(event) => toggleCapability(selected, onChange, option.value, event.target.checked)}
                />
              }
              label={option.label}
            />
          )
        })}
      </FormGroup>
    )
  }

  return (
    <Stack spacing={3}>
      <PageHeader
        title="Identity Providers"
        description="Manage the identity providers enabled in the platform (Local, Firebase, Cognito, etc.)."
        actions={
          <Button startIcon={<AddIcon />} onClick={openAddDialog}>
            Add IdP
          </Button>
        }
      />

      <FeedbackAlerts success={success} error={error} />

      {warnings.length > 0 && (
        <Alert severity="warning" onClose={() => setWarnings([])}>
          <Typography variant="subtitle2" component="div" sx={{ mb: 1 }}>
            Capability conflicts detected
          </Typography>
          <Stack spacing={0.5} component="ul" sx={{ pl: 2, m: 0 }}>
            {warnings.map((message) => (
              <li key={message}>{message}</li>
            ))}
          </Stack>
        </Alert>
      )}

      <SectionCard title="Registered providers">
        <DataTable
          columns={[
            { id: 'alias', label: 'Alias' },
            { id: 'displayName', label: 'Name' },
            { id: 'type', label: 'Type' },
            { id: 'capabilities', label: 'Capabilities' },
            { id: 'status', label: 'Status' },
            { id: 'actions', label: 'Actions', align: 'right' },
          ]}
          rows={items.map((item) => (
            <TableRow key={item.id} hover>
              <TableCell sx={{ fontFamily: 'monospace', fontSize: '0.8rem' }}>{item.alias}</TableCell>
              <TableCell>{item.displayName}</TableCell>
              <TableCell>{providerTypeLabel(item.providerType)}</TableCell>
              <TableCell>
                <Box sx={{ display: 'flex', flexWrap: 'wrap', gap: 0.5 }}>
                  {(item.capabilities ?? []).map((capability) => (
                    <Chip key={capability} label={capabilityLabel(capability)} size="small" variant="outlined" />
                  ))}
                </Box>
              </TableCell>
              <TableCell>
                <Chip
                  label={item.enabled ? 'Enabled' : 'Disabled'}
                  size="small"
                  color={item.enabled ? 'success' : 'default'}
                  variant="outlined"
                />
              </TableCell>
              <TableCell align="right">
                <Stack direction="row" spacing={1} sx={{ justifyContent: 'flex-end' }}>
                  <Button size="small" onClick={() => openEditDialog(item)}>
                    Edit
                  </Button>
                  <Button
                    size="small"
                    color={item.enabled ? 'warning' : 'success'}
                    onClick={() => void handleToggle(item)}
                  >
                    {item.enabled ? 'Disable' : 'Enable'}
                  </Button>
                </Stack>
              </TableCell>
            </TableRow>
          ))}
          emptyDescription="No identity providers registered yet. Add at least one beyond Local for federation."
        />
      </SectionCard>

      {/* Add dialog */}
      <ResourceDialog
        open={addOpen}
        onClose={() => setAddOpen(false)}
        title="Add identity provider"
        description="Register a new identity provider for federation."
        loading={loading}
        submitLabel="Add"
        onSubmit={handleAdd}
      >
        <FormSection title="Identification">
          <FormGrid>
            <FormGridItem>
              <TextField
                label="Alias"
                value={alias}
                onChange={(event) => setAlias(event.target.value)}
                required
                fullWidth
                helperText="Unique identifier (lowercase letters, digits and hyphens)."
              />
            </FormGridItem>
            <FormGridItem>
              <TextField
                label="Display name"
                value={displayName}
                onChange={(event) => setDisplayName(event.target.value)}
                required
                fullWidth
              />
            </FormGridItem>
            <FormGridItem xs={12}>
              <TextField
                select
                label="Type"
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
            <FormGridItem xs={12}>
              <Typography variant="subtitle2" component="div" sx={{ mb: 1 }}>
                Advertised capabilities
              </Typography>
              <Typography variant="caption" component="div" color="text.secondary" sx={{ mb: 1 }}>
                LocalPassword is hard-locked to the Local provider; one provider per platform.
                Social capabilities allow multiple providers but emit warnings on conflict.
              </Typography>
              {renderCapabilityCheckboxes(providerType, capabilities, setCapabilities)}
            </FormGridItem>
            {providerType === IdentityProviderType.Firebase && (
              <FormGridItem xs={12}>
                <FirebaseConfigHelp />
              </FormGridItem>
            )}
            {providerType !== IdentityProviderType.Firebase && (
              <FormGridItem xs={12}>
                <Alert severity="info" sx={{ width: '100%' }}>
                  {CONFIG_SCHEMA_HINTS[providerType]}
                </Alert>
              </FormGridItem>
            )}
            {(providerType === IdentityProviderType.Cognito ||
              providerType === IdentityProviderType.Generic) && (
              <FormGridItem xs={12}>
                <Alert severity="warning" sx={{ width: '100%' }}>
                  Login is not yet implemented for this type; registration prepares the provider for future use.
                </Alert>
              </FormGridItem>
            )}
            <FormGridItem xs={12}>
              <TextField
                label={
                  providerType === IdentityProviderType.Firebase
                    ? 'ConfigJson (paste the full JSON here)'
                    : configJsonRequired(providerType)
                      ? 'Configuration (JSON)'
                      : 'Configuration (JSON, optional)'
                }
                value={configJson}
                onChange={(event) => setConfigJson(event.target.value)}
                fullWidth
                multiline
                minRows={providerType === IdentityProviderType.Firebase ? 14 : 8}
                required={configJsonRequired(providerType)}
                helperText={
                  providerType === IdentityProviderType.Firebase
                    ? 'Replace the placeholders above with values from your Firebase project.'
                    : CONFIG_SCHEMA_HINTS[providerType]
                }
                slotProps={{ input: { sx: { fontFamily: 'monospace', fontSize: '0.75rem', lineHeight: 1.4 } } }}
              />
            </FormGridItem>
          </FormGrid>
        </FormSection>
      </ResourceDialog>

      {/* Edit dialog */}
      <ResourceDialog
        open={editOpen}
        onClose={() => setEditOpen(false)}
        title="Edit identity provider"
        description="Update the display name, capabilities or configuration."
        loading={loading}
        submitLabel="Save"
        onSubmit={handleEdit}
      >
        <FormSection title="Identification">
          <FormGrid>
            <FormGridItem xs={12} md={12}>
              <TextField
                label="Display name"
                value={editDisplayName}
                onChange={(event) => setEditDisplayName(event.target.value)}
                required
                fullWidth
              />
            </FormGridItem>
            <FormGridItem xs={12}>
              <Typography variant="subtitle2" component="div" sx={{ mb: 1 }}>
                Advertised capabilities
              </Typography>
              {renderCapabilityCheckboxes(editProviderType, editCapabilities, setEditCapabilities)}
            </FormGridItem>
            {editProviderType === IdentityProviderType.Firebase && (
              <FormGridItem xs={12}>
                <FirebaseConfigHelp />
              </FormGridItem>
            )}
            {editProviderType !== IdentityProviderType.Firebase && (
              <FormGridItem xs={12}>
                <Alert severity="info" sx={{ width: '100%' }}>
                  {CONFIG_SCHEMA_HINTS[editProviderType]}
                </Alert>
              </FormGridItem>
            )}
            <FormGridItem xs={12}>
              <TextField
                label={
                  editProviderType === IdentityProviderType.Firebase
                    ? 'ConfigJson (new value; empty = keep current)'
                    : 'Configuration (JSON)'
                }
                value={editConfigJson}
                onChange={(event) => setEditConfigJson(event.target.value)}
                fullWidth
                multiline
                minRows={editProviderType === IdentityProviderType.Firebase ? 12 : 6}
                helperText="Leave empty to keep the current configuration. If provided, use valid JSON in the Firebase guide format."
                slotProps={{ input: { sx: { fontFamily: 'monospace', fontSize: '0.75rem', lineHeight: 1.4 } } }}
              />
            </FormGridItem>
          </FormGrid>
        </FormSection>
      </ResourceDialog>
    </Stack>
  )
}
