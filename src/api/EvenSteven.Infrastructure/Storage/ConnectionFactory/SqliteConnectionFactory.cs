using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;

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
