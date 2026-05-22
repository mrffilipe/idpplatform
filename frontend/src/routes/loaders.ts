import { redirect } from 'react-router'
import { getPlatformStatus } from '../services/platformService'
import { clearAuthSession, getAuthSession } from '../utils/authStorage'
import { getSelectedTenantId } from '../utils/tenantStorage'
import type { LoaderFunctionArgs } from 'react-router'

export interface LoginLoaderData {
  requiresBootstrap: boolean
}

export async function requireAuthLoader({ request }: LoaderFunctionArgs): Promise<null> {
  const status = await getPlatformStatus()
  if (status.requiresBootstrap) {
    if (getAuthSession()?.accessToken) {
      clearAuthSession()
    }
    throw redirect('/login')
  }

  const session = getAuthSession()
  if (!session?.accessToken) {
    const url = new URL(request.url)
    const returnUrl = encodeURIComponent(url.pathname + url.search)
    throw redirect(`/login?returnUrl=${returnUrl}`)
  }

  return null
}

export async function loginLoader({ request }: LoaderFunctionArgs): Promise<LoginLoaderData> {
  const status = await getPlatformStatus()

  if (status.requiresBootstrap) {
    if (getAuthSession()?.accessToken) {
      clearAuthSession()
    }
    return { requiresBootstrap: true }
  }

  const session = getAuthSession()
  if (session?.accessToken) {
    const url = new URL(request.url)
    const returnUrl = url.searchParams.get('returnUrl') ?? '/'
    throw redirect(returnUrl)
  }

  return { requiresBootstrap: false }
}

export async function requireTenantLoader(args: LoaderFunctionArgs): Promise<null> {
  await requireAuthLoader(args)

  const tenantId = getSelectedTenantId()
  if (!tenantId) {
    throw redirect('/tenants')
  }

  return null
}
