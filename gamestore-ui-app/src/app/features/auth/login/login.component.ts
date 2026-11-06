import { Component, computed, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/auth/auth.service';
import { apiErrorMessage } from '../../../core/http/api-error';

export type AuthMode = 'login' | 'register';

const MIN_PASSWORD_LENGTH = 4;
const DEFAULT_RETURN_URL = '/games';

@Component({
  selector: 'app-login',
  imports: [RouterLink],
  templateUrl: './login.component.html',
  styleUrl: './login.component.css',
})
export class LoginComponent {
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  readonly mode = input<AuthMode>('login');
  readonly returnUrl = input<string>();

  protected readonly registering = computed(() => this.mode() === 'register');
  protected readonly sending = signal(false);
  protected readonly error = signal('');

  protected submit(login: string, password: string, confirmation: string): void {
    const name = login.trim();
    const problem = this.validate(name, password, confirmation);

    this.error.set(problem);
    if (problem || this.sending()) {
      return;
    }

    const request = this.registering()
      ? this.auth.register(name, password)
      : this.auth.login(name, password);

    this.sending.set(true);
    request.subscribe({
      next: () => {
        this.router.navigateByUrl(this.returnUrl() ?? DEFAULT_RETURN_URL).catch(() => false);
      },
      error: (error: unknown) => {
        this.sending.set(false);
        this.error.set(apiErrorMessage(error, 'Please check the login and the password.'));
      },
    });
  }

  private validate(name: string, password: string, confirmation: string): string {
    if (!name || !password) {
      return 'Enter the login and the password.';
    }

    if (!this.registering()) {
      return '';
    }

    if (password.length < MIN_PASSWORD_LENGTH) {
      return `The password must contain at least ${MIN_PASSWORD_LENGTH} characters.`;
    }

    return password === confirmation ? '' : 'The passwords do not match.';
  }
}
