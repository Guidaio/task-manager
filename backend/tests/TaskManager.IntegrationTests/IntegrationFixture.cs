namespace TaskManager.IntegrationTests;

public sealed class IntegrationFixture : IAsyncLifetime
{
    public IntegrationTestWebApplicationFactory Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await DatabaseReset.DropIfExistsAsync().ConfigureAwait(false);
        Factory = new IntegrationTestWebApplicationFactory();
        _ = Factory.Server;
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync().ConfigureAwait(false);
        await DatabaseReset.DropIfExistsAsync().ConfigureAwait(false);
    }
}
