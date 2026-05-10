using Microsoft.Data.SqlClient;

namespace TaskManager.IntegrationTests;

internal static class DatabaseReset
{
    internal const string DatabaseName = "TaskManager_IntegrationTests";

    private const string MasterConnectionString =
        "Server=localhost,1433;Database=master;User Id=sa;Password=TaskManager_dev_12345;TrustServerCertificate=True;Encrypt=True;";

    internal static async Task DropIfExistsAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(MasterConnectionString);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            IF DB_ID(N'TaskManager_IntegrationTests') IS NOT NULL
            BEGIN
                ALTER DATABASE [TaskManager_IntegrationTests] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                DROP DATABASE [TaskManager_IntegrationTests];
            END
            """;
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
