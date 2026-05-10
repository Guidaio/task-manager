export type TaskItemStatus = 'Pending' | 'InProgress' | 'Completed' | 'Cancelled';

export interface TaskDto {
  id: string;
  title: string;
  description: string | null;
  status: TaskItemStatus;
  dueDateUtc: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
}

export interface PagedTasksResponse {
  items: TaskDto[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export interface CreateTaskRequest {
  title: string;
  description?: string | null;
  status: TaskItemStatus;
  dueDateUtc?: string | null;
}

export interface UpdateTaskRequest {
  title: string;
  description?: string | null;
  status: TaskItemStatus;
  dueDateUtc?: string | null;
}
