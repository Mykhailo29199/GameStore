import { isSessionActive, parseSession } from './jwt';

function buildToken(payload: object): string {
  const encode = (value: object): string =>
    btoa(JSON.stringify(value)).replaceAll('+', '-').replaceAll('/', '_').replaceAll('=', '');

  return `${encode({ alg: 'HS256' })}.${encode(payload)}.signature`;
}

describe('parseSession', () => {
  it('reads the user and a list of permissions', () => {
    const token = buildToken({
      sub: 'user-1',
      name: 'admin',
      exp: 2000,
      permission: ['AddGame', 'BuyGame'],
    });

    expect(parseSession(token)).toEqual({
      token,
      userId: 'user-1',
      userName: 'admin',
      permissions: ['AddGame', 'BuyGame'],
      expiresAt: 2_000_000,
    });
  });

  it('wraps a single permission claim into a list', () => {
    const session = parseSession(buildToken({ permission: 'BuyGame' }));

    expect(session?.permissions).toEqual(['BuyGame']);
  });

  it('returns no permissions when the claim is missing', () => {
    expect(parseSession(buildToken({ name: 'guest' }))?.permissions).toEqual([]);
  });

  it.each([null, '', 'not-a-token', 'a.%%%.c'])('returns null for %s', (token) => {
    expect(parseSession(token)).toBeNull();
  });
});

describe('isSessionActive', () => {
  const session = parseSession(buildToken({ exp: 100 }));

  it('is active before the expiration time', () => {
    expect(isSessionActive(session, 99_999)).toBe(true);
  });

  it('is inactive at or after the expiration time', () => {
    expect(isSessionActive(session, 100_000)).toBe(false);
  });

  it('is inactive without a session', () => {
    expect(isSessionActive(null, 0)).toBe(false);
  });
});
