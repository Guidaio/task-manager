using TaskManager.Domain.Enums;

namespace TaskManager.Application.Dtos.Tasks;

public sealed class TaskDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskItemStatus Status { get; set; }

    public DateTime? DueDateUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime UpdatedAtUtc { get; set; }
}
