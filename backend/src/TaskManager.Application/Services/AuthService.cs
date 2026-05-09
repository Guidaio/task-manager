using TaskManager.Application.Abstractions;
using TaskManager.Application.Common;
using TaskManager.Application.Dtos.Auth;
using TaskManager.Domain.Entities;

namespace TaskManager.Application.Services;

public sealed class AuthService : IAuthService
{
    private const int MinimumPasswordLength = 8;

    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokens;

    public AuthService(IUserRepository users, IPasswordHasher passwordHasher, ITokenService tokens)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokens = tokens;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var name = request.Name.Trim();
        var email = NormalizeEmail(request.Email);
        var password = request.Password;

        if (string.IsNullOrWhiteSpace(name))
            return Result<AuthResponse>.Fail("Name is required.");

        if (string.IsNullOrWhiteSpace(email))
            return Result<AuthResponse>.Fail("Email is required.");

        if (string.IsNullOrEmpty(password) || password.Length < MinimumPasswordLength)
            return Result<AuthResponse>.Fail($"Password must be at least {MinimumPasswordLength} characters.");

        var existing = await _users.GetByEmailAsync(email, cancellationToken).ConfigureAwait(false);
        if (existing is not null)
            return Result<AuthResponse>.Fail("Email is already registered.");

        var now = DateTime.UtcNow;
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email,
            PasswordHash = _passwordHasher.HashPassword(password),
            CreatedAtUtc = now,
        };

        var created = await _users.CreateAsync(user, cancellationToken).ConfigureAwait(false);
        var token = _tokens.CreateToken(created);

        return Result<AuthResponse>.Ok(new AuthResponse
        {
            Token = token,
            UserId = created.Id,
            Name = created.Name,
            Email = created.Email,
        });
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var email = NormalizeEmail(request.Email);
        var password = request.Password;

        if (string.IsNullOrWhiteSpace(email))
            return Result<AuthResponse>.Fail("Email is required.");

        if (string.IsNullOrEmpty(password))
            return Result<AuthResponse>.Fail("Password is required.");

        var user = await _users.GetByEmailAsync(email, cancellationToken).ConfigureAwait(false);
        if (user is null)
            return Result<AuthResponse>.Fail("Invalid email or password.");

        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
            return Result<AuthResponse>.Fail("Invalid email or password.");

        var token = _tokens.CreateToken(user);

        return Result<AuthResponse>.Ok(new AuthResponse
        {
            Token = token,
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
        });
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
