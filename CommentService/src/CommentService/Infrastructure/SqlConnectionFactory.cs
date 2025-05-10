using Npgsql;
using SachkovTech.Core.Database;
using System.Data;

namespace CommentService.Infrastructure;

public class SqlConnectionFactory(IConfiguration configuration) : ISqlConnectionFactory
{
    public IDbConnection Create() =>
        new NpgsqlConnection(configuration.GetConnectionString("Database"));
}
