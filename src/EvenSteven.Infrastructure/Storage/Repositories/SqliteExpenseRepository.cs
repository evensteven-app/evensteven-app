using Dapper;
using EvenSteven.Infrastructure.Storage.ConnectionFactory;
using EvenSteven.Infrastructure.Storage.Repositories.Interfaces;
using EvenSteven.Infrastructure.Storage.Utils;
using EvenSteven.Shared.Models;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace EvenSteven.Infrastructure.Storage.Repositories
{
    public class SqliteExpenseRepository(IDbConnectionFactory connectionFactory, ILogger logger) : IExpenseRepository
    {
        public async Task AddExpenseAsync(Expense expense, List<Participant> participants, CancellationToken cancellationToken)
        {
            await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

            var transaction = await connection.BeginTransactionAsync(cancellationToken);

            string expenseCommand = """
                    INSERT INTO Expenses (Id, RoomId, Amount, Note, CreatedAt)
                        VALUES (@Id, @RoomId, @Amount, @Note, @CreatedAt);
                """;

            var expenseGuid = Guid.NewGuid();
            var distributions = ExpenseUtils.SplitAmount([.. participants.Select(p => p.Id)], expense.Amount);

            try
            {
                await connection.ExecuteAsync(expenseCommand, new
                {
                    Id = expenseGuid,
                    expense.RoomId,
                    expense.Amount,
                    expense.Note,
                    CreateAt = DateTime.UtcNow
                }, transaction);

                string expenseEntryCommand = """
                    INSERT INTO ExpenseEntries (Id, ExpenseId, ParticipantId, Share)
                        VALUES (@Id, @ExpenseId, @ParticipantId, @Share);
                """;

                foreach (var expenseEntry in distributions)
                {
                    await connection.ExecuteAsync(expenseEntryCommand, new
                    {
                        Id = Guid.NewGuid(),
                        ExpenseId = expenseGuid,
                        ParticipantId = expenseEntry.Key,
                        Share = expenseEntry.Value,
                    }, transaction);
                }

                transaction.Commit();
            }
            catch (DbException ex)
            {
                transaction.Rollback();

                logger.LogError(ex, "Error occured while creating new expense with id: {expeseId}", expenseGuid);
                throw;
            }
        }

        public Task<Expense> GetExspenseByRoomAsync(Guid roomId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task RevertExpenseAsync(Guid eventId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
