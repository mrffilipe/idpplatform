/** Alinhado a `#/components/schemas/IdentityProviderType`. */
export const IdentityProviderType = {
  Local: 'Local',
  Firebase: 'Firebase',
  Cognito: 'Cognito',
  Generic: 'Generic',
} as const

export type IdentityProviderType = (typeof IdentityProviderType)[keyof typeof IdentityProviderType]

export interface IdentityProviderDto {
  id: string
  alias: string
  displayName: string
  providerType: IdentityProviderType
  enabled: boolean
}

/** `AddIdentityProviderBody` no OpenAPI. */
export interface AddIdentityProviderBody {
  alias: string
  displayName: string
  providerType: IdentityProviderType
  configJson?: string | null
}

/** `UpdateIdentityProviderBody` no OpenAPI. */
export interface UpdateIdentityProviderBody {
  displayName: string
  configJson?: string | null
}

/** Schema Firebase — espelha `FirebaseProviderConfig` no backend. */
export interface FirebaseProviderConfig {
  projectId: string
  webApiKey: string
  /** Domínio do app Web; se omitido, o backend usa `{projectId}.firebaseapp.com`. */
  authDomain?: string
  serviceAccount: Record<string, unknown>
}

/** Schema Cognito — cadastro apenas; login ainda não disponível. */
export interface CognitoProviderConfig {
  userPoolId: string
  region: string
  clientId: string
}

/** Schema genérico OIDC — cadastro apenas; login ainda não disponível. */
export interface GenericProviderConfig {
  issuer: string
  jwksUri: string
  audience: string
}
