import type { PropsWithChildren } from 'react'
import { Stack, Typography } from '@mui/material'

interface FormSectionProps extends PropsWithChildren {
  title: string
  description?: string
}

export function FormSection({ title, description, children }: FormSectionProps) {
  return (
    <Stack
      spacing={2}
      sx={{
        pt: 0.5,
        '&:not(:first-of-type)': {
          pt: 2.5,
          borderTop: 1,
          borderColor: 'divider',
        },
      }}
    >
      <Stack spacing={0.25}>
        <Typography variant="subtitle2" component="h3" sx={{ fontWeight: 600, letterSpacing: '0.01em' }}>
          {title}
        </Typography>
        {description ? (
          <Typography variant="caption" color="text.secondary" sx={{ lineHeight: 1.45 }}>
            {description}
          </Typography>
        ) : null}
      </Stack>
      {children}
    </Stack>
  )
}
