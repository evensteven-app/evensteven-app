using System.Data.Common;
using Microsoft.Data.Sqlite;

namespace EvenSteven.Infrastructure.Storage.ConnectionFactory
{
    public class SqliteConnectionFactory(string dbPath) : IDbConnectionFactory
    {
        public DbConnection CreateConnection()
        {
            return new SqliteConnection(dbPath);
        }

        public async Task<DbConnection> CreateOpenedConnectionAsync(CancellationToken ct)
        {
            var connection = CreateConnection();

            await connection.OpenAsync(ct);

            return connection;
        }
    }
}
