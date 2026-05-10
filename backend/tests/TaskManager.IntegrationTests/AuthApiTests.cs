using System.Net;
using System.Net.Http.Json;
using TaskManager.Application.Dtos.Auth;
using TaskManager.Infrastructure.Seed;

namespace TaskManager.IntegrationTests;

[Collection("Integration")]
public sealed class AuthApiTests
{
    private readonly IntegrationFixture _fixture;

    public AuthApiTests(IntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Login_demo_user_returns_token()
    {
        using var client = _fixture.Factory.CreateClient();
        var response = await client
            .PostAsJsonAsync(
                new Uri("/api/auth/login", UriKind.Relative),
                new LoginRequest { Email = DemoSeed.Email, Password = DemoSeed.Password },
                TestJson.Options)
;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>(TestJson.Options);
        Assert.NotNull(body);
        Assert.False(string.IsNullOrWhiteSpace(body.Token));
        Assert.Equal(DemoSeed.UserId, body.UserId);
        Assert.Equal(DemoSeed.Email, body.Email);
    }

    [Fact]
    public async Task Register_new_user_then_login_returns_token()
    {
        var email = $"it-{Guid.NewGuid():N}@taskmanager.test";
        using var client = _fixture.Factory.CreateClient();

        var registerResponse = await client
            .PostAsJsonAsync(
                new Uri("/api/auth/register", UriKind.Relative),
                new RegisterRequest { Name = "Integration User", Email = email, Password = "Register_pw_12345" },
                TestJson.Options)
;

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);
        var registered = await registerResponse.Content.ReadFromJsonAsync<AuthResponse>(TestJson.Options);
        Assert.NotNull(registered);
        Assert.Equal(email, registered.Email);

        var loginResponse = await client
            .PostAsJsonAsync(
                new Uri("/api/auth/login", UriKind.Relative),
                new LoginRequest { Email = email, Password = "Register_pw_12345" },
                TestJson.Options)
;

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var loggedIn = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>(TestJson.Options);
        Assert.NotNull(loggedIn);
        Assert.False(string.IsNullOrWhiteSpace(loggedIn.Token));
    }

    [Fact]
    public async Task Login_invalid_password_is_unauthorized()
    {
        using var client = _fixture.Factory.CreateClient();
        var response = await client
            .PostAsJsonAsync(
                new Uri("/api/auth/login", UriKind.Relative),
                new LoginRequest { Email = DemoSeed.Email, Password = "wrong-password" },
                TestJson.Options)
;

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
