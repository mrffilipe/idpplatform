export interface PlatformStatus {
  isConfigured: boolean
  requiresBootstrap: boolean
  oauthClientId?: string | null
}
