import { publicApi } from '../config'
import type { PlatformStatus } from '../types'
import { apiPaths } from './httpPaths'

export async function getPlatformStatus(): Promise<PlatformStatus> {
  const { data } = await publicApi.get<PlatformStatus>(`${apiPaths.platform}/status`)
  return data
}
