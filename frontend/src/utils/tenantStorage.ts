const TENANT_STORAGE_KEY = 'idp.tenant.selected'

function isBrowser(): boolean {
  return typeof window !== 'undefined'
}

export function getSelectedTenantId(): string | null {
  if (!isBrowser()) {
    return null
  }
  return localStorage.getItem(TENANT_STORAGE_KEY)
}

export function setSelectedTenantId(tenantId: string | null): void {
  if (!isBrowser()) {
    return
  }

  if (!tenantId) {
    localStorage.removeItem(TENANT_STORAGE_KEY)
    return
  }

  localStorage.setItem(TENANT_STORAGE_KEY, tenantId)
}
