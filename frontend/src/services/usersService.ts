import { api } from '../config'
import type { ListUserMembershipsResponse, UpdateMeBody, User } from '../types'
import { normalizeUser, normalizeUserMembership } from '../utils/apiMappers'
import { unwrapPagedResult } from '../utils/apiResponse'
import { apiPaths } from './httpPaths'

export async function getMe(): Promise<User> {
  const { data } = await api.get(`${apiPaths.users}/me`)
  return normalizeUser(data)
}

export async function updateMe(body: UpdateMeBody): Promise<void> {
  await api.patch(`${apiPaths.users}/me`, body)
}

export interface ListUserMembershipsParams {
  page?: number
  pageSize?: number
}

export async function listMyMemberships(
  params: ListUserMembershipsParams = {},
): Promise<ListUserMembershipsResponse> {
  const { data } = await api.get(`${apiPaths.users}/me/memberships`, {
    params: {
      page: params.page ?? 1,
      pageSize: params.pageSize ?? 20,
    },
  })
  return unwrapPagedResult(data, normalizeUserMembership)
}
