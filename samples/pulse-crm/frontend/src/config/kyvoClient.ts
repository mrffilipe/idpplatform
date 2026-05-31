import { createKyvoClient } from '@kyvo/client'
import { env } from './env'

export const kyvoClient = createKyvoClient({
  authority: env.kyvoAuthority,
  apiVersion: '1.0',
  oidc: {
    clientId: env.kyvoClientId,
    redirectUri: env.kyvoRedirectUri,
    scopes: env.kyvoScopes,
  },
})
