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
        public IExpenseRepository ExpenseRepository;
        public IParticipantRepository ParticipantRepository;

        public SqliteClassFixture()
        {
            Database = TestDatabase.Create();

            var connectionFactory = new SqliteConnectionFactory(Database.ConnectionString);

            ConnectionFactory = connectionFactory;
            RoomRepository = new SqliteRoomRepository(connectionFactory, new FakeLogger<SqliteRoomRepository>());
            ExpenseRepository = new SqliteExpenseRepository(connectionFactory, new FakeLogger<SqliteExpenseRepository>());
            ParticipantRepository = new SqliteParticipantRepository(connectionFactory, new FakeLogger<SqliteParticipantRepository>());
        }

        public ValueTask DisposeAsync()
        {
            GC.SuppressFinalize(this);

           return Database.DisposeAsync();
        }
    }
}
