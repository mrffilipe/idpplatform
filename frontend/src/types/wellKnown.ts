/** Chave RSA exposta em `/.well-known/jwks.json` (RS256). */
export interface JsonWebKeyRsa {
  kty: 'RSA'
  alg: string
  use: string
  kid: string
  n: string
  e: string
}

export type JsonWebKey = JsonWebKeyRsa

export interface JwksResponse {
  keys: JsonWebKey[]
}

/** Documento mínimo de `/.well-known/openid-configuration`. */
export interface OpenIdConfiguration {
  issuer: string
  authorization_endpoint: string
  token_endpoint: string
  userinfo_endpoint: string
  end_session_endpoint: string
  jwks_uri: string
  response_types_supported: string[]
  grant_types_supported: string[]
  subject_types_supported: string[]
  id_token_signing_alg_values_supported: string[]
  token_endpoint_auth_methods_supported: string[]
  code_challenge_methods_supported: string[]
  scopes_supported: string[]
}
