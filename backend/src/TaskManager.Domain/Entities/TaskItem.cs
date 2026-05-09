using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Entities;

public sealed class TaskItem
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskItemStatus Status { get; set; }

    public DateTime? DueDateUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
