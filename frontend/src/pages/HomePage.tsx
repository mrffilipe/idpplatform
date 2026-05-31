import AppsOutlinedIcon from '@mui/icons-material/AppsOutlined'
import ArticleOutlinedIcon from '@mui/icons-material/ArticleOutlined'
import BusinessOutlinedIcon from '@mui/icons-material/BusinessOutlined'
import ChevronRightIcon from '@mui/icons-material/ChevronRight'
import GroupOutlinedIcon from '@mui/icons-material/GroupOutlined'
import KeyOutlinedIcon from '@mui/icons-material/KeyOutlined'
import PersonOutlinedIcon from '@mui/icons-material/PersonOutlined'
import SecurityOutlinedIcon from '@mui/icons-material/SecurityOutlined'
import VpnKeyOutlinedIcon from '@mui/icons-material/VpnKeyOutlined'
import { Box, Card, CardActionArea, CardContent, Grid, Stack, Typography } from '@mui/material'
import { Link } from 'react-router'
import { PageHeader } from '../components/ui'

const modules = [
  {
    to: '/profile',
    label: 'Meu Perfil',
    description: 'Atualize seus dados e visualize memberships',
    icon: <PersonOutlinedIcon />,
  },
  {
    to: '/sessions',
    label: 'Sessões',
    description: 'Gerencie sessões ativas da sua conta',
    icon: <SecurityOutlinedIcon />,
  },
  {
    to: '/tenants',
    label: 'Tenants',
    description: 'Crie, edite e selecione organizações',
    icon: <BusinessOutlinedIcon />,
  },
  {
    to: '/memberships',
    label: 'Memberships',
    description: 'Membros e papéis por tenant',
    icon: <GroupOutlinedIcon />,
  },
  {
    to: '/tenant-roles',
    label: 'Papéis do Tenant',
    description: 'Defina papéis customizados',
    icon: <VpnKeyOutlinedIcon />,
  },
  {
    to: '/applications',
    label: 'Applications',
    description: 'Aplicações OAuth registradas',
    icon: <AppsOutlinedIcon />,
  },
  {
    to: '/audit-logs',
    label: 'Audit Logs',
    description: 'Histórico de ações na plataforma',
    icon: <ArticleOutlinedIcon />,
  },
  {
    to: '/jwks',
    label: 'JWKS',
    description: 'Chaves públicas para validação de JWT',
    icon: <KeyOutlinedIcon />,
  },
]

export function HomePage() {
  return (
    <Stack spacing={3}>
      <PageHeader
        title="Dashboard"
        description="Acesso rápido aos módulos de administração da Kyvo."
      />
      <Grid container spacing={2}>
        {modules.map((module) => (
          <Grid key={module.to} size={{ xs: 12, sm: 6, lg: 4 }}>
            <Card sx={{ height: '100%' }}>
              <CardActionArea component={Link} to={module.to} sx={{ height: '100%' }}>
                <CardContent>
                  <Stack direction="row" spacing={2} sx={{ alignItems: 'center' }}>
                    <Box
                      sx={{
                        p: 1,
                        borderRadius: 2,
                        bgcolor: 'primary.main',
                        color: 'primary.contrastText',
                        display: 'flex',
                        flexShrink: 0,
                      }}
                    >
                      {module.icon}
                    </Box>
                    <Stack spacing={0.5} sx={{ flex: 1, minWidth: 0 }}>
                      <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                        {module.label}
                      </Typography>
                      <Typography variant="body2" color="text.secondary">
                        {module.description}
                      </Typography>
                    </Stack>
                    <ChevronRightIcon fontSize="small" sx={{ color: 'text.secondary', flexShrink: 0 }} />
                  </Stack>
                </CardContent>
              </CardActionArea>
            </Card>
          </Grid>
        ))}
      </Grid>
    </Stack>
  )
}
