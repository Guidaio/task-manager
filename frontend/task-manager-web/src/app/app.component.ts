import { Component, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth/auth.service';
import { AuthTokenStore } from './core/auth/auth-token.store';
import { SignalRNotificationsService } from './core/realtime/signalr-notifications.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent {
  protected readonly tokenStore = inject(AuthTokenStore);
  protected readonly realtime = inject(SignalRNotificationsService);
  private readonly auth = inject(AuthService);

  protected logout(): void {
    this.auth.logout();
  }
}
