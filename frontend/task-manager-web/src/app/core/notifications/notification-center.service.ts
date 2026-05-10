import { HttpClient } from '@angular/common/http';
import { computed, effect, inject, Injectable, signal } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthTokenStore } from '../auth/auth-token.store';
import type { NotificationPayload, NotificationRecord } from './notification.models';

const MAX_PANEL_ITEMS = 10;

@Injectable({ providedIn: 'root' })
export class NotificationCenterService {
  private readonly http = inject(HttpClient);
  private readonly tokenStore = inject(AuthTokenStore);

  readonly drawerOpen = signal(false);
  readonly items = signal<NotificationRecord[]>([]);
  readonly loading = signal(false);
  readonly loadError = signal<string | null>(null);

  readonly unreadCount = computed(() => this.items().filter((n) => !n.isRead).length);

  constructor() {
    effect(() => {
      const token = this.tokenStore.token();
      if (!token) {
        this.items.set([]);
        this.drawerOpen.set(false);
        this.loadError.set(null);
        return;
      }
      void this.refreshFromApi();
    });
  }

  isHighlighted(n: NotificationRecord): boolean {
    return !n.isRead;
  }

  markRead(id: string): void {
    void this.markReadAsync(id);
  }

  private async markReadAsync(id: string): Promise<void> {
    if (!id) return;
    const row = this.items().find((x) => x.id === id);
    if (!row || row.isRead) return;

    const previous = this.items();
    this.items.update((list) => list.map((x) => (x.id === id ? { ...x, isRead: true } : x)));

    try {
      await firstValueFrom(
        this.http.post(`${environment.apiUrl}/api/notifications/mark-read`, { ids: [id] }),
      );
    } catch {
      this.items.set(previous);
    }
  }

  toggleDrawer(): void {
    if (this.drawerOpen()) {
      this.closeDrawer();
    } else {
      void this.openDrawer();
    }
  }

  async openDrawer(): Promise<void> {
    this.drawerOpen.set(true);
    this.loadError.set(null);
    await this.refreshFromApi();
  }

  closeDrawer(): void {
    this.drawerOpen.set(false);
    void this.markAllCurrentReadAsync();
  }

  private async markAllCurrentReadAsync(): Promise<void> {
    const unreadIds = this.items().filter((n) => !n.isRead).map((n) => n.id);
    if (unreadIds.length === 0) return;

    const previous = this.items();
    this.items.update((list) => list.map((x) => (!x.isRead ? { ...x, isRead: true } : x)));

    try {
      await firstValueFrom(
        this.http.post(`${environment.apiUrl}/api/notifications/mark-read`, { ids: unreadIds }),
      );
    } catch {
      this.items.set(previous);
    }
  }

  async refreshFromApi(): Promise<void> {
    if (!this.tokenStore.token()) return;

    this.loading.set(true);
    this.loadError.set(null);
    try {
      const rows = await firstValueFrom(
        this.http.get<NotificationRecord[]>(`${environment.apiUrl}/api/notifications`),
      );
      const list = (rows ?? []).slice(0, MAX_PANEL_ITEMS);
      this.items.set(list);
    } catch {
      this.loadError.set('Could not load notifications.');
    } finally {
      this.loading.set(false);
    }
  }

  applyRealtimePayload(payload: NotificationPayload): void {
    const row: NotificationRecord = {
      id: payload.id,
      taskId: payload.taskId,
      message: payload.message,
      type: payload.type,
      isRead: payload.isRead,
      createdAtUtc: payload.createdAtUtc,
    };
    this.items.update((list) => {
      const merged = [row, ...list.filter((x) => x.id !== row.id)];
      merged.sort(
        (a, b) => new Date(b.createdAtUtc).getTime() - new Date(a.createdAtUtc).getTime(),
      );
      return merged.slice(0, MAX_PANEL_ITEMS);
    });
  }
}
