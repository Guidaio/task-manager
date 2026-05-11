import { HttpErrorResponse } from '@angular/common/http';
import { Component, computed, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject, switchMap } from 'rxjs';
import { NotificationCenterService } from '../../core/notifications/notification-center.service';
import type { TaskDto, TaskItemStatus } from '../../core/tasks/task.models';
import { TasksService } from '../../core/tasks/tasks.service';

const FLASH_KEY = 'taskManager.flash';
const FLASH_BANNER_KEY = 'taskManager.flashBanner';

type TaskSortKey = 'created' | 'title' | 'status' | 'due';

function apiMessage(err: HttpErrorResponse, fallback: string): string {
  const body = err.error;
  if (body && typeof body === 'object' && 'error' in body) {
    return String((body as { error?: string }).error);
  }
  return fallback;
}

@Component({
  selector: 'app-task-list',
  imports: [RouterLink, FormsModule],
  templateUrl: './task-list.component.html',
  styleUrl: './task-list.component.scss',
})
export class TaskListComponent {
  private readonly tasksService = inject(TasksService);
  private readonly notificationCenter = inject(NotificationCenterService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly load$ = new Subject<void>();
  private searchDebounceId: ReturnType<typeof setTimeout> | undefined;

  protected readonly tasks = signal<TaskDto[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly flash = signal<string | null>(null);
  protected readonly flashKind = signal<'success' | 'error'>('success');
  protected readonly totalCount = signal(0);
  protected readonly statusFilter = signal<TaskItemStatus | ''>('');
  protected readonly page = signal(1);
  protected readonly pageSize = signal(25);
  /** Default: newest first (created desc), matching API defaults. */
  protected readonly sortColumn = signal<TaskSortKey>('created');
  protected readonly sortDescending = signal(true);

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

  /** Plaintext in the search box (updates immediately). */
  protected readonly searchText = signal('');
  /** Trimmed value sent to the API after debounce. */
  protected readonly searchQuery = signal('');

  protected readonly emptyMessage = computed(() => {
    if (this.page() > 1 && this.tasks().length === 0) return 'No tasks on this page.';
    if (this.statusFilter() !== '') return 'No tasks match this filter.';
    if (this.searchQuery() !== '') return 'No tasks match your search.';
    return 'No tasks yet. Create one to get started.';
  });

  constructor() {
    this.destroyRef.onDestroy(() => {
      if (this.searchDebounceId !== undefined) {
        clearTimeout(this.searchDebounceId);
      }
    });

    this.load$
      .pipe(
        switchMap(() => {
          this.loading.set(true);
          this.error.set(null);
          const st = this.statusFilter();
          return this.tasksService.list({
            status: st || undefined,
            page: this.page(),
            pageSize: this.pageSize(),
            sort: this.sortColumn(),
            order: this.sortDescending() ? 'desc' : 'asc',
            search: this.searchQuery() ? this.searchQuery() : undefined,
          });
        }),
        takeUntilDestroyed(),
      )
      .subscribe({
        next: (res) => {
          this.tasks.set(res.items);
          this.totalCount.set(res.totalCount);
          this.pageSize.set(res.pageSize);
          this.loading.set(false);
          this.error.set(null);
        },
        error: (err: unknown) => {
          this.loading.set(false);
          if (err instanceof HttpErrorResponse) {
            this.error.set(apiMessage(err, 'Could not load tasks.'));
          } else {
            this.error.set('Could not load tasks.');
          }
        },
      });

    this.consumeFlashFromStorage();
    this.load$.next();
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

  protected onSearchInput(event: Event): void {
    const v = (event.target as HTMLInputElement).value;
    this.searchText.set(v);
    if (this.searchDebounceId !== undefined) {
      clearTimeout(this.searchDebounceId);
    }
    this.searchDebounceId = setTimeout(() => {
      this.searchDebounceId = undefined;
      this.searchQuery.set(v.trim());
      this.page.set(1);
      this.reload();
    }, 320);
  }

  protected onSortClick(column: 'title' | 'status' | 'due'): void {
    if (this.sortColumn() === column) {
      this.sortDescending.update((d) => !d);
    } else {
      this.sortColumn.set(column);
      this.sortDescending.set(false);
    }
    this.page.set(1);
    this.reload();
  }

  protected sortArrow(column: TaskSortKey): string {
    if (this.sortColumn() !== column) return '';
    return this.sortDescending() ? ' \u2193' : ' \u2191';
  }

  /** Binds reliably to `<select>`; `[value]` alone can desync the visible option from `pageSize()`. */
  protected onPageSizeNgModelChange(raw: string | number): void {
    const v = typeof raw === 'string' ? Number.parseInt(raw, 10) : raw;
    this.pageSize.set(Number.isFinite(v) && v > 0 ? v : 25);
    this.page.set(1);
    this.load$.next();
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
    this.load$.next();
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
