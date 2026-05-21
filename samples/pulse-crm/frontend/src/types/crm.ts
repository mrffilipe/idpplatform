export interface Subscription {
  id: string
  userId: string
  tenantId: string
  membershipId: string
  companyName: string
  tenantKey: string
  planCode: string
  externalCustomerId?: string | null
  paidAt: string
}

export interface MeResponse {
  userId: string
  email?: string
  tenantId?: string | null
  membershipId?: string | null
  tenantRoles: string[]
  platformRoles: string[]
  claims: Record<string, string>
  hasSubscription: boolean
  subscription: Subscription | null
}

export interface Contact {
  id: string
  tenantId: string
  name: string
  email: string
  phone?: string | null
  createdAt: string
}

export interface OnboardingCompleteResponse {
  subscription: Subscription
  requiresTokenRefresh: boolean
  message: string
}

export const PLANS = [
  { code: 'starter', name: 'Starter', price: 'R$ 49/mês', description: 'Até 100 contatos' },
  { code: 'professional', name: 'Professional', price: 'R$ 149/mês', description: 'Até 5.000 contatos' },
  { code: 'enterprise', name: 'Enterprise', price: 'R$ 399/mês', description: 'Ilimitado + suporte' },
] as const
