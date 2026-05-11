namespace TaskManager.Application.Dtos.Tasks;

/// <summary>
/// Whitelist for GET /api/tasks sort column (maps to SQL ORDER BY).
/// </summary>
public enum TaskListSortBy
{
    CreatedAtUtc = 0,
    Title,
    Status,
    DueDateUtc,
}
