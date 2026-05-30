import { createIdPClient } from '@idpplatform/client'
import { env } from './env'

export const idpClient = createIdPClient({
  authority: env.idpAuthority,
  apiVersion: '1.0',
  oidc: {
    clientId: env.idpClientId,
    redirectUri: env.idpRedirectUri,
    scopes: env.idpScopes,
  },
})
