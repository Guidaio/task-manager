import { Injectable, effect, inject, signal } from '@angular/core';
import { HubConnection, HubConnectionBuilder, HubConnectionState } from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { AuthTokenStore } from '../auth/auth-token.store';

export interface ToastMessage {
  id: string;
  message: string;
  type: 'info' | 'success' | 'warning';
}

export interface NotificationPayload {
  id: string;
  taskId: string | null;
  message: string;
  type: string;
  isRead: boolean;
  createdAtUtc: string;
}

@Injectable({ providedIn: 'root' })
export class SignalRNotificationsService {
  private readonly tokenStore = inject(AuthTokenStore);
  private hub: HubConnection | null = null;
  private starting: Promise<void> | null = null;

  readonly toasts = signal<ToastMessage[]>([]);

  constructor() {
    effect(() => {
      const token = this.tokenStore.token();
      if (token) {
        void this.ensureConnected();
      } else {
        void this.disconnect();
      }
    });
  }

  dismiss(id: string): void {
    this.toasts.update((list) => list.filter((t) => t.id !== id));
  }

  private normalizeType(raw: string): ToastMessage['type'] {
    const s = raw?.toLowerCase();
    if (s === 'success') return 'success';
    if (s === 'warning') return 'warning';
    return 'info';
  }

  private addToast(payload: NotificationPayload): void {
    const toast: ToastMessage = {
      id: payload.id,
      message: payload.message,
      type: this.normalizeType(payload.type),
    };
    this.toasts.update((list) => [toast, ...list].slice(0, 5));
    globalThis.setTimeout(() => this.dismiss(toast.id), 6500);
  }

  private async ensureConnected(): Promise<void> {
    const token = this.tokenStore.token();
    if (!token) return;

    if (this.hub?.state === HubConnectionState.Connected) return;

    if (this.starting) {
      await this.starting;
      return;
    }

    this.starting = this.startInternal();
    try {
      await this.starting;
    } finally {
      this.starting = null;
    }
  }

  private async startInternal(): Promise<void> {
    await this.disconnect();

    const hub = new HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/hubs/notifications`, {
        accessTokenFactory: () => this.tokenStore.token() ?? '',
      })
      .withAutomaticReconnect()
      .build();

    hub.on('notification', (payload: NotificationPayload) => {
      this.addToast(payload);
    });

    this.hub = hub;
    try {
      await hub.start();
    } catch (e) {
      console.error('SignalR connection failed', e);
      this.hub = null;
    }
  }

  private async disconnect(): Promise<void> {
    if (!this.hub) return;
    try {
      await this.hub.stop();
    } catch {
      /* ignore */
    }
    this.hub = null;
  }
}
