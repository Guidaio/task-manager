using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskManager.Application.Dtos.Auth;
using TaskManager.Application.Dtos.Notifications;
using TaskManager.Application.Dtos.Tasks;
using TaskManager.Domain.Enums;

namespace TaskManager.IntegrationTests;

[Collection("Integration")]
public sealed class NotificationsApiTests
{
    private readonly IntegrationFixture _fixture;

    public NotificationsApiTests(IntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task List_notifications_without_auth_returns_unauthorized()
    {
        using var client = _fixture.Factory.CreateClient();
        var response = await client.GetAsync(new Uri("/api/notifications", UriKind.Relative));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task List_notifications_with_auth_returns_ok()
    {
        var token = await RegisterAndLoginAsync($"n-{Guid.NewGuid():N}@taskmanager.test");
        using var client = _fixture.Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync(new Uri("/api/notifications", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var items = await response.Content.ReadFromJsonAsync<List<NotificationDto>>(TestJson.Options);
        Assert.NotNull(items);
    }

    [Fact]
    public async Task Task_create_eventually_persist_notification()
    {
        var token = await RegisterAndLoginAsync($"n2-{Guid.NewGuid():N}@taskmanager.test");
        using var client = _fixture.Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        const string title = "Notification integration title";
        var createResp = await client.PostAsJsonAsync(
            new Uri("/api/tasks", UriKind.Relative),
            new CreateTaskRequest { Title = title, Status = TaskItemStatus.Pending },
            TestJson.Options);
        createResp.EnsureSuccessStatusCode();

        NotificationDto? found = null;
        for (var i = 0; i < 20; i++)
        {
            await Task.Delay(150);
            var listResp = await client.GetAsync(new Uri("/api/notifications", UriKind.Relative));
            listResp.EnsureSuccessStatusCode();
            var items = await listResp.Content.ReadFromJsonAsync<List<NotificationDto>>(TestJson.Options);
            found = items?.FirstOrDefault(n =>
                n.Message.Contains(title, StringComparison.Ordinal) &&
                n.Message.Contains("created", StringComparison.OrdinalIgnoreCase));
            if (found is not null)
                break;
        }

        Assert.NotNull(found);
    }

    private async Task<string> RegisterAndLoginAsync(string email)
    {
        using var client = _fixture.Factory.CreateClient();
        await client.PostAsJsonAsync(
            new Uri("/api/auth/register", UriKind.Relative),
            new RegisterRequest { Name = "N", Email = email, Password = "Pw_user_12345" },
            TestJson.Options);

        var login = await client.PostAsJsonAsync(
            new Uri("/api/auth/login", UriKind.Relative),
            new LoginRequest { Email = email, Password = "Pw_user_12345" },
            TestJson.Options);
        login.EnsureSuccessStatusCode();
        var body = await login.Content.ReadFromJsonAsync<AuthResponse>(TestJson.Options);
        return body!.Token;
    }
}