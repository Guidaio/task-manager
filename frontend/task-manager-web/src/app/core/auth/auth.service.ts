import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs/operators';
import { environment } from '../../../environments/environment';
import type { AuthResponse, LoginRequest, RegisterRequest } from './auth.models';
import { AuthTokenStore } from './auth-token.store';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly store = inject(AuthTokenStore);
  private readonly router = inject(Router);

  login(body: LoginRequest) {
    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/api/auth/login`, body)
      .pipe(tap((r) => this.store.setAuth(r)));
  }

  register(body: RegisterRequest) {
    return this.http
      .post<AuthResponse>(`${environment.apiUrl}/api/auth/register`, body)
      .pipe(tap((r) => this.store.setAuth(r)));
  }

  logout(): void {
    this.store.clear();
    void this.router.navigate(['/login']);
  }
}
