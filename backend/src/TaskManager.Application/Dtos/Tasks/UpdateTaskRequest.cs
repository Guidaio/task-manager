using TaskManager.Domain.Enums;

namespace TaskManager.Application.Dtos.Tasks;

public sealed class UpdateTaskRequest
{
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TaskItemStatus Status { get; set; }

    public DateTime? DueDateUtc { get; set; }
}
