import { api } from '../config'
import type {
  CreateTenantRoleBody,
  CreateTenantRoleResponse,
  ListTenantRolesResponse,
  UpdateTenantRoleBody,
} from '../types'
import { normalizeTenantRole } from '../utils/apiMappers'
import { unwrapPagedResult } from '../utils/apiResponse'
import { compactQuery } from '../utils/queryParams'
import { apiPaths } from './httpPaths'

export interface ListTenantRolesParams {
  includeInactive?: boolean
  page?: number
  pageSize?: number
}

export async function listTenantRoles(
  tenantId: string,
  params: ListTenantRolesParams = {},
): Promise<ListTenantRolesResponse> {
  const { data } = await api.get(`${apiPaths.versionPrefix}/tenants/${tenantId}/roles`, {
    params: compactQuery({
      includeInactive: params.includeInactive ?? false,
      page: params.page ?? 1,
      pageSize: params.pageSize ?? 20,
    }),
  })
  return unwrapPagedResult(data, normalizeTenantRole)
}

export async function createTenantRole(
  tenantId: string,
  body: CreateTenantRoleBody,
): Promise<CreateTenantRoleResponse> {
  const { data } = await api.post<CreateTenantRoleResponse>(
    `${apiPaths.versionPrefix}/tenants/${tenantId}/roles`,
    body,
  )
  return data
}

export async function updateTenantRole(id: string, body: UpdateTenantRoleBody): Promise<void> {
  await api.patch(`${apiPaths.tenantRoles}/${id}`, body)
}
