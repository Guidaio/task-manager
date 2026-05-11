import { HttpClient, HttpParams } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import type { CreateTaskRequest, PagedTasksResponse, TaskDto, UpdateTaskRequest } from './task.models';

@Injectable({ providedIn: 'root' })
export class TasksService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/tasks`;

  list(options?: {
    status?: string;
    page?: number;
    pageSize?: number;
    sort?: 'created' | 'title' | 'status' | 'due';
    order?: 'asc' | 'desc';
    search?: string;
  }) {
    let params = new HttpParams();
    if (options?.status) params = params.set('status', options.status);
    if (options?.page != null) params = params.set('page', String(options.page));
    if (options?.pageSize != null) params = params.set('pageSize', String(options.pageSize));
    if (options?.sort) params = params.set('sort', options.sort);
    if (options?.order) params = params.set('order', options.order);
    if (options?.search) params = params.set('search', options.search);
    return this.http.get<PagedTasksResponse>(this.base, { params });
  }

  getById(id: string) {
    return this.http.get<TaskDto>(`${this.base}/${id}`);
  }

  create(body: CreateTaskRequest) {
    return this.http.post<TaskDto>(this.base, body);
  }

  update(id: string, body: UpdateTaskRequest) {
    return this.http.put<TaskDto>(`${this.base}/${id}`, body);
  }

  delete(id: string) {
    return this.http.delete<void>(`${this.base}/${id}`);
  }
}
