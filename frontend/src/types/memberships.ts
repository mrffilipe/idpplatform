import type { PagedResult } from './common'

export interface Membership {
  id: string
  userId: string
  tenantId: string
  roles: string[]
  isActive: boolean
}

export interface CreateMembershipBody {
  userId: string
  roles: string[]
}

export interface UpdateMembershipRoleBody {
  roles: string[]
}

export interface CreateMembershipResponse {
  id: string
}

export type ListMembershipsResponse = PagedResult<Membership>
