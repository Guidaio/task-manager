using TaskManager.Domain.Entities;

namespace TaskManager.Application.Abstractions;

public interface ITaskRepository
{
    Task<TaskItem?> GetByIdAndUserIdAsync(Guid taskId, Guid userId, CancellationToken cancellationToken);

    Task<IReadOnlyList<TaskItem>> ListByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<TaskItem> CreateAsync(TaskItem task, CancellationToken cancellationToken);

    Task<bool> UpdateAsync(TaskItem task, CancellationToken cancellationToken);

    Task<bool> DeleteAsync(Guid taskId, Guid userId, CancellationToken cancellationToken);
}
