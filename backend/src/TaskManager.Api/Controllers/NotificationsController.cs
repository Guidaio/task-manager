using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Extensions;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Dtos.Notifications;
using TaskManager.Domain.Entities;

namespace TaskManager.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/notifications")]
public sealed class NotificationsController : ControllerBase
{
    private readonly INotificationRepository _notifications;

    public NotificationsController(INotificationRepository notifications)
    {
        _notifications = notifications;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var userId = User.TryGetUserId();
        if (userId is null)
            return Unauthorized(new { error = "Missing or invalid authentication." });

        var items = await _notifications.ListByUserIdAsync(userId.Value, cancellationToken).ConfigureAwait(false);
        return Ok(items.Select(Map).ToList());
    }

    [HttpPost("mark-read")]
    public async Task<IActionResult> MarkRead(
        [FromBody] MarkNotificationsReadRequest? body,
        CancellationToken cancellationToken)
    {
        var userId = User.TryGetUserId();
        if (userId is null)
            return Unauthorized(new { error = "Missing or invalid authentication." });

        if (body?.Ids is null || body.Ids.Count == 0)
            return BadRequest(new { error = "At least one notification id is required." });

        if (body.Ids.Count > 50)
            return BadRequest(new { error = "Too many notification ids." });

        var distinct = body.Ids.Distinct().ToList();
        await _notifications
            .MarkAsReadForUserAsync(userId.Value, distinct, cancellationToken)
            .ConfigureAwait(false);
        return NoContent();
    }

    private static NotificationDto Map(Notification n) =>
        new()
        {
            Id = n.Id,
            TaskId = n.TaskId,
            Message = n.Message,
            Type = n.Type,
            IsRead = n.IsRead,
            CreatedAtUtc = n.CreatedAtUtc,
        };
}
