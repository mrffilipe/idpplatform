import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react'
import type { PropsWithChildren } from 'react'
import { getAuthSession } from '../utils/authStorage'
import { getSelectedTenantId, setSelectedTenantId } from '../utils/tenantStorage'

interface TenantContextValue {
  tenantId: string | null
  selectTenant: (tenantId: string | null) => void
}

const TenantContext = createContext<TenantContextValue | undefined>(undefined)

export function TenantProvider({ children }: PropsWithChildren) {
  const [tenantId, setTenantId] = useState<string | null>(getSelectedTenantId())

  useEffect(() => {
    if (getSelectedTenantId()) {
      return
    }

    const session = getAuthSession()
    const preferred = session?.tenantId ?? session?.tenants[0]?.tenantId ?? null
    if (preferred) {
      setSelectedTenantId(preferred)
      setTenantId(preferred)
    }
  }, [])

  const selectTenant = useCallback((nextTenantId: string | null) => {
    setSelectedTenantId(nextTenantId)
    setTenantId(nextTenantId)
  }, [])

  const value = useMemo(
    () => ({
      tenantId,
      selectTenant,
    }),
    [selectTenant, tenantId],
  )

  return <TenantContext.Provider value={value}>{children}</TenantContext.Provider>
}

export function useTenant(): TenantContextValue {
  const context = useContext(TenantContext)
  if (!context) {
    throw new Error('useTenant must be used within TenantProvider')
  }
  return context
}
