import { TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  GuardResult,
  Router,
  RouterStateSnapshot,
  provideRouter,
} from '@angular/router';
import { AuthService } from './auth.service';
import { permissionGuard } from './permission.guard';

describe('permissionGuard', () => {
  const auth = { hasPermission: vi.fn(), isAuthenticated: vi.fn() };
  const state = { url: '/basket' } as RouterStateSnapshot;

  function run(): GuardResult {
    return TestBed.runInInjectionContext(
      () => permissionGuard('BuyGame')({} as ActivatedRouteSnapshot, state) as GuardResult,
    );
  }

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: AuthService, useValue: auth }],
    });
  });

  it('lets a user with the permission in', () => {
    auth.hasPermission.mockReturnValue(true);

    expect(run()).toBe(true);
    expect(auth.hasPermission).toHaveBeenCalledWith('BuyGame');
  });

  it('sends a guest to the login page and remembers the target', () => {
    auth.hasPermission.mockReturnValue(false);
    auth.isAuthenticated.mockReturnValue(false);

    expect(TestBed.inject(Router).serializeUrl(run() as never)).toBe('/login?returnUrl=%2Fbasket');
  });

  it('sends a signed-in user without the permission to the main page', () => {
    auth.hasPermission.mockReturnValue(false);
    auth.isAuthenticated.mockReturnValue(true);

    expect(TestBed.inject(Router).serializeUrl(run() as never)).toBe('/');
  });
});
