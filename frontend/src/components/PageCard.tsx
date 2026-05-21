import type { PropsWithChildren } from 'react'
import { SectionCard } from './ui/SectionCard'

interface PageCardProps extends PropsWithChildren {
  title: string
  subtitle?: string
}

/** @deprecated Use SectionCard directly */
export function PageCard({ title, subtitle, children }: PageCardProps) {
  return (
    <SectionCard title={title} subtitle={subtitle}>
      {children}
    </SectionCard>
  )
}
