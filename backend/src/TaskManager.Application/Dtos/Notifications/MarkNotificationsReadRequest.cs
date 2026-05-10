namespace TaskManager.Application.Dtos.Notifications;

public sealed class MarkNotificationsReadRequest
{
    public required List<Guid> Ids { get; init; }
}
