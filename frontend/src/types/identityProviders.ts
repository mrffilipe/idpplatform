/** Aligned with `#/components/schemas/IdentityProviderType`. */
export const IdentityProviderType = {
  Local: 'Local',
  Firebase: 'Firebase',
  Cognito: 'Cognito',
  Generic: 'Generic',
} as const

export type IdentityProviderType = (typeof IdentityProviderType)[keyof typeof IdentityProviderType]

/** Authentication capabilities advertised by an identity provider on the login page. */
export const IdpCapability = {
  LocalPassword: 'LocalPassword',
  GoogleSocial: 'GoogleSocial',
  MicrosoftSocial: 'MicrosoftSocial',
  AppleSocial: 'AppleSocial',
  GenericOidc: 'GenericOidc',
} as const

export type IdpCapability = (typeof IdpCapability)[keyof typeof IdpCapability]

export interface IdentityProviderDto {
  id: string
  alias: string
  displayName: string
  providerType: IdentityProviderType
  enabled: boolean
  capabilities: IdpCapability[]
}

/** `AddIdentityProviderBody` in OpenAPI. */
export interface AddIdentityProviderBody {
  alias: string
  displayName: string
  providerType: IdentityProviderType
  capabilities: IdpCapability[]
  configJson?: string | null
}

/** `UpdateIdentityProviderBody` in OpenAPI. */
export interface UpdateIdentityProviderBody {
  displayName: string
  capabilities?: IdpCapability[] | null
  configJson?: string | null
}

/** Result returned by POST /v1.0/IdentityProviders. */
export interface AddIdentityProviderResult {
  id: string
  warnings: string[]
}

/** Firebase schema — mirrors `FirebaseProviderConfig` on the backend. */
export interface FirebaseProviderConfig {
  projectId: string
  webApiKey: string
  /** Web app auth domain; when omitted, the backend uses `{projectId}.firebaseapp.com`. */
  authDomain?: string
  serviceAccount: Record<string, unknown>
}

/** Cognito schema — registration only; login not yet available. */
export interface CognitoProviderConfig {
  userPoolId: string
  region: string
  clientId: string
}

/** Generic OIDC schema — registration only; login not yet available. */
export interface GenericProviderConfig {
  issuer: string
  jwksUri: string
  audience: string
}
