import type { PagedResult } from './common'

export interface UserMembership {
  membershipId: string
  tenantId: string
  tenantName: string
  tenantKey: string
  roles: string[]
}

export interface User {
  id: string
  email: string
  displayName: string
  photoUrl?: string | null
  memberships: UserMembership[]
}

export interface UpdateMeBody {
  displayName: string
  photoUrl?: string | null
}

export type ListUserMembershipsResponse = PagedResult<UserMembership>
