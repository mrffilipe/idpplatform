import EditOutlinedIcon from '@mui/icons-material/EditOutlined'
import { Alert, Avatar, Box, Button, Divider, Stack, TextField, Typography } from '@mui/material'
import { useEffect, useState } from 'react'
import {
  FeedbackAlerts,
  FormGrid,
  FormGridItem,
  FormSection,
  PageHeader,
  ResourceDialog,
  SectionCard,
  StatusChip,
} from '../components/ui'
import { getMe, updateMe } from '../services'
import type { User, UserMembership } from '../types'
import { getApiErrorMessage } from '../utils/apiError'

export function ProfilePage() {
  const [user, setUser] = useState<User | null>(null)
  const [memberships, setMemberships] = useState<UserMembership[]>([])
  const [displayName, setDisplayName] = useState('')
  const [photoUrl, setPhotoUrl] = useState('')
  const [message, setMessage] = useState<string | null>(null)
  const [error, setError] = useState<string | null>(null)
  const [loading, setLoading] = useState(false)
  const [loadingProfile, setLoadingProfile] = useState(true)
  const [editOpen, setEditOpen] = useState(false)

  useEffect(() => {
    void loadProfile()
  }, [])

  async function loadProfile(): Promise<void> {
    setError(null)
    setLoadingProfile(true)
    try {
      const data = await getMe()
      setUser(data)
      setDisplayName(data.displayName)
      setPhotoUrl(data.photoUrl ?? '')
      setMemberships(data.memberships)
    } catch (loadError) {
      setError(getApiErrorMessage(loadError))
    } finally {
      setLoadingProfile(false)
    }
  }

  function openEditDialog(): void {
    setDisplayName(user?.displayName ?? '')
    setPhotoUrl(user?.photoUrl ?? '')
    setEditOpen(true)
  }

  async function handleSubmit(event: React.FormEvent<HTMLFormElement>): Promise<void> {
    event.preventDefault()
    setLoading(true)
    setMessage(null)
    setError(null)
    try {
      await updateMe({
        displayName,
        photoUrl: photoUrl || null,
      })
      setMessage('Perfil atualizado com sucesso.')
      setEditOpen(false)
      await loadProfile()
    } catch (submitError) {
      setError(getApiErrorMessage(submitError))
    } finally {
      setLoading(false)
    }
  }

  const initials = (user?.displayName ?? user?.email ?? '?').slice(0, 2).toUpperCase()

  return (
    <Stack spacing={3}>
      <PageHeader
        title="Meu perfil"
        description="Gerencie suas informações pessoais e visualize suas memberships."
        actions={
          <Button startIcon={<EditOutlinedIcon />} onClick={openEditDialog}>
            Editar perfil
          </Button>
        }
      />
      <FeedbackAlerts success={message} error={error} />

      <SectionCard title="Identidade">
        {loadingProfile ? (
          <Typography variant="body2" color="text.secondary">
            Carregando perfil…
          </Typography>
        ) : null}
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={3} sx={{ alignItems: { sm: 'center' } }}>
          <Avatar src={user?.photoUrl || photoUrl || undefined} sx={{ width: 72, height: 72, bgcolor: 'primary.main', fontSize: '1.5rem' }}>
            {initials}
          </Avatar>
          <Box>
            <Typography variant="h6">{user?.displayName ?? '—'}</Typography>
            <Typography variant="body2" color="text.secondary">
              {user?.email ?? '—'}
            </Typography>
          </Box>
        </Stack>
      </SectionCard>

      <SectionCard title="Memberships">
        {memberships.length === 0 ? (
          <Alert severity="info">Você ainda não possui memberships ativas.</Alert>
        ) : (
          <Stack spacing={1.5} divider={<Divider flexItem />}>
            {memberships.map((membership) => (
              <Stack key={membership.membershipId} direction={{ xs: 'column', sm: 'row' }} spacing={1} sx={{ justifyContent: 'space-between' }}>
                <Box>
                  <Typography sx={{ fontWeight: 600 }}>{membership.tenantName}</Typography>
                  <Typography variant="body2" color="text.secondary">
                    {membership.tenantKey}
                  </Typography>
                </Box>
                <Stack direction="row" spacing={0.5} sx={{ flexWrap: 'wrap', gap: 0.5 }}>
                  {membership.roles.map((role) => (
                    <StatusChip key={role} label={role} variant="primary" />
                  ))}
                </Stack>
              </Stack>
            ))}
          </Stack>
        )}
      </SectionCard>

      <ResourceDialog
        open={editOpen}
        onClose={() => setEditOpen(false)}
        title="Editar perfil"
        description="Atualize como você aparece na plataforma."
        loading={loading}
        submitLabel="Salvar alterações"
        onSubmit={handleSubmit}
      >
        <FormSection title="Dados pessoais">
          <FormGrid>
            <FormGridItem>
              <TextField label="Nome de exibição" value={displayName} onChange={(e) => setDisplayName(e.target.value)} required fullWidth />
            </FormGridItem>
            <FormGridItem>
              <TextField label="URL da foto" value={photoUrl} onChange={(e) => setPhotoUrl(e.target.value)} fullWidth />
            </FormGridItem>
          </FormGrid>
        </FormSection>
      </ResourceDialog>
    </Stack>
  )
}
