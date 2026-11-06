import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { APP_SETTINGS } from '../config/app-settings';
import { LanguageService } from './language.service';

/**
 * Every API request tells the selected language, so the API returns the data in it.
 */
export const languageInterceptor: HttpInterceptorFn = (request, next) => {
  if (!request.url.startsWith(inject(APP_SETTINGS).apiUrl)) {
    return next(request);
  }

  const language = inject(LanguageService).current();

  return next(request.clone({ setHeaders: { 'Accept-Language': language } }));
};
