import { HttpErrorResponse } from '@angular/common/http';
import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { AuthService } from '../../../core/auth/auth.service';
import { LoginComponent } from './login.component';

describe('LoginComponent', () => {
  const auth = { login: vi.fn(), register: vi.fn() };
  let fixture: ComponentFixture<LoginComponent>;
  let element: HTMLElement;
  let navigate: ReturnType<typeof vi.spyOn>;

  async function submit(login: string, password: string, confirmation = ''): Promise<void> {
    const inputs = element.querySelectorAll<HTMLInputElement>('input');
    inputs[0].value = login;
    inputs[1].value = password;
    inputs[2].value = confirmation;
    element.querySelector('form')?.dispatchEvent(new Event('submit'));
    await fixture.whenStable();
  }

  function errorText(): string {
    return element.querySelector('[role="alert"]')?.textContent?.trim() ?? '';
  }

  beforeEach(async () => {
    vi.clearAllMocks();
    TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [provideRouter([]), { provide: AuthService, useValue: auth }],
    });
    fixture = TestBed.createComponent(LoginComponent);
    element = fixture.nativeElement as HTMLElement;
    navigate = vi.spyOn(TestBed.inject(Router), 'navigateByUrl').mockResolvedValue(true);
    await fixture.whenStable();
  });

  it('logs in and opens the game list', async () => {
    auth.login.mockReturnValue(of(undefined));

    await submit(' admin ', 'admin');

    expect(auth.login).toHaveBeenCalledWith('admin', 'admin');
    expect(navigate).toHaveBeenCalledWith('/games');
  });

  it('returns to the page that asked for the login', async () => {
    auth.login.mockReturnValue(of(undefined));
    fixture.componentRef.setInput('returnUrl', '/basket');

    await submit('admin', 'admin');

    expect(navigate).toHaveBeenCalledWith('/basket');
  });

  it('shows the server message for wrong credentials', async () => {
    const response = new HttpErrorResponse({ status: 401, error: { Detail: 'Invalid login' } });
    auth.login.mockReturnValue(throwError(() => response));

    await submit('admin', 'wrong');

    expect(errorText()).toBe('Invalid login');
    expect(navigate).not.toHaveBeenCalled();
  });

  it('does not send an empty form', async () => {
    await submit('', '');

    expect(errorText()).toBe('Enter the login and the password.');
    expect(auth.login).not.toHaveBeenCalled();
  });

  it('registers a new user', async () => {
    auth.register.mockReturnValue(of(undefined));
    fixture.componentRef.setInput('mode', 'register');

    await submit('player', 'pass1234', 'pass1234');

    expect(auth.register).toHaveBeenCalledWith('player', 'pass1234');
    expect(navigate).toHaveBeenCalledWith('/games');
  });

  it.each([
    ['abc', 'abc', 'The password must contain at least 4 characters.'],
    ['pass1234', 'pass12345', 'The passwords do not match.'],
  ])('rejects the registration password "%s"', async (password, confirmation, message) => {
    fixture.componentRef.setInput('mode', 'register');

    await submit('player', password, confirmation);

    expect(errorText()).toBe(message);
    expect(auth.register).not.toHaveBeenCalled();
  });
});
