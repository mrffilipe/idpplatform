import { api } from '../config'
import { normalizeMembership } from '../utils/apiMappers'
import { unwrapPagedResult } from '../utils/apiResponse'
import type {
  CreateMembershipBody,
  CreateMembershipResponse,
  ListMembershipsResponse,
  UpdateMembershipRoleBody,
} from '../types'
import { apiPaths } from './httpPaths'

export interface ListMembershipsByTenantParams {
  page?: number
  pageSize?: number
}

export async function createMembership(
  tenantId: string,
  body: CreateMembershipBody,
): Promise<CreateMembershipResponse> {
  const { data } = await api.post<CreateMembershipResponse>(
    `${apiPaths.versionPrefix}/tenants/${tenantId}/memberships`,
    body,
  )
  return data
}

export async function listMembershipsByTenant(
  tenantId: string,
  params: ListMembershipsByTenantParams = {},
): Promise<ListMembershipsResponse> {
  const { data } = await api.get(
    `${apiPaths.versionPrefix}/tenants/${tenantId}/memberships`,
    {
      params: {
        page: params.page ?? 1,
        pageSize: params.pageSize ?? 20,
      },
    },
  )
  return unwrapPagedResult(data, normalizeMembership)
}

export async function updateMembershipRole(id: string, body: UpdateMembershipRoleBody): Promise<void> {
  await api.patch(`${apiPaths.memberships}/${id}`, body)
}

export async function revokeMembership(id: string): Promise<void> {
  await api.delete(`${apiPaths.memberships}/${id}`)
}
