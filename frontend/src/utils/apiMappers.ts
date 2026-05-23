import { parseApiBody } from './apiResponse'
import { ApplicationType, type Application } from '../types/applications'
import type { AuditLogItem } from '../types/auditLogs'
import type { AuthSession } from '../types/auth'
import { SessionStatus } from '../types/auth'
import type { Membership } from '../types/memberships'
import type { TenantRole } from '../types/tenantRoles'
import type { Tenant } from '../types/tenants'
import type { User, UserMembership } from '../types/users'

function asRecord(value: unknown): Record<string, unknown> {
  return value !== null && typeof value === 'object' && !Array.isArray(value)
    ? (value as Record<string, unknown>)
    : {}
}

function readString(record: Record<string, unknown>, ...keys: string[]): string {
  for (const key of keys) {
    const value = record[key]
    if (value !== undefined && value !== null) {
      return String(value)
    }
  }
  return ''
}

function readStringArray(record: Record<string, unknown>, ...keys: string[]): string[] {
  for (const key of keys) {
    const value = record[key]
    if (Array.isArray(value)) {
      return value.map(String)
    }
  }
  return []
}

function readBool(record: Record<string, unknown>, ...keys: string[]): boolean {
  for (const key of keys) {
    const value = record[key]
    if (typeof value === 'boolean') {
      return value
    }
  }
  return false
}

function readSessionStatus(record: Record<string, unknown>): AuthSession['status'] {
  const raw = record.status ?? record.Status
  if (typeof raw === 'string') {
    const normalized = raw.toLowerCase()
    if (normalized === 'active') return SessionStatus.Active
    if (normalized === 'revoked') return SessionStatus.Revoked
    if (normalized === 'expired') return SessionStatus.Expired
    if (raw in SessionStatus) {
      return raw as AuthSession['status']
    }
  }
  if (typeof raw === 'number' && Number.isFinite(raw)) {
    const legacy: AuthSession['status'][] = [SessionStatus.Active, SessionStatus.Revoked, SessionStatus.Expired]
    return legacy[raw] ?? SessionStatus.Active
  }
  return SessionStatus.Active
}

function normalizeApplicationType(value: unknown): ApplicationType {
  if (typeof value === 'string' && value in ApplicationType) {
    return value as ApplicationType
  }
  if (typeof value === 'number' && Number.isFinite(value)) {
    const legacy: ApplicationType[] = [ApplicationType.Web, ApplicationType.Mobile, ApplicationType.Backend]
    return legacy[value] ?? ApplicationType.Web
  }
  const key = String(value)
  const byName: Record<string, ApplicationType> = {
    Web: ApplicationType.Web,
    Mobile: ApplicationType.Mobile,
    Backend: ApplicationType.Backend,
  }
  return byName[key] ?? ApplicationType.Web
}

export function normalizeUserMembership(raw: unknown): UserMembership {
  const record = asRecord(raw)
  return {
    membershipId: readString(record, 'membershipId', 'MembershipId'),
    tenantId: readString(record, 'tenantId', 'TenantId'),
    tenantName: readString(record, 'tenantName', 'TenantName'),
    tenantKey: readString(record, 'tenantKey', 'TenantKey'),
    roles: readStringArray(record, 'roles', 'Roles'),
  }
}

export function normalizeUser(raw: unknown): User {
  const record = asRecord(parseApiBody(raw))
  const membershipsRaw = record.memberships ?? record.Memberships
  const memberships = Array.isArray(membershipsRaw)
    ? membershipsRaw.map(normalizeUserMembership)
    : []

  return {
    id: readString(record, 'id', 'Id'),
    email: readString(record, 'email', 'Email'),
    displayName: readString(record, 'displayName', 'DisplayName'),
    photoUrl: (record.photoUrl ?? record.PhotoUrl ?? null) as string | null | undefined,
    memberships,
  }
}

export function normalizeTenant(raw: unknown): Tenant {
  const record = asRecord(raw)
  return {
    id: readString(record, 'id', 'Id'),
    name: readString(record, 'name', 'Name'),
    key: readString(record, 'key', 'Key'),
  }
}

export function normalizeApplication(raw: unknown): Application {
  const record = asRecord(raw)
  return {
    id: readString(record, 'id', 'Id'),
    name: readString(record, 'name', 'Name'),
    slug: readString(record, 'slug', 'Slug'),
    type: normalizeApplicationType(record.type ?? record.Type),
    isSystem: readBool(record, 'isSystem', 'IsSystem'),
  }
}

export function normalizeMembership(raw: unknown): Membership {
  const record = asRecord(raw)
  return {
    id: readString(record, 'id', 'Id'),
    userId: readString(record, 'userId', 'UserId'),
    tenantId: readString(record, 'tenantId', 'TenantId'),
    roles: readStringArray(record, 'roles', 'Roles'),
    isActive: readBool(record, 'isActive', 'IsActive'),
  }
}

export function normalizeTenantRole(raw: unknown): TenantRole {
  const record = asRecord(raw)
  return {
    id: readString(record, 'id', 'Id'),
    tenantId: readString(record, 'tenantId', 'TenantId'),
    key: readString(record, 'key', 'Key'),
    name: readString(record, 'name', 'Name'),
    description: (record.description ?? record.Description ?? null) as string | null | undefined,
    isSystem: readBool(record, 'isSystem', 'IsSystem'),
    isActive: readBool(record, 'isActive', 'IsActive'),
  }
}

export function normalizeAuditLogItem(raw: unknown): AuditLogItem {
  const record = asRecord(raw)
  return {
    id: readString(record, 'id', 'Id'),
    tenantId: readString(record, 'tenantId', 'TenantId'),
    userId: (record.userId ?? record.UserId ?? null) as string | null | undefined,
    membershipId: (record.membershipId ?? record.MembershipId ?? null) as string | null | undefined,
    action: readString(record, 'action', 'Action'),
    resourceType: readString(record, 'resourceType', 'ResourceType'),
    resourceId: (record.resourceId ?? record.ResourceId ?? null) as string | null | undefined,
    ipAddress: (record.ipAddress ?? record.IpAddress ?? null) as string | null | undefined,
    userAgent: (record.userAgent ?? record.UserAgent ?? null) as string | null | undefined,
    createdAt: readString(record, 'createdAt', 'CreatedAt'),
  }
}

export function normalizeAuthSession(raw: unknown): AuthSession {
  const record = asRecord(raw)
  return {
    sessionId: readString(record, 'sessionId', 'SessionId'),
    tenantId: (record.tenantId ?? record.TenantId ?? null) as string | null | undefined,
    membershipId: (record.membershipId ?? record.MembershipId ?? null) as string | null | undefined,
    clientId: (record.clientId ?? record.ClientId ?? null) as string | null | undefined,
    status: readSessionStatus(record),
    userAgent: (record.userAgent ?? record.UserAgent ?? null) as string | null | undefined,
    ipAddress: (record.ipAddress ?? record.IpAddress ?? null) as string | null | undefined,
    expiresAt: readString(record, 'expiresAt', 'ExpiresAt'),
    lastActivityAt: readString(record, 'lastActivityAt', 'LastActivityAt'),
  }
}
