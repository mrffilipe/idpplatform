import type { HttpClient } from '../api/httpClient.js'
import type { ApiPaths } from '../api/paths.js'
import type { PagedResult } from '../types.js'

export function createUsersResource(http: HttpClient, paths: ApiPaths) {
  return {
    getMe(): Promise<UserDto> {
      return http.request('GET', `${paths.users}/me`)
    },

    updateMe(body: { displayName?: string; photoUrl?: string }): Promise<void> {
      return http.request('PATCH', `${paths.users}/me`, { body })
    },

    listMyMemberships(page = 1, pageSize = 20): Promise<PagedResult<UserMembershipDto>> {
      return http.request('GET', `${paths.users}/me/memberships`, { params: { page, pageSize } })
    },
  }
}

export interface UserDto {
  id: string
  email: string
  displayName?: string
  photoUrl?: string
  createdAt: string
}

export interface UserMembershipDto {
  membershipId: string
  tenantId: string
  tenantName: string
  tenantKey: string
  roles: string[]
}
