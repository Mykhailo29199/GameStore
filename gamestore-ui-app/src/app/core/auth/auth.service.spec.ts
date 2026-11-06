import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { HttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from './auth.service';

function buildToken(payload: object): string {
  return `header.${btoa(JSON.stringify(payload))}.signature`;
}

const FAR_FUTURE = 4_102_444_800;

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(withInterceptors([authInterceptor])), provideHttpClientTesting()],
    });
    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('starts without a session', () => {
    expect(service.isAuthenticated()).toBe(false);
    expect(service.token).toBeNull();
  });

  it('logs in with the internal auth request shape and stores the session', () => {
    const token = buildToken({ name: 'admin', exp: FAR_FUTURE, permission: ['ManageUsers'] });

    service.login('admin', 'secret').subscribe();
    const request = httpMock.expectOne('/api/users/login');
    request.flush({ token });

    expect(request.request.body).toEqual({
      model: { login: 'admin', password: 'secret', internalAuth: true },
    });
    expect(service.userName()).toBe('admin');
    expect(service.hasPermission('ManageUsers')).toBe(true);
    expect(service.hasPermission('ManageRoles')).toBe(false);
  });

  it('registers and signs the new user in', () => {
    service.register('player', 'secret').subscribe();
    const request = httpMock.expectOne('/api/users/register');
    request.flush({ token: buildToken({ name: 'player', exp: FAR_FUTURE }) });

    expect(request.request.body).toEqual({ login: 'player', password: 'secret' });
    expect(service.isAuthenticated()).toBe(true);
  });

  it('sends the token with API requests and drops the session on 401', () => {
    service.login('admin', 'secret').subscribe();
    const token = buildToken({ name: 'admin', exp: FAR_FUTURE });
    httpMock.expectOne('/api/users/login').flush({ token });

    TestBed.inject(HttpClient).get('/api/orders/cart').subscribe({ error: () => undefined });
    const request = httpMock.expectOne('/api/orders/cart');
    request.flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(request.request.headers.get('Authorization')).toBe(`Bearer ${token}`);
    expect(service.isAuthenticated()).toBe(false);
  });

  it('clears the stored token on logout', () => {
    service.login('admin', 'secret').subscribe();
    httpMock.expectOne('/api/users/login').flush({ token: buildToken({ exp: FAR_FUTURE }) });

    service.logout();

    expect(service.isAuthenticated()).toBe(false);
    expect(localStorage.length).toBe(0);
  });
});
