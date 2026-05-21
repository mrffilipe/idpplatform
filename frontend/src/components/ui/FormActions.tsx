import type { PropsWithChildren } from 'react'
import { Stack } from '@mui/material'

interface FormActionsProps extends PropsWithChildren {
  /** Alinha o grupo à direita do container (padrão). Use `false` só em ações isoladas centralizadas (ex.: etapa 1 do bootstrap). */
  alignEnd?: boolean
}

export function FormActions({ children, alignEnd = true }: FormActionsProps) {
  return (
    <Stack
      direction="row"
      spacing={2}
      sx={{
        pt: 1,
        width: '100%',
        alignItems: 'center',
        justifyContent: alignEnd ? 'flex-end' : 'center',
      }}
    >
      {children}
    </Stack>
  )
}
