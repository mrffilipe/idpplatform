export function isJwtFormat(token: string): boolean {
  return token.split('.').length === 3
}

export function parseJwtPayload<T extends Record<string, unknown>>(token: string): T {
  if (!isJwtFormat(token)) {
    throw new Error('JWT inválido.')
  }

  const base64 = token.split('.')[1].replace(/-/g, '+').replace(/_/g, '/')
  const json = decodeURIComponent(
    atob(base64)
      .split('')
      .map((char) => `%${`00${char.charCodeAt(0).toString(16)}`.slice(-2)}`)
      .join(''),
  )

  return JSON.parse(json) as T
}

export function tryParseJwtPayload<T extends Record<string, unknown>>(token: string | undefined): T | null {
  if (!token || !isJwtFormat(token)) {
    return null
  }

  try {
    return parseJwtPayload<T>(token)
  } catch {
    return null
  }
}
