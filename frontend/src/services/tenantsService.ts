import { api } from '../config'
import { normalizeTenant } from '../utils/apiMappers'
import { unwrapPagedResult } from '../utils/apiResponse'
import type {
  AcceptInviteBody,
  AcceptInviteResponse,
  CreateTenantBody,
  InviteMemberBody,
  InviteMemberResponse,
  ListTenantsResponse,
  Tenant,
  UpdateTenantBody,
} from '../types'
import { apiPaths } from './httpPaths'

export interface ListTenantsParams {
  page?: number
  pageSize?: number
}

export async function createTenant(body: CreateTenantBody): Promise<{ id: string }> {
  const { data } = await api.post<{ id: string }>(apiPaths.tenants, body)
  return data
}

export async function listTenants(params: ListTenantsParams = {}): Promise<ListTenantsResponse> {
  const { data } = await api.get(apiPaths.tenants, {
    params: {
      page: params.page ?? 1,
      pageSize: params.pageSize ?? 20,
    },
  })
  return unwrapPagedResult(data, normalizeTenant)
}

export async function getTenantById(id: string): Promise<Tenant> {
  const { data } = await api.get(`${apiPaths.tenants}/${id}`)
  return normalizeTenant(data)
}

export async function updateTenant(id: string, body: UpdateTenantBody): Promise<void> {
  await api.patch(`${apiPaths.tenants}/${id}`, body)
}

export async function inviteMember(id: string, body: InviteMemberBody): Promise<InviteMemberResponse> {
  const { data } = await api.post<InviteMemberResponse>(`${apiPaths.tenants}/${id}/invites`, body)
  return data
}

export async function acceptInvite(body: AcceptInviteBody): Promise<AcceptInviteResponse> {
  const { data } = await api.post<AcceptInviteResponse>(`${apiPaths.invites}/accept`, body)
  return data
}
