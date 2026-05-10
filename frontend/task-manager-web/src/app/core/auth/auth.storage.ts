import type { AuthResponse } from './auth.models';

export const AUTH_STORAGE_KEY = 'tm.auth';

export function readStoredAuth(): AuthResponse | null {
  try {
    const raw = localStorage.getItem(AUTH_STORAGE_KEY);
    if (!raw) return null;
    const parsed = JSON.parse(raw) as AuthResponse;
    if (!parsed?.token || !parsed.userId) return null;
    return parsed;
  } catch {
    return null;
  }
}

export function writeStoredAuth(auth: AuthResponse): void {
  localStorage.setItem(AUTH_STORAGE_KEY, JSON.stringify(auth));
}

export function clearStoredAuth(): void {
  localStorage.removeItem(AUTH_STORAGE_KEY);
}
