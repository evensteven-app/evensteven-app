using Dapper;
using EvenSteven.Infrastructure.Storage.ConnectionFactory;
using EvenSteven.Infrastructure.Storage.Repositories;
using EvenSteven.Shared.Models;
using Microsoft.Extensions.Logging.Testing;
using System.Data.Common;

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
                new (new Guid("00000000-0000-0000-0000-000000000203"), "Test room 2", new Guid("00000000-0000-0000-0000-000000000204"), "BBCCDDEE", "PasswordHash 2", 1, DateTime.UtcNow),
                new (new Guid("00000000-0000-0000-0000-000000000205"), "Test room 3", new Guid("00000000-0000-0000-0000-000000000206"), "CCDDEEFF", "PasswordHash 3", 1, DateTime.UtcNow)
            ];

            _participants = [
                new (new Guid("00000000-0000-0000-0000-000000000211"), _rooms[0].Id, "Test user 1", "DAIF12RE", 2500),
                new (new Guid("00000000-0000-0000-0000-000000000212"), _rooms[0].Id, "Test user 2", "DAUF13RU", -2500),
                new (new Guid("00000000-0000-0000-0000-000000000213"), _rooms[1].Id, "Test user 3", "DAIRDAAR", 0),
                new (new Guid("00000000-0000-0000-0000-000000000214"), _rooms[1].Id, "Test user 4", "DAXAFA01", 0),
                new (new Guid("00000000-0000-0000-0000-000000000215"), _rooms[2].Id, "Test user 5", "DAXBFA02", 0)
            ];

            _expenses = [
                new (new Guid("00000000-0000-0000-0000-000000000221"), _rooms[0].Id, 2000, "Pizza", _participants[0].Id, false, null, DateTime.UtcNow.AddDays(-1)),
                new (new Guid("00000000-0000-0000-0000-000000000222"), _rooms[1].Id, 5000, "Reverted trip", _participants[2].Id, true,
                    DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(-2)),
                new (new Guid("00000000-0000-0000-0000-000000000223"), _rooms[0].Id, 3000, "Groceries", _participants[0].Id, false, null, DateTime.UtcNow.AddDays(-1)),
                new (new Guid("00000000-0000-0000-0000-000000000224"), _rooms[1].Id, 1000, "Coffee", _participants[2].Id, false, null, DateTime.UtcNow.AddDays(-1)),
                new (new Guid("00000000-0000-0000-0000-000000000225"), _rooms[2].Id, 2000, "Reverted dinner", _participants[4].Id, true,
                    DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(-2))
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
        public async Task AddParticipant_ValidParameters_ReturnNewParticipantId()
        {
            var newParticipant = new Participant(Guid.Empty, _rooms[0].Id, "New user", "NEWKEY", 0);
            var participantId = await fixture.ParticipantRepository.AddParticipantAsync(newParticipant, TestContext.Current.CancellationToken);

            var participants = await fixture.ParticipantRepository.GetParticipantsByRoomAsync(_rooms[0].Id, TestContext.Current.CancellationToken);
            var created = participants.Single(p => p.Id == participantId);

            Assert.NotEqual(Guid.Empty, participantId);
            Assert.Equal(newParticipant with { Id = participantId }, created);
        }

        [Fact]
        public async Task AddParticipant_CloseCancellationToken_ThrowsTaskCancelledException()
        {
            CancellationTokenSource token = new();

            await token.CancelAsync();

            var newParticipant = new Participant(Guid.Empty, _rooms[0].Id, "New user", "NEWKEY", 0);

            await Assert.ThrowsAsync<TaskCanceledException>(() =>
                fixture.ParticipantRepository.AddParticipantAsync(newParticipant, token.Token));
        }

        [Fact]
        public async Task AddParticipant_InvalidDbState_ThrowsDbException()
        {
            var poisonFactory = new SqliteConnectionFactory("Data Source=:memory:;Mode=ReadOnly;");

            var isolatedRepo = new SqliteParticipantRepository(poisonFactory, new FakeLogger());

            var newParticipant = new Participant(Guid.Empty, _rooms[0].Id, "New user", "NEWKEY", 0);

            await Assert.ThrowsAnyAsync<DbException>(() =>
                isolatedRepo.AddParticipantAsync(newParticipant, TestContext.Current.CancellationToken));
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
        public async Task GetParticipants_AllExpensesReverted_BalancesAreZero()
        {
            var participants =
                await fixture.ParticipantRepository.GetParticipantsByRoomAsync(_rooms[2].Id, TestContext.Current.CancellationToken);

            Assert.NotEmpty(participants);
            Assert.All(participants, p => Assert.Equal(0, p.Balance));
        }

        [Fact]
        public async Task GetParticipants_PayerWithMultipleExpenses_AggregatesBalance()
        {
            var participants =
                await fixture.ParticipantRepository.GetParticipantsByRoomAsync(_rooms[0].Id, TestContext.Current.CancellationToken);

            var payer = participants.Single(p => p.Id == _participants[0].Id);

            Assert.Equal(2500, payer.Balance);
        }

        [Fact]
        public async Task GetParticipants_OnlyShares_NegativeBalance()
        {
            var participants =
                await fixture.ParticipantRepository.GetParticipantsByRoomAsync(_rooms[0].Id, TestContext.Current.CancellationToken);

            var sharedParticipant = participants.Single(p => p.Id == _participants[1].Id);

            Assert.Equal(-2500, sharedParticipant.Balance);
        }

        [Fact]
        public async Task GetParticipants_InvalidRoomId_ReturnsEmptyList()
        {
            var participants =
                await fixture.ParticipantRepository.GetParticipantsByRoomAsync(new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), TestContext.Current.CancellationToken);

            Assert.Empty(participants);
        }

        [Fact]
        public async Task GetParticipants_CloseCancellationToken_ThrowsTaskCancelledException()
        {
            CancellationTokenSource token = new();

            await token.CancelAsync();

            await Assert.ThrowsAsync<TaskCanceledException>(() =>
                fixture.ParticipantRepository.GetParticipantsByRoomAsync(_rooms[0].Id, token.Token));
        }

        [Fact]
        public async Task GetParticipants_InvalidDbState_ThrowsDbException()
        {
            var poisonFactory = new SqliteConnectionFactory("Data Source=:memory:;Mode=ReadOnly;");

            var isolatedRepo = new SqliteParticipantRepository(poisonFactory, new FakeLogger());

            await Assert.ThrowsAnyAsync<DbException>(() =>
                isolatedRepo.GetParticipantsByRoomAsync(_rooms[0].Id, TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task DeleteParticipant_ValidParticipantId_RemovesParticipant()
        {
            var participantsBefore = await fixture.ParticipantRepository.GetParticipantsByRoomAsync(_rooms[0].Id, TestContext.Current.CancellationToken);

            Assert.Contains(participantsBefore, p => p.Id == _participants[0].Id);

            await fixture.ParticipantRepository.DeleteParticipantAsync(_participants[0].Id, TestContext.Current.CancellationToken);

            var participantsAfter = await fixture.ParticipantRepository.GetParticipantsByRoomAsync(_rooms[0].Id, TestContext.Current.CancellationToken);

            Assert.DoesNotContain(participantsAfter, p => p.Id == _participants[0].Id);
        }

        [Fact]
        public async Task DeleteParticipant_NonExistingParticipant_DoesNotThrow()
        {
            var exception = await Record.ExceptionAsync(() =>
                fixture.ParticipantRepository.DeleteParticipantAsync(new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), TestContext.Current.CancellationToken));

            Assert.Null(exception);
        }

        [Fact]
        public async Task DeleteParticipant_CloseCancellationToken_ThrowsTaskCancelledException()
        {
            CancellationTokenSource token = new();

            await token.CancelAsync();

            await Assert.ThrowsAsync<TaskCanceledException>(() =>
                fixture.ParticipantRepository.DeleteParticipantAsync(_participants[0].Id, token.Token));
        }

        [Fact]
        public async Task DeleteParticipant_InvalidDbState_ThrowsDbException()
        {
            var poisonFactory = new SqliteConnectionFactory("Data Source=:memory:;Mode=ReadOnly;");

            var isolatedRepo = new SqliteParticipantRepository(poisonFactory, new FakeLogger());

            await Assert.ThrowsAnyAsync<DbException>(() =>
                isolatedRepo.DeleteParticipantAsync(Guid.NewGuid(), TestContext.Current.CancellationToken));
        }

        [Fact]
        public async Task UpdateParticipantName_ValidParameters_RenamesParticipant()
        {
            await fixture.ParticipantRepository.UpdateParticipantNameAsync(_participants[0].Id, "Renamed user", TestContext.Current.CancellationToken);

            var participants = await fixture.ParticipantRepository.GetParticipantsByRoomAsync(_rooms[0].Id, TestContext.Current.CancellationToken);
            var updated = participants.Single(p => p.Id == _participants[0].Id);

            Assert.Equal("Renamed user", updated.Name);
            Assert.Equal(_participants[0].Balance, updated.Balance);
        }

        [Fact]
        public async Task UpdateParticipantName_NonExistingParticipant_DoesNotThrow()
        {
            var exception = await Record.ExceptionAsync(() =>
                fixture.ParticipantRepository.UpdateParticipantNameAsync(new Guid("ffffffff-ffff-ffff-ffff-ffffffffffff"), "New name", TestContext.Current.CancellationToken));

            Assert.Null(exception);
        }

        [Fact]
        public async Task UpdateParticipantName_CloseCancellationToken_ThrowsTaskCancelledException()
        {
            CancellationTokenSource token = new();

            await token.CancelAsync();

            await Assert.ThrowsAsync<TaskCanceledException>(() =>
                fixture.ParticipantRepository.UpdateParticipantNameAsync(_participants[0].Id, "New name", token.Token));
        }

        [Fact]
        public async Task UpdateParticipantName_InvalidDbState_ThrowsDbException()
        {
            var poisonFactory = new SqliteConnectionFactory("Data Source=:memory:;Mode=ReadOnly;");

            var isolatedRepo = new SqliteParticipantRepository(poisonFactory, new FakeLogger());

            await Assert.ThrowsAnyAsync<DbException>(() =>
                isolatedRepo.UpdateParticipantNameAsync(Guid.NewGuid(), "New name", TestContext.Current.CancellationToken));
        }
    }
}