import AddIcon from '@mui/icons-material/Add'
import BusinessOutlinedIcon from '@mui/icons-material/BusinessOutlined'
import ContentCopyIcon from '@mui/icons-material/ContentCopy'
import { Alert, Box, Button, Chip, IconButton, MenuItem, Stack, TextField, Tooltip, Typography } from '@mui/material'
import { useEffect, useState } from 'react'
import { useParams } from 'react-router'
import {
  FeedbackAlerts,
  FormGrid,
  FormGridItem,
  FormSection,
  PageHeader,
  SectionCard,
  SteppedFormDialog,
} from '../components/ui'
import { useAuth } from '../contexts/AuthContext'
import { createApplicationClient, getApplicationById, provisionApplicationTenant } from '../services'
import { ClientType, type Application } from '../types'
import { getApiErrorMessage } from '../utils/apiError'
import { applicationTypeLabel } from '../utils/enumLabels'

const clientTypeOptions: Array<{ label: string; value: ClientType }> = [
  { label: 'Público', value: ClientType.Public },
  { label: 'Confidencial', value: ClientType.Confidential },
]

const clientSteps = ['Credenciais', 'URIs e tokens'] as const
const provisionSteps = ['Tenant', 'Metadados'] as const

export function ApplicationDetailPage() {
  const { applicationId } = useParams()
  const { platformRoles } = useAuth()
  const [application, setApplication] = useState<Application | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [success, setSuccess] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)

  const [clientOpen, setClientOpen] = useState(false)
  const [clientStep, setClientStep] = useState(0)
  const [provisionOpen, setProvisionOpen] = useState(false)
  const [provisionStep, setProvisionStep] = useState(0)

  const [clientId, setClientId] = useState('')
  const [clientSecretHash, setClientSecretHash] = useState('')
  const [clientType, setClientType] = useState<ClientType>(ClientType.Public)
  const [redirectUris, setRedirectUris] = useState('')
  const [allowedScopes, setAllowedScopes] = useState('')
  const [accessTokenTtlSeconds, setAccessTokenTtlSeconds] = useState('3600')

  const [tenantName, setTenantName] = useState('')
  const [tenantKey, setTenantKey] = useState('')
  const [initialAdministratorUserId, setInitialAdministratorUserId] = useState('')
  const [externalCustomerId, setExternalCustomerId] = useState('')
  const [planCode, setPlanCode] = useState('')

  const isPlatformAdministrator = platformRoles.includes('plat_admin')
  const isSystemApp = application?.isSystem ?? false
  const canCreateClient = isPlatformAdministrator && !isSystemApp
  const canProvisionTenant = isPlatformAdministrator && !isSystemApp

  useEffect(() => {
    if (!applicationId) {
      return
    }
    void loadApplication(applicationId)
  }, [applicationId])

  async function loadApplication(id: string): Promise<void> {
    setError(null)
    try {
      const data = await getApplicationById(id)
      setApplication(data)
    } catch (loadError) {
      setError(getApiErrorMessage(loadError))
    }
  }

  function openClientDialog(): void {
    setClientStep(0)
    setClientId('')
    setClientSecretHash('')
    setClientType(ClientType.Public)
    setRedirectUris('')
    setAllowedScopes('')
    setAccessTokenTtlSeconds('3600')
    setClientOpen(true)
  }

  function openProvisionDialog(): void {
    setProvisionStep(0)
    setTenantName('')
    setTenantKey('')
    setInitialAdministratorUserId('')
    setExternalCustomerId('')
    setPlanCode('')
    setProvisionOpen(true)
  }

  async function handleCreateClient(event: React.FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault()
    if (!canCreateClient || !applicationId) {
      setError('Sem permissão para criar client.')
      return
    }
    setLoading(true)
    setError(null)
    setSuccess(null)
    try {
      const created = await createApplicationClient(applicationId, {
        clientId,
        clientSecretHash: clientSecretHash || null,
        clientType,
        redirectUris,
        allowedScopes,
        accessTokenTtlSeconds: Number(accessTokenTtlSeconds),
      })
      setSuccess(`Client criado: ${created.id}`)
      setClientOpen(false)
    } catch (createError) {
      setError(getApiErrorMessage(createError))
    } finally {
      setLoading(false)
    }
  }

  async function handleProvisionTenant(event: React.FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault()
    if (!applicationId || !isPlatformAdministrator) {
      return
    }
    setLoading(true)
    setError(null)
    setSuccess(null)
    try {
      const provisioned = await provisionApplicationTenant(applicationId, {
        tenantName,
        tenantKey,
        initialAdministratorUserId: initialAdministratorUserId.trim() || null,
        externalCustomerId: externalCustomerId.trim() || null,
        planCode: planCode.trim() || null,
      })
      setSuccess(`Tenant provisionado: ${provisioned.tenantId}`)
      setProvisionOpen(false)
    } catch (provisionError) {
      setError(getApiErrorMessage(provisionError))
    } finally {
      setLoading(false)
    }
  }

  async function copySecret(): Promise<void> {
    if (clientSecretHash) {
      await navigator.clipboard.writeText(clientSecretHash)
    }
  }

  const clientStepValid =
    clientStep === 0
      ? Boolean(clientId.trim())
      : Boolean(redirectUris.trim() && allowedScopes.trim() && accessTokenTtlSeconds.trim())

  const provisionStepValid =
    provisionStep === 0 ? Boolean(tenantName.trim() && tenantKey.trim()) : true

  return (
    <Stack spacing={3}>
      <PageHeader
        title={application?.name ?? 'Application'}
        description={application ? `Slug: ${application.slug}` : 'Carregando detalhes...'}
        actions={
          <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap', alignItems: 'center' }}>
            {isSystemApp ? (
              <Chip label="Sistema" size="small" color="default" variant="outlined" />
            ) : null}
            {canCreateClient ? (
              <Button startIcon={<AddIcon />} onClick={openClientDialog}>
                Novo client OAuth
              </Button>
            ) : null}
            {canProvisionTenant ? (
              <Button startIcon={<BusinessOutlinedIcon />} onClick={openProvisionDialog}>
                Provisionar tenant
              </Button>
            ) : null}
          </Stack>
        }
      />
      <FeedbackAlerts success={success} error={error} />

      {isSystemApp ? (
        <Alert severity="info">Esta é uma aplicação de sistema gerida pela plataforma e não pode ser modificada.</Alert>
      ) : (
        <>
          {!canCreateClient ? (
            <Alert severity="info">Para criar clients OAuth, use uma conta de administrador de plataforma.</Alert>
          ) : null}
          {!isPlatformAdministrator ? (
            <Alert severity="info">Apenas administradores de plataforma podem provisionar tenants por aplicação.</Alert>
          ) : null}
        </>
      )}

      <SectionCard title="Informações da aplicação">
        <Stack spacing={1.5}>
          <Box>
            <Typography variant="caption" color="text.secondary">
              ID
            </Typography>
            <Typography sx={{ fontFamily: 'monospace', fontSize: '0.875rem' }}>{application?.id ?? applicationId}</Typography>
          </Box>
          <Box>
            <Typography variant="caption" color="text.secondary">
              Nome
            </Typography>
            <Typography>{application?.name ?? '—'}</Typography>
          </Box>
          <Box>
            <Typography variant="caption" color="text.secondary">
              Slug
            </Typography>
            <Typography>{application?.slug ?? '—'}</Typography>
          </Box>
          <Box>
            <Typography variant="caption" color="text.secondary">
              Tipo
            </Typography>
            <Typography>{applicationTypeLabel(application?.type)}</Typography>
          </Box>
          {application?.isSystem ? (
            <Box>
              <Typography variant="caption" color="text.secondary">
                Classificação
              </Typography>
              <Typography>
                <Chip label="Sistema" size="small" color="default" variant="outlined" />
              </Typography>
            </Box>
          ) : null}
        </Stack>
      </SectionCard>

      <SteppedFormDialog
        open={clientOpen}
        onClose={() => setClientOpen(false)}
        title="Novo client OAuth"
        description="Configure credenciais e parâmetros de autorização."
        steps={clientSteps}
        activeStep={clientStep}
        loading={loading}
        submitLabel="Criar client"
        onBack={() => setClientStep((step) => step - 1)}
        onNext={() => setClientStep((step) => step + 1)}
        onSubmit={handleCreateClient}
        disableNext={!clientStepValid}
        disableSubmit={!clientStepValid}
      >
        {clientStep === 0 ? (
          <FormSection title="Credenciais" description="Identificador e tipo do client.">
            <FormGrid>
              <FormGridItem>
                <TextField label="Client Id" value={clientId} onChange={(e) => setClientId(e.target.value)} required fullWidth />
              </FormGridItem>
              <FormGridItem>
                <TextField
                  select
                  label="Tipo do client"
                  value={clientType}
                  onChange={(e) => setClientType(e.target.value as ClientType)}
                  fullWidth
                >
                  {clientTypeOptions.map((option) => (
                    <MenuItem key={option.value} value={option.value}>
                      {option.label}
                    </MenuItem>
                  ))}
                </TextField>
              </FormGridItem>
              <FormGridItem xs={12} md={12}>
                <TextField
                  label="Hash do client secret"
                  value={clientSecretHash}
                  onChange={(e) => setClientSecretHash(e.target.value)}
                  fullWidth
                  slotProps={{
                    input: {
                      endAdornment: clientSecretHash ? (
                        <Tooltip title="Copiar">
                          <IconButton size="small" onClick={() => void copySecret()}>
                            <ContentCopyIcon fontSize="small" />
                          </IconButton>
                        </Tooltip>
                      ) : undefined,
                    },
                  }}
                />
              </FormGridItem>
            </FormGrid>
          </FormSection>
        ) : (
          <FormSection title="URIs e tokens" description="Redirect URIs, scopes e validade do access token.">
            <FormGrid>
              <FormGridItem xs={12} md={12}>
                <TextField label="Redirect URIs" value={redirectUris} onChange={(e) => setRedirectUris(e.target.value)} required fullWidth />
              </FormGridItem>
              <FormGridItem xs={12} md={12}>
                <TextField label="Scopes permitidos" value={allowedScopes} onChange={(e) => setAllowedScopes(e.target.value)} required fullWidth />
              </FormGridItem>
              <FormGridItem>
                <TextField
                  label="TTL do access token (segundos)"
                  value={accessTokenTtlSeconds}
                  onChange={(e) => setAccessTokenTtlSeconds(e.target.value)}
                  required
                  fullWidth
                />
              </FormGridItem>
            </FormGrid>
          </FormSection>
        )}
      </SteppedFormDialog>

      <SteppedFormDialog
        open={provisionOpen}
        onClose={() => setProvisionOpen(false)}
        title="Provisionar tenant"
        description="Onboarding de cliente SaaS após assinatura."
        steps={provisionSteps}
        activeStep={provisionStep}
        loading={loading}
        submitLabel="Provisionar"
        onBack={() => setProvisionStep((step) => step - 1)}
        onNext={() => setProvisionStep((step) => step + 1)}
        onSubmit={handleProvisionTenant}
        disableNext={!provisionStepValid}
        disableSubmit={!provisionStepValid}
      >
        {provisionStep === 0 ? (
          <FormSection title="Tenant" description="Nome e chave da organização provisionada.">
            <FormGrid>
              <FormGridItem>
                <TextField label="Nome do tenant" value={tenantName} onChange={(e) => setTenantName(e.target.value)} required fullWidth />
              </FormGridItem>
              <FormGridItem>
                <TextField label="Chave do tenant" value={tenantKey} onChange={(e) => setTenantKey(e.target.value)} required fullWidth />
              </FormGridItem>
              <FormGridItem xs={12} md={12}>
                <TextField
                  label="Admin inicial (UserId opcional)"
                  value={initialAdministratorUserId}
                  onChange={(e) => setInitialAdministratorUserId(e.target.value)}
                  fullWidth
                />
              </FormGridItem>
            </FormGrid>
          </FormSection>
        ) : (
          <FormSection title="Metadados opcionais" description="Integração com billing ou CRM externo.">
            <FormGrid>
              <FormGridItem>
                <TextField
                  label="External Customer Id (opcional)"
                  value={externalCustomerId}
                  onChange={(e) => setExternalCustomerId(e.target.value)}
                  fullWidth
                />
              </FormGridItem>
              <FormGridItem>
                <TextField label="Plano (opcional)" value={planCode} onChange={(e) => setPlanCode(e.target.value)} fullWidth />
              </FormGridItem>
            </FormGrid>
          </FormSection>
        )}
      </SteppedFormDialog>
    </Stack>
  )
}
