using TaskManager.Domain.Entities;

namespace TaskManager.Application.Abstractions;

public interface INotificationRepository
{
    Task<Notification> CreateAsync(Notification notification, CancellationToken cancellationToken);

    Task<IReadOnlyList<Notification>> ListByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
