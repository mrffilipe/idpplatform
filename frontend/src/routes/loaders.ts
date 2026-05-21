import { redirect } from 'react-router'
import { getAuthSession } from '../utils/authStorage'
import { getSelectedTenantId } from '../utils/tenantStorage'
import type { LoaderFunctionArgs } from 'react-router'

export async function requireAuthLoader({ request }: LoaderFunctionArgs): Promise<null> {
  const session = getAuthSession()
  if (!session?.accessToken) {
    const url = new URL(request.url)
    const returnUrl = encodeURIComponent(url.pathname + url.search)
    throw redirect(`/login?returnUrl=${returnUrl}`)
  }

  return null
}

export async function loginLoader({ request }: LoaderFunctionArgs): Promise<null> {
  const session = getAuthSession()
  if (session?.accessToken) {
    const url = new URL(request.url)
    const returnUrl = url.searchParams.get('returnUrl') ?? '/'
    throw redirect(returnUrl)
  }

  return null
}

export async function requireTenantLoader(args: LoaderFunctionArgs): Promise<null> {
  await requireAuthLoader(args)

  const tenantId = getSelectedTenantId()
  if (!tenantId) {
    throw redirect('/tenants')
  }

  return null
}
