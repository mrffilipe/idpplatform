import { publicApi } from '../config'
import type { BootstrapResult, PlatformStatus } from '../types'
import { apiPaths } from './httpPaths'

export async function getPlatformStatus(): Promise<PlatformStatus> {
  const { data } = await publicApi.get<PlatformStatus>(`${apiPaths.platform}/status`)
  return data
}

export async function bootstrapPlatform(): Promise<BootstrapResult> {
  const { data } = await publicApi.post<BootstrapResult>(`${apiPaths.platform}/bootstrap`)
  return data
}
