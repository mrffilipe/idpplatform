import { env } from '../config/env'

const versionPrefix = `/v${env.apiVersion}`

/** Paths versionados e well-known alinhados a `frontend/swagger.json`. */
export const apiPaths = {
  versionPrefix,
  auth: `${versionPrefix}/auth`,
  platform: `${versionPrefix}/platform`,
  users: `${versionPrefix}/Users`,
  tenants: `${versionPrefix}/Tenants`,
  memberships: `${versionPrefix}/Memberships`,
  tenantRoles: `${versionPrefix}/TenantRoles`,
  applications: `${versionPrefix}/Applications`,
  auditLogs: `${versionPrefix}/AuditLogs`,
  identityProviders: `${versionPrefix}/IdentityProviders`,
  invites: `${versionPrefix}/invites`,
  wellKnown: '/.well-known',
  /** OIDC (sem versão no path) */
  connectAuthorize: '/connect/authorize',
  connectToken: '/connect/token',
  connectLogout: '/connect/logout',
  connectUserinfo: '/connect/userinfo',
  accountLogin: '/account/login',
  accountLogout: '/account/logout',
} as const
