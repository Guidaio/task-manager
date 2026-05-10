import { HttpErrorResponse } from '@angular/common/http';
import { environment } from '../../environments/environment';

/**
 * Maps API / network errors to a user-visible string.
 */
export function apiErrorMessage(err: HttpErrorResponse, fallback: string): string {
  if (err.status === 0) {
    return `Cannot reach the API at ${environment.apiUrl}. Start the backend (see README) and try again.`;
  }
  if (typeof err.error === 'object' && err.error !== null && 'error' in err.error) {
    return String((err.error as { error?: string }).error);
  }
  if (typeof err.error === 'string' && err.error.trim()) {
    return err.error;
  }
  return err.status ? `${fallback} (HTTP ${err.status})` : fallback;
}
