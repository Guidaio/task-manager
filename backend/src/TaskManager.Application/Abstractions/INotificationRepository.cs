using TaskManager.Domain.Entities;

namespace TaskManager.Application.Abstractions;

public interface INotificationRepository
{
    Task<Notification> CreateAsync(Notification notification, CancellationToken cancellationToken);

    Task<IReadOnlyList<Notification>> ListByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    /// <summary>
    /// Clears TaskId on notifications so a task row can be deleted (FK uses ON DELETE NO ACTION on TaskId).
    /// </summary>
    Task DetachTaskReferencesAsync(Guid taskId, CancellationToken cancellationToken);
}
