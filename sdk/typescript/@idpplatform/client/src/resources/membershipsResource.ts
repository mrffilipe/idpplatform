import type { HttpClient } from '../api/httpClient.js'
import type { ApiPaths } from '../api/paths.js'
import type { PagedResult } from '../types.js'

export function createMembershipsResource(http: HttpClient, paths: ApiPaths) {
  return {
    create(tenantId: string, body: { userId: string; roleKeys: string[] }): Promise<{ id: string }> {
      return http.request('POST', `${paths.versionPrefix}/tenants/${tenantId}/memberships`, { body })
    },

    listByTenant(tenantId: string, page = 1, pageSize = 20): Promise<PagedResult<MembershipDto>> {
      return http.request('GET', `${paths.versionPrefix}/tenants/${tenantId}/memberships`, {
        params: { page, pageSize },
      })
    },

    updateRoles(membershipId: string, body: { roleKeys: string[] }): Promise<void> {
      return http.request('PATCH', `${paths.memberships}/${membershipId}`, { body })
    },

    revoke(membershipId: string): Promise<void> {
      return http.request('DELETE', `${paths.memberships}/${membershipId}`)
    },
  }
}

export interface MembershipDto {
  id: string
  userId: string
  email: string
  displayName?: string
  roles: string[]
  createdAt: string
}
