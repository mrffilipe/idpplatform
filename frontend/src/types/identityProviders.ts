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
