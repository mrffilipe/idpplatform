export interface PlatformStatus {
  isConfigured: boolean
  requiresBootstrap: boolean
  oauthClientId?: string | null
}

export interface BootstrapResult {
  isConfigured: boolean
  rootUserId: string
  oauthClientId: string
}
