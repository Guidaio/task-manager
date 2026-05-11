using System.Data;
using Microsoft.Data.SqlClient;
using TaskManager.Application.Abstractions;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Persistence;

public sealed class NotificationRepository : INotificationRepository
{
    private static readonly TimeSpan ListRetention = TimeSpan.FromDays(30);

    private readonly IDbConnectionFactory _connections;

    public NotificationRepository(IDbConnectionFactory connections)
    {
        _connections = connections;
    }

    public async Task<Notification> CreateAsync(Notification notification, CancellationToken cancellationToken)
    {
        await using var connection = _connections.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Notifications (Id, UserId, TaskId, Message, Type, IsRead, CreatedAtUtc)
            VALUES (@Id, @UserId, @TaskId, @Message, @Type, @IsRead, @CreatedAtUtc);
            """;
        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = notification.Id });
        command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = notification.UserId });
        command.Parameters.Add(new SqlParameter("@TaskId", SqlDbType.UniqueIdentifier)
        {
            Value = notification.TaskId.HasValue ? notification.TaskId.Value : DBNull.Value,
        });
        command.Parameters.Add(new SqlParameter("@Message", SqlDbType.NVarChar, 1000) { Value = notification.Message });
        command.Parameters.Add(new SqlParameter("@Type", SqlDbType.TinyInt) { Value = (byte)notification.Type });
        command.Parameters.Add(new SqlParameter("@IsRead", SqlDbType.Bit) { Value = notification.IsRead });
        command.Parameters.Add(new SqlParameter("@CreatedAtUtc", SqlDbType.DateTime2) { Value = notification.CreatedAtUtc });

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return notification;
    }

    public async Task DetachTaskReferencesAsync(Guid taskId, CancellationToken cancellationToken)
    {
        await using var connection = _connections.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE dbo.Notifications SET TaskId = NULL WHERE TaskId = @TaskId;
            """;
        command.Parameters.Add(new SqlParameter("@TaskId", SqlDbType.UniqueIdentifier) { Value = taskId });

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<Notification>> ListByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        await using var connection = _connections.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        var cutoff = DateTime.UtcNow - ListRetention;
        await using (var pruneCmd = connection.CreateCommand())
        {
            pruneCmd.CommandText = """
                DELETE FROM dbo.Notifications
                WHERE UserId = @UserId AND CreatedAtUtc < @Cutoff;
                """;
            pruneCmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
            pruneCmd.Parameters.Add(new SqlParameter("@Cutoff", SqlDbType.DateTime2) { Value = cutoff });
            await pruneCmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, UserId, TaskId, Message, Type, IsRead, CreatedAtUtc
            FROM Notifications
            WHERE UserId = @UserId
            ORDER BY CreatedAtUtc DESC;
            """;
        command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var list = new List<Notification>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            list.Add(Map(reader));

        return list;
    }

    public async Task MarkAsReadForUserAsync(
        Guid userId,
        IReadOnlyList<Guid> notificationIds,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(notificationIds);
        if (notificationIds.Count == 0)
            return;

        await using var connection = _connections.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        var paramNames = new string[notificationIds.Count];
        for (var i = 0; i < notificationIds.Count; i++)
        {
            var name = $"@id{i}";
            paramNames[i] = name;
            command.Parameters.Add(new SqlParameter(name, SqlDbType.UniqueIdentifier) { Value = notificationIds[i] });
        }

        command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
        command.CommandText = $"""
            UPDATE dbo.Notifications
            SET IsRead = 1
            WHERE UserId = @UserId AND Id IN ({string.Join(", ", paramNames)});
            """;

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<int> DeleteAllForUserAsync(Guid userId, CancellationToken cancellationToken)
    {
        await using var connection = _connections.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            DELETE FROM dbo.Notifications
            WHERE UserId = @UserId;
            """;
        command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });

        return await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }

    private static Notification Map(SqlDataReader reader)
    {
        var taskIdOrdinal = reader.GetOrdinal("TaskId");

        return new Notification
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
            TaskId = reader.IsDBNull(taskIdOrdinal) ? null : reader.GetGuid(taskIdOrdinal),
            Message = reader.GetString(reader.GetOrdinal("Message")),
            Type = (NotificationType)reader.GetByte(reader.GetOrdinal("Type")),
            IsRead = reader.GetBoolean(reader.GetOrdinal("IsRead")),
            CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc")),
        };
    }
}
