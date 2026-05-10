import { computed, Injectable, signal } from '@angular/core';
import type { AuthResponse } from './auth.models';
import { clearStoredAuth, readStoredAuth, writeStoredAuth } from './auth.storage';

@Injectable({ providedIn: 'root' })
export class AuthTokenStore {
  private readonly _auth = signal<AuthResponse | null>(readStoredAuth());

  readonly auth = this._auth.asReadonly();
  readonly token = computed(() => this._auth()?.token ?? null);

  setAuth(response: AuthResponse): void {
    writeStoredAuth(response);
    this._auth.set(response);
  }

  clear(): void {
    clearStoredAuth();
    this._auth.set(null);
  }
}
