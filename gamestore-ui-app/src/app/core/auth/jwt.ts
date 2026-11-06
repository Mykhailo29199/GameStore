export interface Session {
  readonly token: string;
  readonly userId: string;
  readonly userName: string;
  readonly permissions: readonly string[];
  readonly expiresAt: number;
}

interface JwtPayload {
  sub?: string;
  name?: string;
  exp?: number;
  permission?: string | string[];
}

const MILLISECONDS_IN_SECOND = 1000;

function decodeBase64Url(value: string): string {
  const base64 = value.replaceAll('-', '+').replaceAll('_', '/');
  const padded = base64.padEnd(Math.ceil(base64.length / 4) * 4, '=');
  const bytes = Uint8Array.from(atob(padded), (char) => char.codePointAt(0) ?? 0);

  return new TextDecoder().decode(bytes);
}

function toPermissionList(claim: string | string[] | undefined): string[] {
  if (claim === undefined) {
    return [];
  }

  return Array.isArray(claim) ? claim : [claim];
}

export function parseSession(token: string | null): Session | null {
  const payloadPart = token?.split('.')[1];
  if (!token || !payloadPart) {
    return null;
  }

  try {
    const payload = JSON.parse(decodeBase64Url(payloadPart)) as JwtPayload;

    return {
      token,
      userId: payload.sub ?? '',
      userName: payload.name ?? '',
      permissions: toPermissionList(payload.permission),
      expiresAt: (payload.exp ?? 0) * MILLISECONDS_IN_SECOND,
    };
  } catch {
    return null;
  }
}

export function isSessionActive(session: Session | null, now: number): session is Session {
  return session !== null && session.expiresAt > now;
}
