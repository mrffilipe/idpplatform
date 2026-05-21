import { api } from '../config'
import { normalizeApplication } from '../utils/apiMappers'
import { unwrapPagedResult } from '../utils/apiResponse'
import type {
  Application,
  CreateApplicationBody,
  CreateApplicationClientBody,
  CreateApplicationClientResponse,
  CreateApplicationResponse,
  ListApplicationsResponse,
  ProvisionApplicationTenantBody,
  ProvisionApplicationTenantResponse,
} from '../types'
import { apiPaths } from './httpPaths'

export interface ListApplicationsParams {
  page?: number
  pageSize?: number
}

export async function createApplication(body: CreateApplicationBody): Promise<CreateApplicationResponse> {
  const { data } = await api.post<CreateApplicationResponse>(apiPaths.applications, body)
  return data
}

export async function listApplications(
  params: ListApplicationsParams = {},
): Promise<ListApplicationsResponse> {
  const { data } = await api.get(apiPaths.applications, {
    params: {
      page: params.page ?? 1,
      pageSize: params.pageSize ?? 20,
    },
  })
  return unwrapPagedResult(data, normalizeApplication)
}

export async function getApplicationById(id: string): Promise<Application> {
  const { data } = await api.get(`${apiPaths.applications}/${id}`)
  return normalizeApplication(data)
}

export async function createApplicationClient(
  applicationId: string,
  body: CreateApplicationClientBody,
): Promise<CreateApplicationClientResponse> {
  const { data } = await api.post<CreateApplicationClientResponse>(
    `${apiPaths.applications}/${applicationId}/clients`,
    body,
  )
  return data
}

export async function provisionApplicationTenant(
  applicationId: string,
  body: ProvisionApplicationTenantBody,
): Promise<ProvisionApplicationTenantResponse> {
  const { data } = await api.post<ProvisionApplicationTenantResponse>(
    `${apiPaths.applications}/${applicationId}/tenants/provision`,
    body,
  )
  return data
}
