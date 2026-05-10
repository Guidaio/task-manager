import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NotificationCenterService } from '../../core/notifications/notification-center.service';
import type { TaskDto, TaskItemStatus } from '../../core/tasks/task.models';
import { TasksService } from '../../core/tasks/tasks.service';

const FLASH_KEY = 'taskManager.flash';
const FLASH_BANNER_KEY = 'taskManager.flashBanner';

function apiMessage(err: HttpErrorResponse, fallback: string): string {
  const body = err.error;
  if (body && typeof body === 'object' && 'error' in body) {
    return String((body as { error?: string }).error);
  }
  return fallback;
}

@Component({
  selector: 'app-task-list',
  imports: [RouterLink],
  templateUrl: './task-list.component.html',
  styleUrl: './task-list.component.scss',
})
export class TaskListComponent {
  private readonly tasksService = inject(TasksService);
  private readonly notificationCenter = inject(NotificationCenterService);

  protected readonly tasks = signal<TaskDto[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly flash = signal<string | null>(null);
  protected readonly flashKind = signal<'success' | 'error'>('success');
  protected readonly totalCount = signal(0);
  protected readonly statusFilter = signal<TaskItemStatus | ''>('');
  protected readonly page = signal(1);
  protected readonly pageSize = signal(25);

  protected readonly totalPages = computed(() => {
    const tc = this.totalCount();
    const ps = this.pageSize();
    return Math.max(1, Math.ceil(tc / ps));
  });

  protected readonly statusOptions: { value: TaskItemStatus | ''; label: string }[] = [
    { value: '', label: 'All statuses' },
    { value: 'Pending', label: 'Pending' },
    { value: 'InProgress', label: 'In progress' },
    { value: 'Completed', label: 'Completed' },
    { value: 'Cancelled', label: 'Cancelled' },
  ];

  protected readonly pageSizeOptions = [10, 25, 50, 100] as const;

  protected readonly emptyMessage = computed(() => {
    if (this.page() > 1 && this.tasks().length === 0) return 'No tasks on this page.';
    if (this.statusFilter() !== '') return 'No tasks match this filter.';
    return 'No tasks yet. Create one to get started.';
  });

  constructor() {
    this.consumeFlashFromStorage();
    this.reload();
  }

  private consumeFlashFromStorage(): void {
    try {
      const msg = globalThis.sessionStorage?.getItem(FLASH_KEY);
      if (msg) {
        const kind = globalThis.sessionStorage?.getItem(FLASH_BANNER_KEY);
        this.flashKind.set(kind === 'error' ? 'error' : 'success');
        this.flash.set(msg);
        globalThis.sessionStorage?.removeItem(FLASH_KEY);
        globalThis.sessionStorage?.removeItem(FLASH_BANNER_KEY);
        globalThis.setTimeout(() => {
          this.flash.set(null);
          this.flashKind.set('success');
        }, 5000);
      }
    } catch {
      /* ignore */
    }
  }

  protected onStatusChange(event: Event): void {
    const v = (event.target as HTMLSelectElement).value as TaskItemStatus | '';
    this.statusFilter.set(v);
    this.page.set(1);
    this.reload();
  }

  protected onPageSizeChange(event: Event): void {
    const v = Number((event.target as HTMLSelectElement).value);
    this.pageSize.set(Number.isFinite(v) ? v : 25);
    this.page.set(1);
    this.reload();
  }

  protected prevPage(): void {
    if (this.page() <= 1) return;
    this.page.update((p) => p - 1);
    this.reload();
  }

  protected nextPage(): void {
    if (this.page() >= this.totalPages()) return;
    this.page.update((p) => p + 1);
    this.reload();
  }

  protected reload(): void {
    this.loading.set(true);
    this.error.set(null);
    const st = this.statusFilter();
    this.tasksService
      .list({
        status: st || undefined,
        page: this.page(),
        pageSize: this.pageSize(),
      })
      .subscribe({
        next: (res) => {
          this.tasks.set(res.items);
          this.totalCount.set(res.totalCount);
          this.loading.set(false);
        },
        error: (err: HttpErrorResponse) => {
          this.loading.set(false);
          this.error.set(apiMessage(err, 'Could not load tasks.'));
        },
      });
  }

  protected statusLabel(s: string): string {
    return s.replace(/([A-Z])/g, ' $1').trim();
  }

  protected formatDate(iso: string | null): string {
    if (!iso) return '—';
    return new Date(iso).toLocaleString(undefined, {
      dateStyle: 'medium',
      timeStyle: 'short',
    });
  }

  protected async deleteTask(task: TaskDto): Promise<void> {
    const ok = globalThis.confirm(`Delete “${task.title}”?`);
    if (!ok) return;
    this.tasksService.delete(task.id).subscribe({
      next: () => {
        void this.notificationCenter.refreshFromApi();
        this.reload();
      },
      error: (err: HttpErrorResponse) => {
        this.error.set(apiMessage(err, 'Delete failed.'));
      },
    });
  }
}
