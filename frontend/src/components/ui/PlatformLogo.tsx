import ShieldOutlinedIcon from '@mui/icons-material/ShieldOutlined'
import { Box, type SxProps, type Theme } from '@mui/material'
import { alpha } from '@mui/material/styles'

interface PlatformLogoProps {
  size?: number
  sx?: SxProps<Theme>
}

export function PlatformLogo({ size = 56, sx }: PlatformLogoProps) {
  return (
    <Box
      aria-hidden
      sx={[
        {
          mx: 'auto',
          width: size,
          height: size,
          borderRadius: 2,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          border: '1.5px solid',
          borderColor: 'primary.main',
          color: 'primary.main',
          bgcolor: (theme) => alpha(theme.palette.primary.main, theme.palette.mode === 'dark' ? 0.12 : 0.06),
          transition: 'transform 0.2s ease, box-shadow 0.2s ease',
        },
        ...(Array.isArray(sx) ? sx : sx ? [sx] : []),
      ]}
    >
      <ShieldOutlinedIcon sx={{ fontSize: size * 0.5 }} />
    </Box>
  )
}
