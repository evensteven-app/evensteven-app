using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System.Data.Common;

namespace EvenSteven.Infrastructure.Storage.ConnectionFactory
{
    public class SqliteConnectionFactory : IDbConnectionFactory
    {
        private readonly string? _connectionString;


        public SqliteConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }
        public SqliteConnectionFactory(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("db-connection");
        }

        public DbConnection CreateConnection()
        {
            if (_connectionString == null) {
                throw new InvalidOperationException("Connection string 'db-connection' is not configured.");
            }
            return new SqliteConnection(_connectionString);
        }

        public async Task<DbConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken)
        {
            var connection = CreateConnection();
            await connection.OpenAsync(cancellationToken);
            return connection;
        }
    }
}
