using TaskManager.Application.Messaging;

namespace TaskManager.Application.Abstractions;

public interface INotificationPublisher
{
    ValueTask PublishAsync(NotificationDispatchRequest request, CancellationToken cancellationToken = default);
}
