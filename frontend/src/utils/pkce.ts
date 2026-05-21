function toBase64Url(bytes: Uint8Array): string {
  const binary = String.fromCharCode(...bytes)
  return btoa(binary).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/g, '')
}

function randomString(length: number): string {
  const bytes = new Uint8Array(length)
  crypto.getRandomValues(bytes)
  return toBase64Url(bytes).slice(0, length)
}

async function sha256(input: string): Promise<Uint8Array> {
  const encoded = new TextEncoder().encode(input)
  const hash = await crypto.subtle.digest('SHA-256', encoded)
  return new Uint8Array(hash)
}

export async function generatePkcePair(): Promise<{ codeVerifier: string; codeChallenge: string }> {
  const codeVerifier = randomString(64)
  const hash = await sha256(codeVerifier)
  const codeChallenge = toBase64Url(hash)
  return { codeVerifier, codeChallenge }
}
