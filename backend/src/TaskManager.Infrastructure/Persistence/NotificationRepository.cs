using System.Data;
using Microsoft.Data.SqlClient;
using TaskManager.Application.Abstractions;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Persistence;

public sealed class NotificationRepository : INotificationRepository
{
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
