import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NotificationCenterService } from '../../core/notifications/notification-center.service';
import type { TaskDto } from '../../core/tasks/task.models';
import { TasksService } from '../../core/tasks/tasks.service';

const FLASH_KEY = 'taskManager.flash';

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

  constructor() {
    this.consumeFlashFromStorage();
    this.reload();
  }

  private consumeFlashFromStorage(): void {
    try {
      const msg = globalThis.sessionStorage?.getItem(FLASH_KEY);
      if (msg) {
        this.flash.set(msg);
        globalThis.sessionStorage?.removeItem(FLASH_KEY);
        globalThis.setTimeout(() => this.flash.set(null), 5000);
      }
    } catch {
      /* ignore */
    }
  }

  protected reload(): void {
    this.loading.set(true);
    this.error.set(null);
    this.tasksService.list().subscribe({
      next: (tasks) => {
        this.tasks.set(tasks);
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
