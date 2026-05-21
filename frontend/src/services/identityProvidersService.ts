import { api } from '../config'
import type {
  AddIdentityProviderBody,
  IdentityProviderDto,
  UpdateIdentityProviderBody,
} from '../types'
import { apiPaths } from './httpPaths'

export async function listIdentityProviders(): Promise<IdentityProviderDto[]> {
  const { data } = await api.get<IdentityProviderDto[]>(apiPaths.identityProviders)
  return data
}

export async function getIdentityProviderById(id: string): Promise<IdentityProviderDto> {
  const { data } = await api.get<IdentityProviderDto>(`${apiPaths.identityProviders}/${id}`)
  return data
}

export async function addIdentityProvider(body: AddIdentityProviderBody): Promise<{ id: string }> {
  const { data } = await api.post<{ id: string }>(apiPaths.identityProviders, body)
  return data
}

export async function updateIdentityProvider(id: string, body: UpdateIdentityProviderBody): Promise<void> {
  await api.patch(`${apiPaths.identityProviders}/${id}`, body)
}

export async function enableIdentityProvider(id: string): Promise<void> {
  await api.post(`${apiPaths.identityProviders}/${id}/enable`)
}

export async function disableIdentityProvider(id: string): Promise<void> {
  await api.post(`${apiPaths.identityProviders}/${id}/disable`)
}
