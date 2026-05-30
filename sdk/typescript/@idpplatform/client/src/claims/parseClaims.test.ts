import { describe, it } from 'node:test'
import assert from 'node:assert/strict'
import { hasTenant, parseAccessTokenClaims, requiresOnboarding } from './parseClaims.js'

function fakeJwt(payload: Record<string, unknown>): string {
  const header = btoa(JSON.stringify({ alg: 'none' }))
  const body = btoa(JSON.stringify(payload)).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '')
  return `${header}.${body}.sig`
}

describe('parseAccessTokenClaims', () => {
  it('reads tid and roles', () => {
    const token = fakeJwt({ sub: 'u1', tid: 't1', trole: ['owner', 'admin'] })
    const claims = parseAccessTokenClaims(token)
    assert.equal(claims.sub, 'u1')
    assert.equal(claims.tid, 't1')
    assert.deepEqual(claims.trole, ['owner', 'admin'])
  })

  it('hasTenant and requiresOnboarding', () => {
    const withTenant = fakeJwt({ tid: 'abc' })
    const without = fakeJwt({ sub: 'x' })
    assert.equal(hasTenant(withTenant), true)
    assert.equal(requiresOnboarding(without), true)
  })
})
