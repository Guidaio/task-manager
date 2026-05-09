using Microsoft.Data.SqlClient;

namespace TaskManager.Infrastructure.Data;

public interface IDbConnectionFactory
{
    SqlConnection CreateConnection();
}
