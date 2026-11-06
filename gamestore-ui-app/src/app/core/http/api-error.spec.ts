import { HttpErrorResponse } from '@angular/common/http';
import { apiErrorMessage } from './api-error';

describe('apiErrorMessage', () => {
  it('reads the detail of a problem response', () => {
    const error = new HttpErrorResponse({ status: 400, error: { Detail: 'User is banned' } });

    expect(apiErrorMessage(error, 'Failed')).toBe('User is banned');
  });

  it('prefers the first validation error', () => {
    const error = new HttpErrorResponse({
      status: 400,
      error: { detail: 'Invalid', errors: { Password: ['Password is too short.'] } },
    });

    expect(apiErrorMessage(error, 'Failed')).toBe('Password is too short.');
  });

  it.each([new Error('boom'), new HttpErrorResponse({ status: 500, error: 'text' }), null])(
    'falls back for %s',
    (error) => {
      expect(apiErrorMessage(error, 'Failed')).toBe('Failed');
    },
  );
});
