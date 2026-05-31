import { createApiPaths } from './api/paths.js'
import { createHttpClientFromSession } from './api/httpClient.js'
import * as claims from './claims/parseClaims.js'
import { OidcClient } from './oidc/oidcClient.js'
import { createAuthResource } from './resources/authResource.js'
import { createUsersResource } from './resources/usersResource.js'
import { createTenantsResource } from './resources/tenantsResource.js'
import { createMembershipsResource } from './resources/membershipsResource.js'
import { createTenantRolesResource } from './resources/tenantRolesResource.js'
import { createAuditLogsResource } from './resources/auditLogsResource.js'
import { SessionManager } from './session/sessionManager.js'
import { createMemoryStorage } from './session/memoryStorage.js'
import { hasTenant } from './claims/parseClaims.js'
import type { KyvoClientConfig, OidcTokenResponse } from './types.js'

const DEFAULT_SCOPES = 'openid profile email offline_access'

export interface KyvoClient {
  oidc: OidcClient
  session: SessionManager
  claims: typeof claims
  auth: ReturnType<typeof createAuthResource>
  users: ReturnType<typeof createUsersResource>
  tenants: ReturnType<typeof createTenantsResource>
  memberships: ReturnType<typeof createMembershipsResource>
  tenantRoles: ReturnType<typeof createTenantRolesResource>
  auditLogs: ReturnType<typeof createAuditLogsResource>

  getAccessToken(): string | null

  /** Refresh until JWT contains tid (post-subscribe / switch-tenant). */
  refreshAccessTokenWithTenant(): Promise<OidcTokenResponse>
}

export function createKyvoClient(config: KyvoClientConfig): KyvoClient {
  const apiVersion = config.apiVersion ?? '1.0'
  const authority = config.authority.replace(/\/$/, '')
  const paths = createApiPaths(apiVersion)

  const sessionStorage = config.oidc.storage ?? (
    typeof globalThis !== 'undefined' && 'localStorage' in globalThis
      ? globalThis.localStorage
      : createMemoryStorage()
  )

  const pkceStorage =
    config.oidc.pkceStorage ??
    (typeof globalThis !== 'undefined' && 'sessionStorage' in globalThis
      ? globalThis.sessionStorage
      : createMemoryStorage())

  const session = new SessionManager(sessionStorage)
  const oidc = new OidcClient({
    authority,
    clientId: config.oidc.clientId,
    redirectUri: config.oidc.redirectUri,
    scopes: config.oidc.scopes ?? DEFAULT_SCOPES,
    pkceStorage,
  })

  const http = createHttpClientFromSession(authority, session, (rt) => oidc.refresh(rt))

  const client: KyvoClient = {
    oidc,
    session,
    claims,
    auth: createAuthResource(http, paths),
    users: createUsersResource(http, paths),
    tenants: createTenantsResource(http, paths),
    memberships: createMembershipsResource(http, paths),
    tenantRoles: createTenantRolesResource(http, paths),
    auditLogs: createAuditLogsResource(http, paths),
    getAccessToken: () => session.getAccessToken(),

    async refreshAccessTokenWithTenant() {
      const current = session.getSession()
      if (!current?.refreshToken) {
        throw new Error('Refresh token missing. Ensure offline_access scope and sign in again.')
      }
      const tokens = await oidc.refresh(current.refreshToken)
      session.updateAccessToken(tokens)
      if (!hasTenant(tokens.access_token)) {
        throw new Error('Token still missing tid. Complete onboarding or sign in again.')
      }
      return tokens
    },
  }

  return client
}
