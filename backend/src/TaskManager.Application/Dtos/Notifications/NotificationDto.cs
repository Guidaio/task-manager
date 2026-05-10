using TaskManager.Domain.Enums;

namespace TaskManager.Application.Dtos.Notifications;

public sealed class NotificationDto
{
    public Guid Id { get; set; }

    public Guid? TaskId { get; set; }

    public string Message { get; set; } = string.Empty;

    public NotificationType Type { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
