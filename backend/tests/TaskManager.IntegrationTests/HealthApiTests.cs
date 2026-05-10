using System.Net;
using System.Net.Http.Json;

namespace TaskManager.IntegrationTests;

[Collection("Integration")]
public sealed class HealthApiTests
{
    private readonly IntegrationFixture _fixture;

    public HealthApiTests(IntegrationFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Get_health_returns_ok_and_payload()
    {
        using var client = _fixture.Factory.CreateClient();
        var response = await client.GetAsync(new Uri("/api/health", UriKind.Relative));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<HealthResponse>(TestJson.Options);
        Assert.NotNull(body);
        Assert.Equal("Healthy", body.Status);
    }

    private sealed class HealthResponse
    {
        public string Status { get; set; } = string.Empty;
    }
}
