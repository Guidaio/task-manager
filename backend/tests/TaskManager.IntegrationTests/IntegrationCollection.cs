namespace TaskManager.IntegrationTests;

[CollectionDefinition("Integration", DisableParallelization = true)]
public sealed class IntegrationCollection : ICollectionFixture<IntegrationFixture>
{
}
