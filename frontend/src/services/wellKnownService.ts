import { publicApi } from '../config'
import type { JwksResponse, OpenIdConfiguration } from '../types'
import { apiPaths } from './httpPaths'

export async function getOpenIdConfiguration(): Promise<OpenIdConfiguration> {
  const { data } = await publicApi.get<OpenIdConfiguration>(`${apiPaths.wellKnown}/openid-configuration`)
  return data
}

export async function getJwks(): Promise<JwksResponse> {
  const { data } = await publicApi.get<JwksResponse>(`${apiPaths.wellKnown}/jwks.json`)
  return data
}
