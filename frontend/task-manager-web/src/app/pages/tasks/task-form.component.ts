import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, NavigationEnd, Router, RouterLink } from '@angular/router';
import { filter, finalize } from 'rxjs/operators';
import { NotificationCenterService } from '../../core/notifications/notification-center.service';
import type { TaskItemStatus } from '../../core/tasks/task.models';
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

function toDatetimeLocalValue(iso: string | null): string {
  if (!iso) return '';
  const d = new Date(iso);
  const pad = (n: number) => n.toString().padStart(2, '0');
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(
    d.getMinutes(),
  )}`;
}

function fromDatetimeLocalValue(v: string): string | null {
  const t = v.trim();
  if (!t) return null;
  const d = new Date(t);
  return Number.isNaN(d.getTime()) ? null : d.toISOString();
}

@Component({
  selector: 'app-task-form',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './task-form.component.html',
  styleUrl: './task-form.component.scss',
})
export class TaskFormComponent {
  private readonly fb = inject(FormBuilder);
  private readonly tasksService = inject(TasksService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly destroyRef = inject(DestroyRef);
  private readonly notificationCenter = inject(NotificationCenterService);

  protected readonly statuses: TaskItemStatus[] = [
    'Pending',
    'InProgress',
    'Completed',
    'Cancelled',
  ];

  protected readonly loading = signal(false);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly isCreate = signal(true);
  private taskId: string | null = null;

  protected readonly form = this.fb.nonNullable.group({
    title: ['', [Validators.required, Validators.maxLength(500)]],
    description: ['', [Validators.maxLength(8000)]],
    status: this.fb.nonNullable.control<TaskItemStatus>('Pending'),
    dueLocal: [''],
  });

  constructor() {
    this.syncRoute();
    this.router.events
      .pipe(
        filter((e): e is NavigationEnd => e instanceof NavigationEnd),
        takeUntilDestroyed(this.destroyRef),
      )
      .subscribe(() => this.syncRoute());
  }

  private syncRoute(): void {
    const path = this.route.snapshot.routeConfig?.path;
    if (path === 'new') {
      this.isCreate.set(true);
      this.taskId = null;
      this.form.reset({
        title: '',
        description: '',
        status: 'Pending',
        dueLocal: '',
      });
      this.error.set(null);
      this.loading.set(false);
      return;
    }
    const id = this.route.snapshot.paramMap.get('id');
    if (id && path === ':id') {
      this.isCreate.set(false);
      this.taskId = id;
      this.loadTask(id);
    }
  }

  private loadTask(id: string): void {
    this.loading.set(true);
    this.error.set(null);
    this.tasksService.getById(id).subscribe({
      next: (task) => {
        this.form.patchValue({
          title: task.title,
          description: task.description ?? '',
          status: task.status,
          dueLocal: toDatetimeLocalValue(task.dueDateUtc),
        });
        this.loading.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.loading.set(false);
        if (err.status === 404) {
          try {
            globalThis.sessionStorage?.setItem(FLASH_KEY, apiMessage(err, 'Task was not found.'));
            globalThis.sessionStorage?.setItem(FLASH_BANNER_KEY, 'error');
          } catch {
            /* ignore private mode */
          }
          void this.router.navigate(['/tasks']);
          return;
        }
        this.error.set(apiMessage(err, 'Could not load task.'));
      },
    });
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }
    const raw = this.form.getRawValue();
    const due = fromDatetimeLocalValue(raw.dueLocal);
    this.saving.set(true);
    this.error.set(null);

    if (this.isCreate()) {
      this.tasksService
        .create({
          title: raw.title,
          description: raw.description.trim() === '' ? null : raw.description,
          status: raw.status,
          dueDateUtc: due,
        })
        .pipe(finalize(() => this.saving.set(false)))
        .subscribe({
          next: () => {
            try {
              globalThis.sessionStorage?.setItem(FLASH_KEY, 'Task created.');
            } catch {
              /* ignore private mode */
            }
            void this.notificationCenter.refreshFromApi();
            void this.router.navigate(['/tasks']);
          },
          error: (err: HttpErrorResponse) => this.error.set(apiMessage(err, 'Save failed.')),
        });
    } else if (this.taskId) {
      this.tasksService
        .update(this.taskId, {
          title: raw.title,
          description: raw.description.trim() === '' ? null : raw.description,
          status: raw.status,
          dueDateUtc: due,
        })
        .pipe(finalize(() => this.saving.set(false)))
        .subscribe({
          next: () => {
            try {
              globalThis.sessionStorage?.setItem(FLASH_KEY, 'Task updated.');
            } catch {
              /* ignore */
            }
            void this.notificationCenter.refreshFromApi();
            void this.router.navigate(['/tasks']);
          },
          error: (err: HttpErrorResponse) => this.error.set(apiMessage(err, 'Save failed.')),
        });
    }
  }

  protected deleteTask(): void {
    if (!this.taskId) return;
    const title = this.form.controls.title.value;
    const ok = globalThis.confirm(`Delete “${title}”?`);
    if (!ok) return;
    this.tasksService.delete(this.taskId).subscribe({
      next: () => {
        void this.notificationCenter.refreshFromApi();
        void this.router.navigate(['/tasks']);
      },
      error: (err: HttpErrorResponse) => this.error.set(apiMessage(err, 'Delete failed.')),
    });
  }
}
