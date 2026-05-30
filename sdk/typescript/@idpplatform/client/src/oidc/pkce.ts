function base64UrlEncode(buffer: ArrayBuffer): string {
  const bytes = new Uint8Array(buffer)
  let binary = ''
  for (const b of bytes) binary += String.fromCharCode(b)
  return btoa(binary).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '')
}

export async function generatePkcePair(): Promise<{ codeVerifier: string; codeChallenge: string }> {
  const array = new Uint8Array(32)
  crypto.getRandomValues(array)
  const codeVerifier = base64UrlEncode(array.buffer)
  const digest = await crypto.subtle.digest('SHA-256', new TextEncoder().encode(codeVerifier))
  const codeChallenge = base64UrlEncode(digest)
  return { codeVerifier, codeChallenge }
}
