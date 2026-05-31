import type { HttpClient } from '../api/httpClient.js'
import type { ApiPaths } from '../api/paths.js'
import type { PagedResult } from '../types.js'

export interface ListAuditLogsFilters {
  page?: number
  pageSize?: number
  action?: string
  from?: string
  to?: string
}

export function createAuditLogsResource(http: HttpClient, paths: ApiPaths) {
  return {
    list(filters: ListAuditLogsFilters = {}): Promise<PagedResult<AuditLogItemDto>> {
      return http.request('GET', paths.auditLogs, {
        params: {
          page: filters.page ?? 1,
          pageSize: filters.pageSize ?? 20,
          action: filters.action,
          from: filters.from,
          to: filters.to,
        },
      })
    },
  }
}

export interface AuditLogItemDto {
  id: string
  action: string
  actorUserId?: string
  actorEmail?: string
  occurredAt: string
  resourceType?: string
  resourceId?: string
  details?: string
}
