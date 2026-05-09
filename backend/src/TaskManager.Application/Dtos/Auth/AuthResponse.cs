namespace TaskManager.Application.Dtos.Auth;

public sealed class AuthResponse
{
    public string Token { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
}
