import { HttpClient } from '@angular/common/http';
import { inject, Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';
import type { CreateTaskRequest, TaskDto, UpdateTaskRequest } from './task.models';

@Injectable({ providedIn: 'root' })
export class TasksService {
  private readonly http = inject(HttpClient);
  private readonly base = `${environment.apiUrl}/api/tasks`;

  list() {
    return this.http.get<TaskDto[]>(this.base);
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
