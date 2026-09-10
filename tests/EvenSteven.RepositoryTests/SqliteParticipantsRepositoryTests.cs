using Dapper;
using EvenSteven.Shared.Models;

namespace EvenSteven.RepositoryTests
{
    public class SqliteParticipantsRepositoryTests(SqliteClassFixture fixture) : IAsyncLifetime, IClassFixture<SqliteClassFixture>
    {
        private List<Room> _rooms;
        private List<Participant> _participants;
        private List<Expense> _expenses;
        private List<ExpenseEntry> _expenseEntries;

        public async ValueTask DisposeAsync()
        {
            await fixture.Database.ResetAsync();
        }

        public async ValueTask InitializeAsync()
        {
            _rooms = [
                new (new Guid("00000000-0000-0000-0000-000000000201"), "Test room 1", new Guid("00000000-0000-0000-0000-000000000202"), "AABBCCDD", "PasswordHash 1", 1, DateTime.UtcNow),
                new (new Guid("00000000-0000-0000-0000-000000000203"), "Test room 2", new Guid("00000000-0000-0000-0000-000000000204"), "BBCCDDEE", "PasswordHash 2", 1, DateTime.UtcNow)
            ];

            _participants = [
                new (new Guid("00000000-0000-0000-0000-000000000211"), _rooms[0].Id, "Test user 1", "DAIF12RE", 2500),
                new (new Guid("00000000-0000-0000-0000-000000000212"), _rooms[0].Id, "Test user 2", "DAUF13RU", -2500),
                new (new Guid("00000000-0000-0000-0000-000000000213"), _rooms[1].Id, "Test user 3", "DAIRDAAR", 0),
                new (new Guid("00000000-0000-0000-0000-000000000214"), _rooms[1].Id, "Test user 4", "DAXAFA01", 0)
            ];

            _expenses = [
                new (new Guid("00000000-0000-0000-0000-000000000221"), _rooms[0].Id, 2000, "Pizza", _participants[0].Id, false, null, DateTime.UtcNow.AddDays(-1)),
                new (new Guid("00000000-0000-0000-0000-000000000222"), _rooms[1].Id, 5000, "Reverted trip", _participants[2].Id, true,
                    DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(-2)),
                new (new Guid("00000000-0000-0000-0000-000000000223"), _rooms[0].Id, 3000, "Groceries", _participants[0].Id, false, null, DateTime.UtcNow.AddDays(-1)),
                new (new Guid("00000000-0000-0000-0000-000000000224"), _rooms[1].Id, 1000, "Coffee", _participants[2].Id, false, null, DateTime.UtcNow.AddDays(-1))
            ];

            _expenseEntries = [
                new (new Guid("00000000-0000-0000-0000-000000000231"), _expenses[0].Id, _participants[0].Id, 1000),
                new (new Guid("00000000-0000-0000-0000-000000000232"), _expenses[0].Id, _participants[1].Id, 1000),
                new (new Guid("00000000-0000-0000-0000-000000000233"), _expenses[2].Id, _participants[0].Id, 1500),
                new (new Guid("00000000-0000-0000-0000-000000000234"), _expenses[2].Id, _participants[1].Id, 1500),
                new (new Guid("00000000-0000-0000-0000-000000000235"), _expenses[3].Id, _participants[2].Id, 1000)
            ];

            await using var connection = await fixture.ConnectionFactory.CreateOpenConnectionAsync(TestContext.Current.CancellationToken);

            string roomCommand = """
                                     INSERT INTO Rooms (Id, Title, EditKey, InviteCode, PasswordHash, Version, CreatedAt) 
                                         VALUES (@Id, @Title, @EditKey, @InviteCode, @PasswordHash, @Version, @CreatedAt);
                                 """;

            string participantCommand = """
                                            INSERT INTO Participants (Id, RoomId, Name, ParticipantKey)
                                                VALUES (@Id, @RoomId, @Name, @ParticipantKey);
                                        """;

            string expenseCommand = """
                                        INSERT INTO Expenses (Id, RoomId, Amount, Note, PayerId, IsReverted, RevertedAt, CreatedAt)
                                            VALUES (@Id, @RoomId, @Amount, @Note, @PayerId, @IsReverted, @RevertedAt, @CreatedAt);
                                    """;

            string expenseEntryCommand = """
                                             INSERT INTO ExpenseEntries (Id, ExpenseId, ParticipantId, Share)
                                                 VALUES (@Id, @ExpenseId, @ParticipantId, @Share);
                                         """;

            await connection.ExecuteAsync(roomCommand, _rooms);
            await connection.ExecuteAsync(participantCommand, _participants);
            await connection.ExecuteAsync(expenseCommand, _expenses);
            await connection.ExecuteAsync(expenseEntryCommand, _expenseEntries);
        }

        [Fact]
        public async Task GetParticipants_ValidRoomId_ReturnParticipantsWithBalances()
        {
            var participants =
                await fixture.ParticipantRepository.GetParticipantsByRoomAsync(_rooms[0].Id, TestContext.Current.CancellationToken);

            Assert.Equal(
                _participants.Where(p => p.RoomId == _rooms[0].Id).OrderBy(p => p.Name),
                participants.OrderBy(p => p.Name));
        }

        [Fact]
        public async Task GetParticipants_ParticipantWithoutSpending_IsIncludedWithZeroBalance()
        {
            var participants =
                await fixture.ParticipantRepository.GetParticipantsByRoomAsync(_rooms[1].Id, TestContext.Current.CancellationToken);

            var idleParticipant = _participants[3];

            Assert.Contains(participants, p => p.Id == idleParticipant.Id);
            Assert.Equal(0, participants.Single(p => p.Id == idleParticipant.Id).Balance);
        }

        [Fact]
        public async Task GetParticipants_RevertedExpense_DoesNotCountTowardsPayerBalance()
        {
            var participants =
                await fixture.ParticipantRepository.GetParticipantsByRoomAsync(_rooms[1].Id, TestContext.Current.CancellationToken);

            var payer = participants.Single(p => p.Id == _participants[2].Id);

            Assert.Equal(0, payer.Balance);
        }

        [Fact]
        public async Task GetParticipants_InvalidRoomId_ReturnsEmptyList()
        {
            var participants =
                await fixture.ParticipantRepository.GetParticipantsByRoomAsync(new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), TestContext.Current.CancellationToken);

            Assert.Empty(participants);
        }
    }
}
