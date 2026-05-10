import { DatePipe } from '@angular/common';
import { Component, HostListener, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from './core/auth/auth.service';
import { AuthTokenStore } from './core/auth/auth-token.store';
import { NotificationCenterService } from './core/notifications/notification-center.service';
import { SignalRNotificationsService } from './core/realtime/signalr-notifications.service';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, DatePipe],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
})
export class AppComponent {
  protected readonly footerYear = new Date().getFullYear();

  protected readonly tokenStore = inject(AuthTokenStore);
  protected readonly center = inject(NotificationCenterService);
  protected readonly realtime = inject(SignalRNotificationsService);
  private readonly auth = inject(AuthService);

  @HostListener('document:keydown.escape')
  protected onEscape(): void {
    if (this.center.drawerOpen()) {
      this.center.closeDrawer();
    }
  }

  protected logout(): void {
    this.auth.logout();
  }

  protected badgeText(count: number): string {
    if (count <= 0) return '';
    if (count > 99) return '99+';
    return String(count);
  }
}
