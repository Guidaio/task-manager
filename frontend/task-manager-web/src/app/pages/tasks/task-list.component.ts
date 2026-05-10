import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import type { TaskDto } from '../../core/tasks/task.models';
import { TasksService } from '../../core/tasks/tasks.service';

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

  protected readonly tasks = signal<TaskDto[]>([]);
  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);

  constructor() {
    this.reload();
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
      next: () => this.reload(),
      error: (err: HttpErrorResponse) => {
        this.error.set(apiMessage(err, 'Delete failed.'));
      },
    });
  }
}
