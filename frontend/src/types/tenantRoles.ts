import type { PagedResult } from './common'

export interface TenantRole {
  id: string
  tenantId: string
  key: string
  name: string
  description?: string | null
  isSystem: boolean
  isActive: boolean
}

export interface CreateTenantRoleBody {
  key: string
  name: string
  description?: string | null
}

export interface UpdateTenantRoleBody {
  name: string
  description?: string | null
  isActive: boolean
}

export interface CreateTenantRoleResponse {
  id: string
}

export type ListTenantRolesResponse = PagedResult<TenantRole>
