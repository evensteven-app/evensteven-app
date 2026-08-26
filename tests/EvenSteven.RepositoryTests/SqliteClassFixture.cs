using EvenSteven.Infrastructure.Storage.ConnectionFactory;
using EvenSteven.Infrastructure.Storage.Repositories;
using EvenSteven.Infrastructure.Storage.Repositories.Interfaces;
using Microsoft.Extensions.Logging.Testing;

namespace EvenSteven.RepositoryTests
{
    public class SqliteClassFixture : IAsyncDisposable
    {
        public TestDatabase Database;
        public IDbConnectionFactory ConnectionFactory;
        public IRoomRepository RoomRepository;

        public SqliteClassFixture()
        {
            Database = TestDatabase.Create();

            var connectionFactory = new SqliteConnectionFactory(Database.ConnectionString);
            var logger = new FakeLogger<SqliteRoomRepository>();

            ConnectionFactory = connectionFactory;
            RoomRepository = new SqliteRoomRepository(connectionFactory, logger);
        }

        public ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);

           return Database.DisposeAsync();
        }
    }
}
