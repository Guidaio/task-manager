using Moq;
using TaskManager.Application.Abstractions;
using TaskManager.Application.Dtos.Auth;
using TaskManager.Application.Dtos.Tasks;
using TaskManager.Application.Services;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;

namespace TaskManager.UnitTests;

public sealed class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<ITokenService> _tokens = new();

    private AuthService CreateSut() => new(_users.Object, _passwordHasher.Object, _tokens.Object);

    [Fact]
    public async Task Register_ShouldFail_WhenEmailAlreadyExists()
    {
        var sut = CreateSut();
        var existing = new User { Id = Guid.NewGuid(), Email = "a@test.com", Name = "A", PasswordHash = "hash", CreatedAtUtc = DateTime.UtcNow };
        _users.Setup(x => x.GetByEmailAsync("a@test.com", It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var result = await sut.RegisterAsync(new RegisterRequest { Name = "B", Email = "  A@Test.COM ", Password = "password123" }, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("Email is already registered.", result.Error);
        _users.Verify(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Login_ShouldFail_WhenPasswordIsInvalid()
    {
        var sut = CreateSut();
        var user = new User { Id = Guid.NewGuid(), Email = "a@test.com", Name = "A", PasswordHash = "stored-hash", CreatedAtUtc = DateTime.UtcNow };
        _users.Setup(x => x.GetByEmailAsync("a@test.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(x => x.VerifyPassword("wrong", "stored-hash")).Returns(false);

        var result = await sut.LoginAsync(new LoginRequest { Email = "a@test.com", Password = "wrong" }, CancellationToken.None);

        Assert.False(result.Succeeded);
        Assert.Equal("Invalid email or password.", result.Error);
        _tokens.Verify(x => x.CreateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public async Task Login_ShouldReturnToken_WhenCredentialsAreValid()
    {
        var sut = CreateSut();
        var user = new User { Id = Guid.NewGuid(), Email = "a@test.com", Name = "A", PasswordHash = "stored-hash", CreatedAtUtc = DateTime.UtcNow };
        _users.Setup(x => x.GetByEmailAsync("a@test.com", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(x => x.VerifyPassword("password123", "stored-hash")).Returns(true);
        _tokens.Setup(x => x.CreateToken(user)).Returns("jwt-token");

        var result = await sut.LoginAsync(new LoginRequest { Email = "  A@Test.COM ", Password = "password123" }, CancellationToken.None);

        Assert.True(result.Succeeded);
        Assert.NotNull(result.Value);
        Assert.Equal("jwt-token", result.Value!.Token);
        Assert.Equal(user.Id, result.Value.UserId);
    }
}
