import type { ReactNode } from 'react'
import InboxOutlinedIcon from '@mui/icons-material/InboxOutlined'
import { Box, Typography } from '@mui/material'

interface EmptyStateProps {
  title?: string
  description?: string
  icon?: ReactNode
}

export function EmptyState({
  title = 'Nenhum registro encontrado',
  description,
  icon,
}: EmptyStateProps) {
  return (
    <Box
      sx={{
        py: 6,
        px: 2,
        textAlign: 'center',
        color: 'text.secondary',
      }}
    >
      <Box sx={{ mb: 1.5, color: 'action.disabled' }}>{icon ?? <InboxOutlinedIcon sx={{ fontSize: 48 }} />}</Box>
      <Typography variant="subtitle1" color="text.primary" gutterBottom>
        {title}
      </Typography>
      {description ? (
        <Typography variant="body2" color="text.secondary">
          {description}
        </Typography>
      ) : null}
    </Box>
  )
}
