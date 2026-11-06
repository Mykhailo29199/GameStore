import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, map, tap } from 'rxjs';
import { APP_SETTINGS } from '../config/app-settings';
import { Session, isSessionActive, parseSession } from './jwt';

interface TokenResponse {
  token: string;
}

const TOKEN_STORAGE_KEY = 'gamestore.token';

function readStoredSession(): Session | null {
  const session = parseSession(localStorage.getItem(TOKEN_STORAGE_KEY));

  return isSessionActive(session, Date.now()) ? session : null;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly usersUrl = `${inject(APP_SETTINGS).apiUrl}/users`;
  private readonly currentSession = signal<Session | null>(readStoredSession());

  readonly session = this.currentSession.asReadonly();
  readonly isAuthenticated = computed(() => this.currentSession() !== null);
  readonly userName = computed(() => this.currentSession()?.userName ?? '');

  get token(): string | null {
    const session = this.currentSession();

    return isSessionActive(session, Date.now()) ? session.token : null;
  }

  hasPermission(permission: string): boolean {
    return this.currentSession()?.permissions.includes(permission) ?? false;
  }

  login(login: string, password: string): Observable<void> {
    const body = { model: { login, password, internalAuth: true } };

    return this.startSession(this.http.post<TokenResponse>(`${this.usersUrl}/login`, body));
  }

  register(login: string, password: string): Observable<void> {
    const body = { login, password };

    return this.startSession(this.http.post<TokenResponse>(`${this.usersUrl}/register`, body));
  }

  logout(): void {
    localStorage.removeItem(TOKEN_STORAGE_KEY);
    this.currentSession.set(null);
  }

  private startSession(request: Observable<TokenResponse>): Observable<void> {
    return request.pipe(
      tap(({ token }) => {
        localStorage.setItem(TOKEN_STORAGE_KEY, token);
        this.currentSession.set(parseSession(token));
      }),
      map(() => undefined),
    );
  }
}
