function getRequiredEnv(name: string): string {
  const envValues = import.meta.env as Record<string, string | undefined>
  const value = envValues[name]
  if (!value) {
    throw new Error(`Missing required environment variable: ${name}`)
  }

  return String(value)
}

function getPositiveNumberFromEnv(name: string): number {
  const value = getRequiredEnv(name)
  const parsed = Number(value)
  if (!Number.isFinite(parsed) || parsed <= 0) {
    throw new Error(`Environment variable ${name} must be a positive number. Received: ${value}`)
  }

  return parsed
}

export const env = {
  apiBaseUrl: getRequiredEnv('VITE_API_BASE_URL').replace(/\/$/, ''),
  apiVersion: getRequiredEnv('VITE_API_VERSION'),
  apiTimeoutMs: getPositiveNumberFromEnv('VITE_API_TIMEOUT_MS'),
  oauthClientId: getRequiredEnv('VITE_OAUTH_CLIENT_ID'),
  oauthRedirectUri: getRequiredEnv('VITE_OAUTH_REDIRECT_URI').replace(/\/$/, ''),
}
