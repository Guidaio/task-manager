using TaskManager.Application.Common;
using TaskManager.Application.Dtos.Auth;

namespace TaskManager.Application.Abstractions;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken);

    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
}
