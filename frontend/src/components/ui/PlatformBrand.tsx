import { Box, Typography, type SxProps, type Theme } from '@mui/material'
import { Link } from 'react-router'
import { PlatformLogo } from './PlatformLogo'

interface PlatformBrandProps {
  logoSize?: number
  showTitle?: boolean
  to?: string
  sx?: SxProps<Theme>
}

export function PlatformBrand({ logoSize = 32, showTitle = true, to = '/', sx }: PlatformBrandProps) {
  const inner = (
    <>
      <PlatformLogo size={logoSize} sx={{ mx: 0, flexShrink: 0 }} />
      {showTitle ? (
        <Typography variant="h6" component="span" sx={{ fontWeight: 700, letterSpacing: '-0.02em', lineHeight: 1.2 }}>
          IdP Platform
        </Typography>
      ) : null}
    </>
  )

  const boxSx = [
    {
      display: 'inline-flex',
      alignItems: 'center',
      gap: 1.25,
      color: 'text.primary',
      textDecoration: 'none',
    },
    ...(Array.isArray(sx) ? sx : sx ? [sx] : []),
  ] as const

  if (to) {
    return (
      <Box component={Link} to={to} sx={boxSx}>
        {inner}
      </Box>
    )
  }

  return <Box sx={boxSx}>{inner}</Box>
}
