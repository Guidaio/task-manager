using System.Threading.Channels;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Messaging;

namespace TaskManager.Infrastructure.Notifications;

public sealed class NotificationPublisher : INotificationPublisher
{
    private readonly ChannelWriter<NotificationDispatchRequest> _writer;

    public NotificationPublisher(ChannelWriter<NotificationDispatchRequest> writer)
    {
        _writer = writer;
    }

    public ValueTask PublishAsync(NotificationDispatchRequest request, CancellationToken cancellationToken = default) =>
        _writer.WriteAsync(request, cancellationToken);
}
