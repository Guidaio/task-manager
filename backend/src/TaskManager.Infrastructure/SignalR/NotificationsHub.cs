using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace TaskManager.Infrastructure.SignalR;

[Authorize]
public sealed class NotificationsHub : Hub
{
}
