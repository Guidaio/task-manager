using System.Threading.Channels;
using TaskManager.Application.Dtos.Notifications;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Messaging;
using TaskManager.Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TaskManager.Infrastructure.SignalR;

namespace TaskManager.Infrastructure.Notifications;

public sealed class NotificationDispatchWorker : BackgroundService
{
    private readonly ChannelReader<NotificationDispatchRequest> _reader;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationDispatchWorker> _logger;

    public NotificationDispatchWorker(
        ChannelReader<NotificationDispatchRequest> reader,
        IServiceScopeFactory scopeFactory,
        ILogger<NotificationDispatchWorker> logger)
    {
        _reader = reader;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var item in _reader.ReadAllAsync(stoppingToken).ConfigureAwait(false))
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var repo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationsHub>>();

                var entity = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = item.UserId,
                    TaskId = item.TaskId,
                    Message = item.Message,
                    Type = item.Type,
                    IsRead = false,
                    CreatedAtUtc = DateTime.UtcNow,
                };

                await repo.CreateAsync(entity, stoppingToken).ConfigureAwait(false);

                var dto = new NotificationDto
                {
                    Id = entity.Id,
                    TaskId = entity.TaskId,
                    Message = entity.Message,
                    Type = entity.Type,
                    IsRead = entity.IsRead,
                    CreatedAtUtc = entity.CreatedAtUtc,
                };

                await hubContext.Clients
                    .User(item.UserId.ToString())
                    .SendAsync("notification", dto, stoppingToken)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to dispatch notification for user {UserId}.", item.UserId);
            }
        }
    }
}
