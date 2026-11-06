import { HttpErrorResponse } from '@angular/common/http';

interface ProblemBody {
  Detail?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

function firstValidationError(errors: Record<string, string[]> | undefined): string | undefined {
  return Object.values(errors ?? {}).flat()[0];
}

export function apiErrorMessage(error: unknown, fallback: string): string {
  if (!(error instanceof HttpErrorResponse) || typeof error.error !== 'object' || !error.error) {
    return fallback;
  }

  const body = error.error as ProblemBody;

  return firstValidationError(body.errors) ?? body.Detail ?? body.detail ?? fallback;
}
