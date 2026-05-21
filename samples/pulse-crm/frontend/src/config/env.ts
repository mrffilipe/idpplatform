function required(name: string): string {
  const value = import.meta.env[name]
  if (!value) {
    throw new Error(`Missing env: ${name}`)
  }
  return String(value)
}

export const env = {
  idpAuthority: required('VITE_IDP_AUTHORITY').replace(/\/$/, ''),
  idpClientId: required('VITE_IDP_CLIENT_ID'),
  idpRedirectUri: required('VITE_IDP_REDIRECT_URI').replace(/\/$/, ''),
  idpScopes: required('VITE_IDP_SCOPES'),
  crmApiUrl: required('VITE_CRM_API_URL').replace(/\/$/, ''),
}
