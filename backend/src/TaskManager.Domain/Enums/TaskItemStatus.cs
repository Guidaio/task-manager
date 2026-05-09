namespace TaskManager.Domain.Enums;

/// <summary>
/// Lifecycle state for a user's task. Named TaskItemStatus to avoid clashing with System.Threading.Tasks.TaskStatus.
/// </summary>
public enum TaskItemStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2,
    Cancelled = 3,
}
