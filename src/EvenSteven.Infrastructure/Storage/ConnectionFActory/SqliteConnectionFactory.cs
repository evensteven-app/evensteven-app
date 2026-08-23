using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace EvenSteven.Infrastructure.Storage.ConnectionFActory
{
    public class SqliteConnectionFactory(IConfigurationManager manager) : IDbConnectionFactory
    {
        private readonly string? _connectionString = manager.GetConnectionString("db-connection");
        public IDbConnection CreateConnection()
        {
            if (_connectionString == null) {
                throw new ArgumentException("failed to get connection string from appsetting");
            }
            return new SqliteConnection(_connectionString);
        }
    }
}
