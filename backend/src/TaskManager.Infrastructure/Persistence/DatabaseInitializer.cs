using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskManager.Application.Abstractions;
using TaskManager.Domain.Enums;
using TaskManager.Infrastructure.Data;
using TaskManager.Infrastructure.Options;
using TaskManager.Infrastructure.Seed;

namespace TaskManager.Infrastructure.Persistence;

public sealed class DatabaseInitializer
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly DatabaseOptions _databaseOptions;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<DatabaseInitializer> _logger;

    public DatabaseInitializer(
        IDbConnectionFactory connectionFactory,
        IOptions<DatabaseOptions> databaseOptions,
        IPasswordHasher passwordHasher,
        ILogger<DatabaseInitializer> logger)
    {
        _connectionFactory = connectionFactory;
        _databaseOptions = databaseOptions.Value;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_databaseOptions.ConnectionString))
            throw new InvalidOperationException("Database:ConnectionString is not configured.");

        var builder = new SqlConnectionStringBuilder(_databaseOptions.ConnectionString);
        var databaseName = builder.InitialCatalog;
        if (string.IsNullOrWhiteSpace(databaseName))
            throw new InvalidOperationException("Database connection string must specify Initial Catalog / Database.");

        if (!string.IsNullOrWhiteSpace(_databaseOptions.MasterConnectionString))
        {
            await using var masterConnection = new SqlConnection(_databaseOptions.MasterConnectionString);
            await masterConnection.OpenAsync(cancellationToken).ConfigureAwait(false);
            await EnsureDatabaseExistsAsync(masterConnection, databaseName, cancellationToken).ConfigureAwait(false);
        }

        await using var connection = _connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await EnsureSchemaAsync(connection, cancellationToken).ConfigureAwait(false);
        await SeedDemoDataAsync(connection, cancellationToken).ConfigureAwait(false);
    }

    private async Task EnsureDatabaseExistsAsync(SqlConnection masterConnection, string databaseName, CancellationToken cancellationToken)
    {
        await using var command = masterConnection.CreateCommand();
        command.CommandText = """
            IF NOT EXISTS (SELECT 1 FROM sys.databases WHERE name = @DbName)
            BEGIN
                DECLARE @sql nvarchar(max) = N'CREATE DATABASE ' + QUOTENAME(@DbName);
                EXEC (@sql);
            END
            """;
        command.Parameters.Add(new SqlParameter("@DbName", SqlDbType.NVarChar, 128) { Value = databaseName });

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        _logger.LogInformation("Ensured database {DatabaseName} exists.", databaseName);
    }

    private async Task EnsureSchemaAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        await ExecuteNonQueryAsync(
            connection,
            """
            IF OBJECT_ID(N'dbo.Users', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Users (
                    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
                    Name NVARCHAR(200) NOT NULL,
                    Email NVARCHAR(320) NOT NULL,
                    PasswordHash NVARCHAR(MAX) NOT NULL,
                    CreatedAtUtc DATETIME2 NOT NULL,
                    CONSTRAINT UQ_Users_Email UNIQUE (Email)
                );
            END
            """,
            cancellationToken).ConfigureAwait(false);

        await ExecuteNonQueryAsync(
            connection,
            """
            IF OBJECT_ID(N'dbo.TaskItems', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.TaskItems (
                    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_TaskItems PRIMARY KEY,
                    UserId UNIQUEIDENTIFIER NOT NULL,
                    Title NVARCHAR(500) NOT NULL,
                    Description NVARCHAR(MAX) NULL,
                    Status TINYINT NOT NULL,
                    DueDateUtc DATETIME2 NULL,
                    CreatedAtUtc DATETIME2 NOT NULL,
                    UpdatedAtUtc DATETIME2 NOT NULL,
                    CONSTRAINT FK_TaskItems_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE
                );
                CREATE INDEX IX_TaskItems_UserId ON dbo.TaskItems(UserId);
            END
            """,
            cancellationToken).ConfigureAwait(false);

        await ExecuteNonQueryAsync(
            connection,
            """
            IF OBJECT_ID(N'dbo.Notifications', N'U') IS NULL
            BEGIN
                CREATE TABLE dbo.Notifications (
                    Id UNIQUEIDENTIFIER NOT NULL CONSTRAINT PK_Notifications PRIMARY KEY,
                    UserId UNIQUEIDENTIFIER NOT NULL,
                    TaskId UNIQUEIDENTIFIER NULL,
                    Message NVARCHAR(1000) NOT NULL,
                    Type TINYINT NOT NULL,
                    IsRead BIT NOT NULL,
                    CreatedAtUtc DATETIME2 NOT NULL,
                    CONSTRAINT FK_Notifications_Users FOREIGN KEY (UserId) REFERENCES dbo.Users(Id) ON DELETE CASCADE,
                    CONSTRAINT FK_Notifications_TaskItems FOREIGN KEY (TaskId) REFERENCES dbo.TaskItems(Id) ON DELETE NO ACTION
                );
                CREATE INDEX IX_Notifications_UserId ON dbo.Notifications(UserId);
            END
            """,
            cancellationToken).ConfigureAwait(false);

        _logger.LogInformation("Database schema verified.");
    }

    private async Task SeedDemoDataAsync(SqlConnection connection, CancellationToken cancellationToken)
    {
        await using (var existsCmd = connection.CreateCommand())
        {
            existsCmd.CommandText = "SELECT 1 FROM dbo.Users WHERE Email = @Email;";
            existsCmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 320) { Value = DemoSeed.Email });

            var exists = await existsCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            if (exists is null || exists is DBNull)
            {
                var hash = _passwordHasher.HashPassword(DemoSeed.Password);
                var now = DateTime.UtcNow;

                await using var insert = connection.CreateCommand();
                insert.CommandText = """
                    INSERT INTO dbo.Users (Id, Name, Email, PasswordHash, CreatedAtUtc)
                    VALUES (@Id, @Name, @Email, @PasswordHash, @CreatedAtUtc);
                    """;
                insert.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = DemoSeed.UserId });
                insert.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 200) { Value = DemoSeed.Name });
                insert.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 320) { Value = DemoSeed.Email });
                insert.Parameters.Add(new SqlParameter("@PasswordHash", SqlDbType.NVarChar, -1) { Value = hash });
                insert.Parameters.Add(new SqlParameter("@CreatedAtUtc", SqlDbType.DateTime2) { Value = now });

                await insert.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                _logger.LogInformation("Seeded demo user {Email}.", DemoSeed.Email);
            }
        }

        await using (var countCmd = connection.CreateCommand())
        {
            countCmd.CommandText = "SELECT COUNT(1) FROM dbo.TaskItems WHERE UserId = @UserId;";
            countCmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = DemoSeed.UserId });

            var countObj = await countCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            var count = Convert.ToInt32(countObj);
            if (count > 0)
                return;

            var now = DateTime.UtcNow;

            await using var insertTasks = connection.CreateCommand();
            insertTasks.CommandText = """
                INSERT INTO dbo.TaskItems (Id, UserId, Title, Description, Status, DueDateUtc, CreatedAtUtc, UpdatedAtUtc)
                VALUES
                    (@Id1, @UserId, @Title1, @Desc1, @Status1, @Due1, @Created1, @Updated1),
                    (@Id2, @UserId, @Title2, @Desc2, @Status2, @Due2, @Created2, @Updated2);
                """;
            insertTasks.Parameters.Add(new SqlParameter("@Id1", SqlDbType.UniqueIdentifier) { Value = DemoSeed.TaskWelcomeId });
            insertTasks.Parameters.Add(new SqlParameter("@Id2", SqlDbType.UniqueIdentifier) { Value = DemoSeed.TaskSecondId });
            insertTasks.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = DemoSeed.UserId });
            insertTasks.Parameters.Add(new SqlParameter("@Title1", SqlDbType.NVarChar, 500) { Value = "Welcome to Task Manager" });
            insertTasks.Parameters.Add(new SqlParameter("@Desc1", SqlDbType.NVarChar, -1) { Value = "Use the API or UI to manage your tasks." });
            insertTasks.Parameters.Add(new SqlParameter("@Status1", SqlDbType.TinyInt) { Value = (byte)TaskItemStatus.Pending });
            insertTasks.Parameters.Add(new SqlParameter("@Due1", SqlDbType.DateTime2) { Value = DBNull.Value });
            insertTasks.Parameters.Add(new SqlParameter("@Created1", SqlDbType.DateTime2) { Value = now });
            insertTasks.Parameters.Add(new SqlParameter("@Updated1", SqlDbType.DateTime2) { Value = now });

            insertTasks.Parameters.Add(new SqlParameter("@Title2", SqlDbType.NVarChar, 500) { Value = "Explore authentication" });
            insertTasks.Parameters.Add(new SqlParameter("@Desc2", SqlDbType.NVarChar, -1) { Value = "Register a new account or log in as the demo user." });
            insertTasks.Parameters.Add(new SqlParameter("@Status2", SqlDbType.TinyInt) { Value = (byte)TaskItemStatus.InProgress });
            insertTasks.Parameters.Add(new SqlParameter("@Due2", SqlDbType.DateTime2) { Value = now.AddDays(7) });
            insertTasks.Parameters.Add(new SqlParameter("@Created2", SqlDbType.DateTime2) { Value = now });
            insertTasks.Parameters.Add(new SqlParameter("@Updated2", SqlDbType.DateTime2) { Value = now });

            await insertTasks.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
            _logger.LogInformation("Seeded demo tasks for user {UserId}.", DemoSeed.UserId);
        }
    }

    private static async Task ExecuteNonQueryAsync(SqlConnection connection, string sql, CancellationToken cancellationToken)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}
