using Dapper;
using EvenSteven.Infrastructure.Storage.ConnectionFactory;
using EvenSteven.Infrastructure.Storage.Repositories.Interfaces;
using EvenSteven.Infrastructure.Storage.Utils;
using EvenSteven.Shared.Models;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace EvenSteven.Infrastructure.Storage.Repositories
{
    public class SqliteExpenseRepository(IDbConnectionFactory connectionFactory, ILogger logger) : IExpenseRepository
    {
        public async Task AddExpenseAsync(Expense expense, List<Participant> participants, CancellationToken cancellationToken)
        {
            await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

            var expenseGuid = Guid.NewGuid();
            var distributions = ExpenseUtils.SplitAmount([.. participants.Select(p => p.Id)], expense.Amount);

            string expenseCommandText = """
                                        INSERT INTO Expenses (Id, RoomId, Amount, Note, PayerId, CreatedAt)
                                            VALUES (@Id, @RoomId, @Amount, @Note, @PayerId, @CreatedAt);
                                    """;
            string expenseEntryCommandText = """
                                                 INSERT INTO ExpenseEntries (Id, ExpenseId, ParticipantId, Share)
                                                     VALUES (@Id, @ExpenseId, @ParticipantId, @Share);
                                             """;

            var parameters = distributions.Select(d =>
                new
                {
                    Id = Guid.NewGuid(),
                    ExpenseId = expenseGuid,
                    ParticipantId = d.Key,
                    Share = d.Value,
                }
            );

            var transaction = await connection.BeginTransactionAsync(cancellationToken);
            var expenseCommand = new CommandDefinition(expenseCommandText, new
            {
                Id = expenseGuid,
                expense.RoomId,
                expense.Amount,
                expense.Note,
                expense.PayerId,
                CreatedAt = DateTime.UtcNow
            }, transaction, cancellationToken: cancellationToken);

            var expenseEntryCommandDefinition = new CommandDefinition(
                expenseEntryCommandText,
                parameters,
                transaction,
                cancellationToken: cancellationToken);

            try
            {
                await connection.ExecuteAsync(expenseCommand);

                await connection.ExecuteAsync(expenseEntryCommandDefinition);
            }
            catch (DbException ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                logger.LogError(ex, "Error occured while creating new expense with id: {expenseId}", expenseGuid);
                throw;
            }

            await transaction.CommitAsync(cancellationToken);
        }

        public async Task<List<Expense>> GetExspensesByRoomAsync(Guid roomId, CancellationToken cancellationToken)
        {
            await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

            string commandText = """
                                 SELECT Id, RoomId, Amount, Note, PayerId, IsReverted, RevertedAt, CreatedAt
                                     FROM Expenses
                                     WHERE RoomId = @RoomId;
                             """;

            try
            {
                var command = new CommandDefinition(commandText, new { RoomId = roomId }, cancellationToken: cancellationToken);
                return [.. (await connection.QueryAsync<Expense>(command))];
            }
            catch (DbException ex)
            {
                logger.LogError(ex, "Error occured while getting room expenses with roomId: {roomId}", roomId);
                throw;
            }
        }

        public async Task RevertExpenseAsync(Guid expenseId, CancellationToken cancellationToken)
        {
            await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

            var transaction = await connection.BeginTransactionAsync(cancellationToken);

            string revertExpenseCommandText = """
                                              UPDATE Expenses
                                                  SET IsReverted = 1,
                                                      RevertedAt = @RevertedAt
                                                  WHERE Id = @ExpenseId;
                                          """;
            string deleteExpenseEntryCommandText = """
                                                   DELETE FROM ExpenseEntries
                                                       WHERE ExpenseId = @ExpenseId;
                                               """;

            var revertCommandDefinition = new CommandDefinition(revertExpenseCommandText, new
            {
                RevertedAt = DateTime.UtcNow,
                ExpenseId = expenseId
            }, transaction, cancellationToken: cancellationToken);
            var deleteCommandDefinition = new CommandDefinition(deleteExpenseEntryCommandText, new
            { ExpenseId = expenseId }, transaction, cancellationToken: cancellationToken);

            try
            {
                await connection.ExecuteAsync(revertCommandDefinition);

                await connection.ExecuteAsync(deleteCommandDefinition);
            }
            catch (DbException ex)
            {
                await transaction.RollbackAsync(cancellationToken);

                logger.LogError(ex, "Error occured while reverting expense with id: {expenseId}", expenseId);
                throw;
            }

            await transaction.CommitAsync(cancellationToken);
        }
    }
}
