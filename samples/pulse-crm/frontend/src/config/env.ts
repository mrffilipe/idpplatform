// Default values mirror samples/pulse-crm/frontend/.env.example so the sample app can run without
// an .env file in local development. Real deployments must override these via the build environment.
const ENV_DEFAULTS = {
  VITE_KYVO_AUTHORITY: 'http://localhost:5000',
  VITE_KYVO_CLIENT_ID: 'pulse-crm-web',
  VITE_KYVO_REDIRECT_URI: 'http://localhost:5173/auth/callback',
  VITE_KYVO_SCOPES: 'openid profile email offline_access',
  VITE_CRM_API_URL: 'http://localhost:5100',
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

export const env = {
  kyvoAuthority: getEnvWithDefault('VITE_KYVO_AUTHORITY').replace(/\/$/, ''),
  kyvoClientId: getEnvWithDefault('VITE_KYVO_CLIENT_ID'),
  kyvoRedirectUri: getEnvWithDefault('VITE_KYVO_REDIRECT_URI').replace(/\/$/, ''),
  kyvoScopes: getEnvWithDefault('VITE_KYVO_SCOPES'),
  crmApiUrl: getEnvWithDefault('VITE_CRM_API_URL').replace(/\/$/, ''),
}
