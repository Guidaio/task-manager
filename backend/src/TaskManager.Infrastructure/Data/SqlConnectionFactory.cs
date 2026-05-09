using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using TaskManager.Infrastructure.Options;

namespace TaskManager.Infrastructure.Data;

public sealed class SqlConnectionFactory : IDbConnectionFactory
{
    private readonly DatabaseOptions _options;

    public SqlConnectionFactory(IOptions<DatabaseOptions> options)
    {
        _options = options.Value;
    }

    public SqlConnection CreateConnection()
    {
        if (string.IsNullOrWhiteSpace(_options.ConnectionString))
            throw new InvalidOperationException("Database:ConnectionString is not configured.");

        return new SqlConnection(_options.ConnectionString);
    }
}
