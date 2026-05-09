using System.Data;
using Microsoft.Data.SqlClient;
using TaskManager.Application.Abstractions;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Infrastructure.Persistence;

public sealed class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connections;

    public UserRepository(IDbConnectionFactory connections)
    {
        _connections = connections;
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        await using var connection = _connections.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Name, Email, PasswordHash, CreatedAtUtc
            FROM Users
            WHERE Email = @Email;
            """;
        command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 320) { Value = email });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            return null;

        return Map(reader);
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        await using var connection = _connections.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Name, Email, PasswordHash, CreatedAtUtc
            FROM Users
            WHERE Id = @Id;
            """;
        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = id });

        await using var reader = await command.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);
        if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
            return null;

        return Map(reader);
    }

    public async Task<User> CreateAsync(User user, CancellationToken cancellationToken)
    {
        await using var connection = _connections.CreateConnection();
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Users (Id, Name, Email, PasswordHash, CreatedAtUtc)
            VALUES (@Id, @Name, @Email, @PasswordHash, @CreatedAtUtc);
            """;
        command.Parameters.Add(new SqlParameter("@Id", SqlDbType.UniqueIdentifier) { Value = user.Id });
        command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar, 200) { Value = user.Name });
        command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar, 320) { Value = user.Email });
        command.Parameters.Add(new SqlParameter("@PasswordHash", SqlDbType.NVarChar, -1) { Value = user.PasswordHash });
        command.Parameters.Add(new SqlParameter("@CreatedAtUtc", SqlDbType.DateTime2) { Value = user.CreatedAtUtc });

        await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
        return user;
    }

    private static User Map(SqlDataReader reader)
    {
        return new User
        {
            Id = reader.GetGuid(reader.GetOrdinal("Id")),
            Name = reader.GetString(reader.GetOrdinal("Name")),
            Email = reader.GetString(reader.GetOrdinal("Email")),
            PasswordHash = reader.GetString(reader.GetOrdinal("PasswordHash")),
            CreatedAtUtc = reader.GetDateTime(reader.GetOrdinal("CreatedAtUtc")),
        };
    }
}
