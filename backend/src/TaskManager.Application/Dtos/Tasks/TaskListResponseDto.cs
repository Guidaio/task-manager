namespace TaskManager.Application.Dtos.Tasks;

/// <summary>
/// Paged task list for GET /api/tasks.
/// </summary>
public sealed class TaskListResponseDto
{
    public IReadOnlyList<TaskDto> Items { get; set; } = Array.Empty<TaskDto>();

    public int TotalCount { get; set; }

    public int Page { get; set; }

    public int PageSize { get; set; }
}
