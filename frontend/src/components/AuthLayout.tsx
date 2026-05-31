import { Box, Paper, Stack, Typography } from '@mui/material'
import type { PropsWithChildren } from 'react'
import { useThemeMode } from '../contexts/ThemeModeContext'
import { ThemeModeToggle } from './ThemeModeToggle'
import { PlatformLogo } from './ui/PlatformLogo'
import { getAuthBackground, layout, radius } from '../theme'

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

  const iconSize = 60

  return (
    <Box
      sx={{
        position: 'relative',
        minHeight: '100vh',
        display: 'flex',
        alignItems: 'center',
        justifyContent: 'center',
        px: 2,
        py: 4,
        bgcolor: getAuthBackground(mode),
      }}
    >
      <Box sx={{ position: 'absolute', top: 16, right: 16, zIndex: 1 }}>
        <ThemeModeToggle />
      </Box>
      <Box sx={{ width: '100%', maxWidth, mx: 'auto' }}>
        <Stack spacing={2.5} sx={{ mb: 3, alignItems: 'center', textAlign: 'center' }}>
          <Box
            sx={{
              display: 'inline-flex',
              alignItems: 'center',
              justifyContent: 'center',
              bgcolor: '#ffffff',
              borderRadius: `${radius.md}px`,
              border: 1,
              borderColor: 'divider',
              p: 0.85,
              boxShadow: '0 4px 20px rgba(15, 18, 30, 0.12)',
            }}
          >
            <PlatformLogo size={iconSize} />
          </Box>
          <Stack spacing={0.5} sx={{ width: '100%' }}>
            {title !== 'Kyvo' ? (
              <Typography variant="h4" component="h1">
                {title}
              </Typography>
            ) : null}
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
