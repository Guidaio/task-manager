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
    public async Task Clear_notifications_without_auth_returns_unauthorized()
    {
        using var client = _fixture.Factory.CreateClient();
        var response = await client.DeleteAsync(new Uri("/api/notifications", UriKind.Relative));
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

    [Fact]
    public async Task Mark_read_persists_and_list_returns_isRead()
    {
        var token = await RegisterAndLoginAsync($"nread-{Guid.NewGuid():N}@taskmanager.test");
        using var client = _fixture.Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        const string title = "Mark read notification title";
        var createResp = await client.PostAsJsonAsync(
            new Uri("/api/tasks", UriKind.Relative),
            new CreateTaskRequest { Title = title, Status = TaskItemStatus.Pending },
            TestJson.Options);
        createResp.EnsureSuccessStatusCode();

        NotificationDto? note = null;
        for (var i = 0; i < 20; i++)
        {
            await Task.Delay(150);
            var listResp = await client.GetAsync(new Uri("/api/notifications", UriKind.Relative));
            listResp.EnsureSuccessStatusCode();
            var items = await listResp.Content.ReadFromJsonAsync<List<NotificationDto>>(TestJson.Options);
            note = items?.FirstOrDefault(n =>
                n.Message.Contains(title, StringComparison.Ordinal) &&
                n.Message.Contains("created", StringComparison.OrdinalIgnoreCase));
            if (note is not null)
                break;
        }

        Assert.NotNull(note);
        Assert.False(note.IsRead);

        var markResp = await client.PostAsJsonAsync(
            new Uri("/api/notifications/mark-read", UriKind.Relative),
            new MarkNotificationsReadRequest { Ids = [note.Id] },
            TestJson.Options);
        Assert.Equal(HttpStatusCode.NoContent, markResp.StatusCode);

        var listAfter = await client.GetAsync(new Uri("/api/notifications", UriKind.Relative));
        listAfter.EnsureSuccessStatusCode();
        var listItems = await listAfter.Content.ReadFromJsonAsync<List<NotificationDto>>(TestJson.Options);
        var updated = listItems!.First(n => n.Id == note.Id);
        Assert.True(updated.IsRead);
    }

    [Fact]
    public async Task Clear_notifications_removes_all_rows_for_user()
    {
        var token = await RegisterAndLoginAsync($"nclear-{Guid.NewGuid():N}@taskmanager.test");
        using var client = _fixture.Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResp = await client.PostAsJsonAsync(
            new Uri("/api/tasks", UriKind.Relative),
            new CreateTaskRequest { Title = "For clear test", Status = TaskItemStatus.Pending },
            TestJson.Options);
        createResp.EnsureSuccessStatusCode();

        NotificationDto? note = null;
        for (var i = 0; i < 20; i++)
        {
            await Task.Delay(150);
            var listResp = await client.GetAsync(new Uri("/api/notifications", UriKind.Relative));
            listResp.EnsureSuccessStatusCode();
            var items = await listResp.Content.ReadFromJsonAsync<List<NotificationDto>>(TestJson.Options);
            note = items?.FirstOrDefault(n =>
                n.Message.Contains("For clear test", StringComparison.Ordinal) &&
                n.Message.Contains("created", StringComparison.OrdinalIgnoreCase));
            if (note is not null)
                break;
        }

        Assert.NotNull(note);

        var clearResp = await client.DeleteAsync(new Uri("/api/notifications", UriKind.Relative));
        Assert.Equal(HttpStatusCode.NoContent, clearResp.StatusCode);

        var listAfter = await client.GetAsync(new Uri("/api/notifications", UriKind.Relative));
        listAfter.EnsureSuccessStatusCode();
        var listItems = await listAfter.Content.ReadFromJsonAsync<List<NotificationDto>>(TestJson.Options);
        Assert.NotNull(listItems);
        Assert.DoesNotContain(listItems!, n => n.Id == note!.Id);
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