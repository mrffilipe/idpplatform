import type { PagedResult } from './common'

export interface AuditLogItem {
  id: string
  tenantId: string
  userId?: string | null
  membershipId?: string | null
  action: string
  resourceType: string
  resourceId?: string | null
  ipAddress?: string | null
  userAgent?: string | null
  createdAt: string
}

export interface ListAuditLogsFilters {
  userId?: string
  action?: string
  resourceType?: string
  from?: string
  to?: string
  page?: number
  pageSize?: number
}

export type ListAuditLogsResponse = PagedResult<AuditLogItem>
