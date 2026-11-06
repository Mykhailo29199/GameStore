import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { APP_SETTINGS } from '../config/app-settings';
import { AuthService } from './auth.service';

const UNAUTHORIZED = 401;

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthService);
  const token = auth.token;

  if (!token || !request.url.startsWith(inject(APP_SETTINGS).apiUrl)) {
    return next(request);
  }

  const authorized = request.clone({ setHeaders: { Authorization: `Bearer ${token}` } });

  return next(authorized).pipe(
    catchError((error: unknown) => {
      if (error instanceof HttpErrorResponse && error.status === UNAUTHORIZED) {
        auth.logout();
      }

      return throwError(() => error);
    }),
  );
};
