import { Box, Paper, Stack, Typography } from '@mui/material'
import type { PropsWithChildren } from 'react'
import { useThemeMode } from '../contexts/ThemeModeContext'
import { PlatformLogo } from './ui/PlatformLogo'
import { getAuthBackground, layout } from '../theme'

interface AuthLayoutProps extends PropsWithChildren {
  maxWidth?: number
  title?: string
  subtitle?: string
}

export function AuthLayout({
  children,
  maxWidth = layout.authMaxWidth,
  title = 'Kyvo',
  subtitle = 'Identity Provider centralizado para suas aplicações',
}: AuthLayoutProps) {
  const { mode } = useThemeMode()

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        px: 2,
        py: 4,
        background: getAuthBackground(mode),
      }}
    >
      <Box sx={{ width: '100%', maxWidth, mx: 'auto' }}>
        <Stack spacing={2.5} sx={{ mb: 3, alignItems: 'center', textAlign: 'center' }}>
          <PlatformLogo size={68} />
          <Stack spacing={0.5} sx={{ width: '100%' }}>
            <Typography variant="h4" component="h1">
              {title}
            </Typography>
            <Typography variant="body2" color="text.secondary">
              {subtitle}
            </Typography>
          </Stack>
        </Stack>
        <Paper sx={{ p: { xs: 2.5, sm: 3.5 } }}>{children}</Paper>
      </Box>
    </Box>
  )
}
