// Default values must stay in sync with backend/IdPPlatform.Domain/Constants/PlatformDefaults.cs
// (admin console client id and redirect URI) and backend/IdPPlatform.API/appsettings.Development.json
// (issuer/api base url) so the admin SPA runs without an .env file in local development.
const ENV_DEFAULTS = {
  VITE_API_BASE_URL: 'http://localhost:5000',
  VITE_API_VERSION: '1.0',
  VITE_API_TIMEOUT_MS: '30000',
  VITE_OAUTH_CLIENT_ID: 'platform-admin-web',
  VITE_OAUTH_REDIRECT_URI: 'http://localhost:3000/auth/callback',
} as const

type EnvKey = keyof typeof ENV_DEFAULTS

function getEnvWithDefault(name: EnvKey): string {
  const envValues = import.meta.env as Record<string, string | undefined>
  const value = envValues[name]
  if (value === undefined || value === '') {
    return ENV_DEFAULTS[name]
  }

  return String(value)
}

function getPositiveNumberFromEnv(name: EnvKey): number {
  const raw = getEnvWithDefault(name)
  const parsed = Number(raw)
  if (!Number.isFinite(parsed) || parsed <= 0) {
    throw new Error(`Environment variable ${name} must be a positive number. Received: ${raw}`)
  }

  return parsed
}

export const env = {
  apiBaseUrl: getEnvWithDefault('VITE_API_BASE_URL').replace(/\/$/, ''),
  apiVersion: getEnvWithDefault('VITE_API_VERSION'),
  apiTimeoutMs: getPositiveNumberFromEnv('VITE_API_TIMEOUT_MS'),
  oauthClientId: getEnvWithDefault('VITE_OAUTH_CLIENT_ID'),
  oauthRedirectUri: getEnvWithDefault('VITE_OAUTH_REDIRECT_URI').replace(/\/$/, ''),
}
