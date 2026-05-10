import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { APP_INITIALIZER, ApplicationConfig, provideZoneChangeDetection } from '@angular/core';
import { provideRouter } from '@angular/router';
import { authInterceptor } from './core/auth/auth.interceptor';
import { SignalRNotificationsService } from './core/realtime/signalr-notifications.service';
import { routes } from './app.routes';

function signalrPrimeFactory(realtime: SignalRNotificationsService) {
  return () => realtime.primeConnection();
}

export const appConfig: ApplicationConfig = {
  providers: [
    provideZoneChangeDetection({ eventCoalescing: true }),
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor])),
    {
      provide: APP_INITIALIZER,
      multi: true,
      useFactory: signalrPrimeFactory,
      deps: [SignalRNotificationsService],
    },
  ],
};
