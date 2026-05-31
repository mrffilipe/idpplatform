import type { HttpClient } from '../api/httpClient.js'
import type { ApiPaths } from '../api/paths.js'
import type { PagedResult } from '../types.js'

export function createTenantsResource(http: HttpClient, paths: ApiPaths) {
  return {
    list(page = 1, pageSize = 20): Promise<PagedResult<TenantDto>> {
      return http.request('GET', paths.tenants, { params: { page, pageSize } })
    },

    getById(id: string): Promise<TenantDto> {
      return http.request('GET', `${paths.tenants}/${id}`)
    },

    update(id: string, body: { name: string }): Promise<void> {
      return http.request('PATCH', `${paths.tenants}/${id}`, { body })
    },

    inviteMember(id: string, body: { email: string; roleKeys: string[] }): Promise<{ id: string }> {
      return http.request('POST', `${paths.tenants}/${id}/invites`, { body })
    },

    acceptInvite(body: { token: string }): Promise<{ membershipId: string }> {
      return http.request('POST', `${paths.invites}/accept`, { body })
    },
  }
}

export interface TenantDto {
  id: string
  name: string
  key: string
  createdAt: string
  updatedAt?: string
}
