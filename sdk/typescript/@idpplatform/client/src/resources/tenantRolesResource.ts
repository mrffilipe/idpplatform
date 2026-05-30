import type { HttpClient } from '../api/httpClient.js'
import type { ApiPaths } from '../api/paths.js'
import type { PagedResult } from '../types.js'

export function createTenantRolesResource(http: HttpClient, paths: ApiPaths) {
  return {
    list(tenantId: string, includeInactive = false, page = 1, pageSize = 20): Promise<PagedResult<TenantRoleDto>> {
      return http.request('GET', `${paths.versionPrefix}/tenants/${tenantId}/roles`, {
        params: { includeInactive, page, pageSize },
      })
    },

    create(tenantId: string, body: { key: string; name: string }): Promise<{ id: string }> {
      return http.request('POST', `${paths.versionPrefix}/tenants/${tenantId}/roles`, { body })
    },

    update(roleId: string, body: { name?: string; isActive?: boolean }): Promise<void> {
      return http.request('PATCH', `${paths.tenantRoles}/${roleId}`, { body })
    },
  }
}

export interface TenantRoleDto {
  id: string
  key: string
  name: string
  isActive: boolean
}
