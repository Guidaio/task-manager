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

  /** Notification ids treated as read (badge off, row not highlighted). */
  private readonly readIds = signal<ReadonlySet<string>>(new Set());

  readonly drawerOpen = signal(false);
  readonly items = signal<NotificationRecord[]>([]);
  readonly loading = signal(false);
  readonly loadError = signal<string | null>(null);

  readonly unreadCount = computed(() => {
    const seen = this.readIds();
    return this.items().filter((n) => !seen.has(n.id)).length;
  });

  constructor() {
    effect(() => {
      const token = this.tokenStore.token();
      if (!token) {
        this.items.set([]);
        this.drawerOpen.set(false);
        this.readIds.set(new Set());
        this.loadError.set(null);
        return;
      }
      void this.refreshFromApi();
    });
  }

  isHighlighted(n: NotificationRecord): boolean {
    return !this.readIds().has(n.id);
  }

  markRead(id: string): void {
    if (!id || this.readIds().has(id)) return;
    this.readIds.update((s) => new Set(s).add(id));
  }

  private markAllCurrentRead(): void {
    const ids = this.items().map((n) => n.id);
    if (ids.length === 0) return;
    this.readIds.update((s) => {
      const next = new Set(s);
      for (const id of ids) next.add(id);
      return next;
    });
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
    this.markAllCurrentRead();
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
