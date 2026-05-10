using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAndUserIdAsync(Guid taskId, Guid userId, CancellationToken cancellationToken);

    Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> ListByUserIdPagedAsync(
        Guid userId,
        TaskItemStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken);

    Task<TaskItem> CreateAsync(TaskItem task, CancellationToken cancellationToken);

    Task<bool> UpdateAsync(TaskItem task, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid taskId, Guid userId, CancellationToken cancellationToken);
}
