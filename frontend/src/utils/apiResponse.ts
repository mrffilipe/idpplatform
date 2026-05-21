import type { PagedResult } from '../types/common'

export function parseApiBody<T = unknown>(data: unknown): T {
  if (typeof data !== 'string') {
    return data as T
  }

  const trimmed = data.trim()
  if (!trimmed.startsWith('{') && !trimmed.startsWith('[')) {
    return data as T
  }

  try {
    return JSON.parse(trimmed) as T
  } catch {
    return data as T
  }
}

function asRecord(value: unknown): Record<string, unknown> {
  return value !== null && typeof value === 'object' && !Array.isArray(value)
    ? (value as Record<string, unknown>)
    : {}
}

export function unwrapPagedResult<T>(raw: unknown, mapItem: (item: unknown) => T): PagedResult<T> {
  if (Array.isArray(raw)) {
    const items = raw.map(mapItem)
    return {
      items,
      total: items.length,
      page: 1,
      pageSize: items.length > 0 ? items.length : 20,
    }
  }

  const record = asRecord(raw)
  const itemsRaw = record.items ?? record.Items
  const items = Array.isArray(itemsRaw) ? itemsRaw.map(mapItem) : []

  return {
    items,
    total: Number(record.total ?? record.Total ?? items.length),
    page: Number(record.page ?? record.Page ?? 1),
    pageSize: Number(record.pageSize ?? record.PageSize ?? 20),
  }
}

export function unwrapArray<T>(raw: unknown, mapItem: (item: unknown) => T): T[] {
  if (Array.isArray(raw)) {
    return raw.map(mapItem)
  }

  const paged = unwrapPagedResult(raw, mapItem)
  if (paged.items.length > 0) {
    return paged.items
  }

  const record = asRecord(raw)
  for (const key of ['value', 'Value', 'data', 'Data', 'sessions', 'Sessions'] as const) {
    const candidate = record[key]
    if (Array.isArray(candidate)) {
      return candidate.map(mapItem)
    }
  }

  return []
}
