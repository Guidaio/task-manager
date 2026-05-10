import { inject } from '@angular/core';
import { Router, type CanActivateFn } from '@angular/router';
import { AuthTokenStore } from './auth-token.store';

export const authGuard: CanActivateFn = () => {
  const store = inject(AuthTokenStore);
  const router = inject(Router);
  if (store.token()) return true;
  return router.createUrlTree(['/login'], { queryParams: { returnUrl: router.url } });
};
