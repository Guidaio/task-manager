using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using TaskManager.Application.Dtos.Auth;
using TaskManager.Application.Dtos.Tasks;
using TaskManager.Domain.Enums;
using TaskManager.Infrastructure.Seed;

namespace TaskManager.IntegrationTests;

[Collection("Integration")]
public sealed class TasksApiTests
{
    private readonly IntegrationFixture _fixture;

    public TasksApiTests(IntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task List_tasks_without_auth_returns_unauthorized()
    {
        using var client = _fixture.Factory.CreateClient();
        var response = await client.GetAsync(new Uri("/api/tasks", UriKind.Relative));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_task_without_auth_returns_unauthorized()
    {
        using var client = _fixture.Factory.CreateClient();
        var response = await client
            .PostAsJsonAsync(
                new Uri("/api/tasks", UriKind.Relative),
                new CreateTaskRequest { Title = "Blocked", Status = TaskItemStatus.Pending },
                TestJson.Options)
;

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Demo_user_lists_seeded_tasks()
    {
        var token = await LoginDemoAsync();
        using var client = _fixture.Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync(new Uri("/api/tasks", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var page = await response.Content.ReadFromJsonAsync<TaskListResponseDto>(TestJson.Options);
        Assert.NotNull(page);
        var tasks = page!.Items;
        Assert.True(tasks.Count >= 2);
        Assert.Contains(tasks, t => t.Id == DemoSeed.TaskWelcomeId);
    }

    [Fact]
    public async Task Task_crud_round_trip_for_new_user()
    {
        var email = $"tasks-{Guid.NewGuid():N}@taskmanager.test";
        var token = await RegisterAndLoginAsync(email);

        using var client = _fixture.Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createResponse = await client
            .PostAsJsonAsync(
                new Uri("/api/tasks", UriKind.Relative),
                new CreateTaskRequest
                {
                    Title = "Integration CRUD",
                    Description = "from tests",
                    Status = TaskItemStatus.InProgress,
                },
                TestJson.Options)
;

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var created = await createResponse.Content.ReadFromJsonAsync<TaskDto>(TestJson.Options);
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal("Integration CRUD", created.Title);

        var listResponse = await client.GetAsync(new Uri("/api/tasks", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);
        var listBody = await listResponse.Content.ReadFromJsonAsync<TaskListResponseDto>(TestJson.Options);
        Assert.Contains(listBody!.Items, t => t.Id == created.Id);

        var getResponse = await client.GetAsync(new Uri($"/api/tasks/{created.Id}", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var updateResponse = await client
            .PutAsJsonAsync(
                new Uri($"/api/tasks/{created.Id}", UriKind.Relative),
                new UpdateTaskRequest
                {
                    Title = "Updated title",
                    Description = "d",
                    Status = TaskItemStatus.Completed,
                    DueDateUtc = DateTime.UtcNow.AddDays(1),
                },
                TestJson.Options)
;
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updated = await updateResponse.Content.ReadFromJsonAsync<TaskDto>(TestJson.Options);
        Assert.Equal("Updated title", updated!.Title);
        Assert.Equal(TaskItemStatus.Completed, updated.Status);

        var deleteResponse = await client.DeleteAsync(new Uri($"/api/tasks/{created.Id}", UriKind.Relative));
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var gone = await client.GetAsync(new Uri($"/api/tasks/{created.Id}", UriKind.Relative));
        Assert.Equal(HttpStatusCode.NotFound, gone.StatusCode);
    }

    [Fact]
    public async Task Another_user_cannot_read_update_or_delete_peer_task()
    {
        var emailA = $"iso-a-{Guid.NewGuid():N}@taskmanager.test";
        var emailB = $"iso-b-{Guid.NewGuid():N}@taskmanager.test";
        var tokenA = await RegisterAndLoginAsync(emailA);
        var tokenB = await RegisterAndLoginAsync(emailB);

        Guid taskId;
        using (var clientA = _fixture.Factory.CreateClient())
        {
            clientA.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenA);
            var createResponse = await clientA
                .PostAsJsonAsync(
                    new Uri("/api/tasks", UriKind.Relative),
                    new CreateTaskRequest { Title = "Private to A", Status = TaskItemStatus.Pending },
                    TestJson.Options)
;
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
            var created = await createResponse.Content.ReadFromJsonAsync<TaskDto>(TestJson.Options);
            taskId = created!.Id;
        }

        using var clientB = _fixture.Factory.CreateClient();
        clientB.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenB);

        var get = await clientB.GetAsync(new Uri($"/api/tasks/{taskId}", UriKind.Relative));
        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);

        var put = await clientB
            .PutAsJsonAsync(
                new Uri($"/api/tasks/{taskId}", UriKind.Relative),
                new UpdateTaskRequest { Title = "Hacked", Status = TaskItemStatus.Completed },
                TestJson.Options)
;
        Assert.Equal(HttpStatusCode.NotFound, put.StatusCode);

        var delete = await clientB.DeleteAsync(new Uri($"/api/tasks/{taskId}", UriKind.Relative));
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
    }

    [Fact]
    public async Task List_tasks_invalid_status_returns_bad_request()
    {
        var token = await LoginDemoAsync();
        using var client = _fixture.Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await client.GetAsync(new Uri("/api/tasks?status=NotAStatus", UriKind.Relative));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task List_tasks_filters_by_status()
    {
        var email = $"filter-{Guid.NewGuid():N}@taskmanager.test";
        var token = await RegisterAndLoginAsync(email);
        using var client = _fixture.Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var createPending = await client.PostAsJsonAsync(
            new Uri("/api/tasks", UriKind.Relative),
            new CreateTaskRequest { Title = "Pending only", Status = TaskItemStatus.Pending },
            TestJson.Options);
        createPending.EnsureSuccessStatusCode();
        var createCompleted = await client.PostAsJsonAsync(
            new Uri("/api/tasks", UriKind.Relative),
            new CreateTaskRequest { Title = "Completed only", Status = TaskItemStatus.Completed },
            TestJson.Options);
        createCompleted.EnsureSuccessStatusCode();

        var response = await client.GetAsync(new Uri("/api/tasks?status=Completed", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<TaskListResponseDto>(TestJson.Options);
        Assert.NotNull(body!.Items);
        Assert.All(body.Items, t => Assert.Equal(TaskItemStatus.Completed, t.Status));
        Assert.Contains(body.Items, t => t.Title == "Completed only");
        Assert.DoesNotContain(body.Items, t => t.Title == "Pending only");
    }

    [Fact]
    public async Task List_tasks_pagination_returns_total_and_slice()
    {
        var email = $"page-{Guid.NewGuid():N}@taskmanager.test";
        var token = await RegisterAndLoginAsync(email);
        using var client = _fixture.Factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        for (var i = 0; i < 3; i++)
        {
            var r = await client.PostAsJsonAsync(
                new Uri("/api/tasks", UriKind.Relative),
                new CreateTaskRequest { Title = $"Paged {i}", Status = TaskItemStatus.Pending },
                TestJson.Options);
            r.EnsureSuccessStatusCode();
        }

        var first = await client.GetAsync(new Uri("/api/tasks?page=1&pageSize=2", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var body = await first.Content.ReadFromJsonAsync<TaskListResponseDto>(TestJson.Options);
        Assert.Equal(3, body!.TotalCount);
        Assert.Equal(2, body.Items.Count);
        Assert.Equal(1, body.Page);
        Assert.Equal(2, body.PageSize);

        var second = await client.GetAsync(new Uri("/api/tasks?page=2&pageSize=2", UriKind.Relative));
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        var body2 = await second.Content.ReadFromJsonAsync<TaskListResponseDto>(TestJson.Options);
        Assert.Equal(3, body2!.TotalCount);
        Assert.Single(body2.Items);
        Assert.Equal(2, body2.Page);
    }

    private async Task<string> LoginDemoAsync()
    {
        using var client = _fixture.Factory.CreateClient();
        var response = await client
            .PostAsJsonAsync(
                new Uri("/api/auth/login", UriKind.Relative),
                new LoginRequest { Email = DemoSeed.Email, Password = DemoSeed.Password },
                TestJson.Options)
;
        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>(TestJson.Options);
        return body!.Token;
    }

    private async Task<string> RegisterAndLoginAsync(string email)
    {
        using var client = _fixture.Factory.CreateClient();
        await client
            .PostAsJsonAsync(
                new Uri("/api/auth/register", UriKind.Relative),
                new RegisterRequest { Name = "T", Email = email, Password = "Pw_user_12345" },
                TestJson.Options)
;

        var login = await client
            .PostAsJsonAsync(
                new Uri("/api/auth/login", UriKind.Relative),
                new LoginRequest { Email = email, Password = "Pw_user_12345" },
                TestJson.Options)
;
        login.EnsureSuccessStatusCode();
        var body = await login.Content.ReadFromJsonAsync<AuthResponse>(TestJson.Options);
        return body!.Token;
    }
}
