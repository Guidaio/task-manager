using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;

namespace TaskManager.Infrastructure.SignalR;

public sealed class SignalRUserIdProvider : IUserIdProvider
{
    public string? GetUserId(HubConnectionContext connection) =>
        connection.User?.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? connection.User?.FindFirstValue(JwtRegisteredClaimNames.Sub);
}
