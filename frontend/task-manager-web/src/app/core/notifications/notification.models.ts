/** Shape returned by GET /api/notifications and SignalR `notification` payloads (camelCase JSON). */
export interface NotificationRecord {
  id: string;
  taskId: string | null;
  message: string;
  type: string;
  isRead: boolean;
  createdAtUtc: string;
}

export type NotificationPayload = NotificationRecord;
