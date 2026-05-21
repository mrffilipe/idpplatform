import SearchOffOutlinedIcon from '@mui/icons-material/SearchOffOutlined'
import { Button, Stack, Typography } from '@mui/material'
import { Link } from 'react-router'
import { SectionCard } from '../components/ui'

export function NotFoundPage() {
  return (
    <SectionCard>
      <Stack spacing={3} sx={{ alignItems: 'center', py: 4, textAlign: 'center' }}>
        <SearchOffOutlinedIcon sx={{ fontSize: 64, color: 'action.disabled' }} />
        <Stack spacing={1}>
          <Typography variant="h4">Página não encontrada</Typography>
          <Typography color="text.secondary" sx={{ maxWidth: 400 }}>
            A rota solicitada não existe nesta aplicação ou você não tem permissão para acessá-la.
          </Typography>
        </Stack>
        <Button component={Link} to="/" size="large">
          Voltar ao dashboard
        </Button>
      </Stack>
    </SectionCard>
  )
}
