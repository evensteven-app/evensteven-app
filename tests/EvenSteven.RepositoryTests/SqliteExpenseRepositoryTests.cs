using Dapper;
using EvenSteven.Shared.Models;

namespace EvenSteven.RepositoryTests
{
    public class SqliteExpenseRepositoryTests(SqliteClassFixture fixture) : IAsyncLifetime, IClassFixture<SqliteClassFixture>
    {
        private List<Room> _rooms;
        private List<Participant> _participants;
        private List<Expense> _expenses;

        private List<ExpenseEntry> _expenseEntries;

        private async Task<List<ExpenseEntry>> GetExpenseEntriesByExpenseId(Guid expenseId)
        {
            await using var connection = await fixture.ConnectionFactory.CreateOpenConnectionAsync(TestContext.Current.CancellationToken);

            string command = """
                SELECT Id, ExpenseId, ParticipantId, Share
                    FROM ExpenseEntries
                    WHERE Id = @ExpenseId;
            """;

            return [.. await connection.QueryAsync<ExpenseEntry>(command, new { ExpenseId = expenseId })];
        }

        private async Task<List<Expense>> GetAllExpenses()
        {
            await using var connection = await fixture.ConnectionFactory.CreateOpenConnectionAsync(TestContext.Current.CancellationToken);

            string command = """
                SELECT Id, RoomId, Amount, Note, PayerId, IsReverted, RevertedAt, CreatedAt
                    FROM Expenses;
            """;

            return [.. await connection.QueryAsync<Expense>(command)];
        }

        private async Task<List<ExpenseEntry>> GetAllExpenseEntries()
        {
            await using var connection = await fixture.ConnectionFactory.CreateOpenConnectionAsync(TestContext.Current.CancellationToken);

            string command = """
                SELECT Id, ExpenseId, ParticipantId, Share
                    FROM ExpenseEntries;
            """;

            return [.. await connection.QueryAsync<ExpenseEntry>(command)];
        }

        public async ValueTask DisposeAsync()
        {
            await fixture.Database.ResetAsync();
        }

        public async ValueTask InitializeAsync()
        {
            _rooms = [
                new (Guid.NewGuid(), "Test room 1", Guid.NewGuid(), "AABBCCDD", "PasswordHash 1", 1, DateTime.UtcNow),
                new (Guid.NewGuid(), "Test room 2", Guid.NewGuid(), "BBCCDDEE", "PasswordHash 2", 1, DateTime.UtcNow)
            ];

            _participants = [
                new (Guid.NewGuid(), _rooms[0].Id, "Test user 1", "DAIF12RE", -1000),
                new (Guid.NewGuid(), _rooms[0].Id, "Test user 2", "DAUF13RU", 1000),
                new (Guid.NewGuid(), _rooms[1].Id, "Test user 3", "DAIRDAAR", 0)
            ];

            _expenses = [
                new (Guid.NewGuid(), _rooms[0].Id, 2000, "Test note", _participants[0].Id, false, null, DateTime.UtcNow.AddDays(-1)),
                new (Guid.NewGuid(), _rooms[1].Id, 5000, "Test note 2", _participants[2].Id, true,
                    DateTime.UtcNow.AddDays(-1), DateTime.UtcNow.AddDays(-2))
            ];

            _expenseEntries = [
                new (Guid.NewGuid(), _expenses[0].Id, _participants[0].Id, 1000),
                new (Guid.NewGuid(), _expenses[0].Id, _participants[1].Id, 1000),
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
        public async Task AddExpense_ValidData_CreateExpenseAndExpenseEntries()
        {
            var newExpense = new Expense(Guid.NewGuid(), _rooms[1].Id, 1000, "Test new note", _participants[2].Id,
                false, null, DateTime.UtcNow);

            await fixture.ExpenseRepository.AddExpenseAsync(newExpense, [.. _participants.Where(p => p.RoomId == _rooms[1].Id)], TestContext.Current.CancellationToken);

            var expenses = (await fixture.ExpenseRepository
                .GetExspensesByRoomAsync(_rooms[1].Id, TestContext.Current.CancellationToken))
                .FirstOrDefault(ex => ex.Note == newExpense.Note);
            var expenseEntries = await GetAllExpenseEntries();

            Assert.NotNull(expenses);
            Assert.Equal(newExpense.RoomId, expenses.RoomId);
            Assert.Equal(newExpense.Amount, expenses.Amount);
            Assert.Equal(newExpense.Note, expenses.Note);
            Assert.Equal(newExpense.PayerId, expenses.PayerId);
            Assert.True(expenses.CreatedAt != default);
            Assert.Single(expenseEntries, x => x.ExpenseId == expenses.Id);
        }

        [Fact]
        public async Task AddExpense_ValidData_SplitAmountAmongParticipants()
        {
            var newExpense = new Expense(Guid.NewGuid(), _rooms[0].Id, 1000, "Test split note", _participants[0].Id,
                false, null, DateTime.UtcNow);

            var roomParticipants = _participants.Where(p => p.RoomId == _rooms[0].Id).ToList();

            await fixture.ExpenseRepository.AddExpenseAsync(newExpense, roomParticipants, TestContext.Current.CancellationToken);

            var expense = (await fixture.ExpenseRepository
                .GetExspensesByRoomAsync(_rooms[0].Id, TestContext.Current.CancellationToken))
                .FirstOrDefault(ex => ex.Note == newExpense.Note);

            Assert.NotNull(expense);

            var expenseEntries = (await GetAllExpenseEntries())
                .Where(x => x.ExpenseId == expense.Id)
                .ToList();

            Assert.Equal(roomParticipants.Count, expenseEntries.Count);
            Assert.Equal(newExpense.Amount, expenseEntries.Sum(x => x.Share));
            Assert.Equal(
                roomParticipants.Select(p => p.Id).OrderBy(id => id),
                expenseEntries.Select(x => x.ParticipantId).OrderBy(id => id));
        }

        [Fact]
        public async Task GetExpenses_ValidId_ReturnAllExpensesForGivenRoom()
        {
            var expenses = await fixture.ExpenseRepository.GetExspensesByRoomAsync(_rooms[0].Id, TestContext.Current.CancellationToken);

            Assert.Equal(_expenses.Where(ex => ex.RoomId == _rooms[0].Id), expenses);
        }

        [Fact]
        public async Task GetExpenses_UnknownId_ReturnEmptyList()
        {
            var expenses = await fixture.ExpenseRepository.GetExspensesByRoomAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

            Assert.Empty(expenses);
        }

        [Fact]
        public async Task RevertExpense_ValidExpenseId_SetExpenseAsRevertedAndDeleteAllExpenseEntries()
        {
            await fixture.ExpenseRepository.RevertExpenseAsync(_expenses[0].Id, TestContext.Current.CancellationToken);

            var expense = (await fixture.ExpenseRepository
                .GetExspensesByRoomAsync(_rooms[0].Id, TestContext.Current.CancellationToken))
                .FirstOrDefault(ex => ex.Id == _expenses[0].Id);
            var expenseEntries = await GetExpenseEntriesByExpenseId(_expenses[0].Id);

            Assert.NotNull(expense);
            Assert.True(expense.IsReverted);
            Assert.NotNull(expense.RevertedAt);
            Assert.True(DateTime.Compare((DateTime)expense.RevertedAt, DateTime.UtcNow) < 0);
            Assert.DoesNotContain(
                expenseEntries,
                x => x.ExpenseId == _expenses[0].Id);
        }

        [Fact]
        public async Task RevertExpense_InvalidExpenseId_DoNothing()
        {
            await fixture.ExpenseRepository.RevertExpenseAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

            var allExpenses = await GetAllExpenses();
            var allExpenseEntries = await GetAllExpenseEntries();

            Assert.Equal(_expenses, allExpenses);
            Assert.Equal(_expenseEntries, allExpenseEntries);
        }
    }
}
