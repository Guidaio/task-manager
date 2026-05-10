using System.Data;
using Microsoft.Data.SqlClient;
using TaskManager.Application.Abstractions;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Persistence;

public sealed class TaskRepository : ITaskRepository
{
    private readonly IDbConnectionFactory _connections;

    public TaskRepository(IDbConnectionFactory connections)
    {
        _connections = connections;
    }

    public async Task<TaskItem?> GetByIdAndUserIdAsync(Guid taskId, Guid userId, CancellationToken cancellationToken)
    {
        await using var connection = _connections.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, UserId, Title, Description, Status, DueDateUtc, CreatedAtUtc, UpdatedAtUtc
            FROM TaskItems
            WHERE Id = @Id AND UserId = @UserId;
            """;
        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = taskId });
        command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            return null;

        return Map(reader);
    }

    public async Task<(IReadOnlyList<TaskItem> Items, int TotalCount)> ListByUserIdPagedAsync(
        Guid userId,
        TaskItemStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        if (page < 1)
            page = 1;
        if (pageSize < 1)
            pageSize = 1;

        var offset = (page - 1) * pageSize;

        await using var connection = _connections.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        int total;
        await using (var countCmd = connection.CreateCommand())
        {
            countCmd.CommandText = """
                SELECT COUNT(1)
                FROM TaskItems
                WHERE UserId = @UserId
                  AND (@HasStatus = 0 OR Status = @StatusVal);
                """;
            countCmd.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
            countCmd.Parameters.Add(new SqlParameter("@HasStatus", SqlDbType.Bit) { Value = status.HasValue });
            countCmd.Parameters.Add(new SqlParameter("@StatusVal", SqlDbType.TinyInt)
            {
                Value = status.HasValue ? (byte)status.Value : (object)DBNull.Value,
            });
            var scalar = await countCmd.ExecuteScalarAsync(cancellationToken).ConfigureAwait(false);
            total = scalar is int i ? i : Convert.ToInt32(scalar!, System.Globalization.CultureInfo.InvariantCulture);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, UserId, Title, Description, Status, DueDateUtc, CreatedAtUtc, UpdatedAtUtc
            FROM TaskItems
            WHERE UserId = @UserId
              AND (@HasStatus = 0 OR Status = @StatusVal)
            ORDER BY CreatedAtUtc DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;
        command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });
        command.Parameters.Add(new SqlParameter("@HasStatus", SqlDbType.Bit) { Value = status.HasValue });
        command.Parameters.Add(new SqlParameter("@StatusVal", SqlDbType.TinyInt)
        {
            Value = status.HasValue ? (byte)status.Value : (object)DBNull.Value,
        });
        command.Parameters.Add(new SqlParameter("@Offset", SqlDbType.Int) { Value = offset });
        command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        var list = new List<TaskItem>();
        while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            list.Add(Map(reader));

        return (list, total);
    }

    public async Task<TaskItem> CreateAsync(TaskItem task, CancellationToken cancellationToken)
    {
        await using var connection = _connections.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO TaskItems (Id, UserId, Title, Description, Status, DueDateUtc, CreatedAtUtc, UpdatedAtUtc)
            VALUES (@Id, @UserId, @Title, @Description, @Status, @DueDateUtc, @CreatedAtUtc, @UpdatedAtUtc);
            """;
        AddTaskParameters(command, task);

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return task;
    }

    public async Task<bool> UpdateAsync(TaskItem task, CancellationToken cancellationToken)
    {
        await using var connection = _connections.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE TaskItems
            SET Title = @Title,
                Description = @Description,
                Status = @Status,
                DueDateUtc = @DueDateUtc,
                UpdatedAtUtc = @UpdatedAtUtc
            WHERE Id = @Id AND UserId = @UserId;
            """;
        AddTaskParameters(command, task);

        var affected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(Guid taskId, Guid userId, CancellationToken cancellationToken)
    {
        await using var connection = _connections.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            DELETE FROM TaskItems
            WHERE Id = @Id AND UserId = @UserId;
            """;
        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = taskId });
        command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = userId });

        var affected = await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return affected > 0;
    }

    private static void AddTaskParameters(SqlCommand command, TaskItem task)
    {
        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = task.Id });
        command.Parameters.Add(new SqlParameter("@UserId", SqlDbType.UniqueIdentifier) { Value = task.UserId });
        command.Parameters.Add(new SqlParameter("@Title", SqlDbType.NVarChar, 500) { Value = task.Title });
        command.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar, -1)
        {
            Value = task.Description ?? (object)DBNull.Value,
        });
        command.Parameters.Add(new SqlParameter("@Status", SqlDbType.TinyInt) { Value = (byte)task.Status });
        command.Parameters.Add(new SqlParameter("@DueDateUtc", SqlDbType.DateTime2)
        {
            Value = task.DueDateUtc.HasValue ? task.DueDateUtc.Value : DBNull.Value,
        });
        command.Parameters.Add(new SqlParameter("@CreatedAtUtc", SqlDbType.DateTime2) { Value = task.CreatedAtUtc });
        command.Parameters.Add(new SqlParameter("@UpdatedAtUtc", SqlDbType.DateTime2) { Value = task.UpdatedAtUtc });
    }

    private static TaskItem Map(SqlDataReader reader)
    {
        var descOrdinal = reader.GetOrdinal("Description");
        var dueOrdinal = reader.GetOrdinal("DueDateUtc");

        return new TaskItem
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            UserId = reader.GetGuid(reader.GetOrdinal("UserId")),
            Title = reader.GetString(reader.GetOrdinal("Title")),
            Description = reader.IsDBNull(descOrdinal) ? null : reader.GetString(descOrdinal),
            Status = (TaskItemStatus)reader.GetByte(reader.GetOrdinal("Status")),
            DueDateUtc = reader.IsDBNull(dueOrdinal) ? null : reader.GetDateTime(dueOrdinal),
            CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc")),
            UpdatedAtUtc = reader.GetDateTime(reader.GetOrdinal("UpdatedAtUtc")),
        };
    }
}
