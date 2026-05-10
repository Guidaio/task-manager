using TaskManager.Domain.Enums;

namespace TaskManager.Application.Messaging;

public readonly record struct NotificationDispatchRequest(
    Guid UserId,
    Guid? TaskId,
    string Message,
    NotificationType Type);
