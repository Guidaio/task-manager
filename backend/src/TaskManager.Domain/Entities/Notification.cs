using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Entities;

public sealed class Notification
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid? TaskId { get; set; }

    public string Message { get; set; } = string.Empty;

    public NotificationType Type { get; set; }

    public bool IsRead { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
