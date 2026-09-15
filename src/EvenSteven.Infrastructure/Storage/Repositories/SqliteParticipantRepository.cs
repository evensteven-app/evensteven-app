using Dapper;
using EvenSteven.Infrastructure.Storage.ConnectionFactory;
using EvenSteven.Infrastructure.Storage.Repositories.Interfaces;
using EvenSteven.Shared.Models;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace EvenSteven.Infrastructure.Storage.Repositories
{
    internal class SqliteParticipantRepository : IParticipantRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ILogger<SqliteParticipantRepository> _logger;

        public SqliteParticipantRepository(IDbConnectionFactory connectionFactory, ILogger<SqliteParticipantRepository> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task<Guid> AddParticipantAsync(Participant participant, CancellationToken cancellationToken)
        {
            await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

            string commandText = """
                                 INSERT INTO Participants (Id, RoomId, Name, ParticipantKey)
                                     VALUES (@Id, @RoomId, @Name, @ParticipantKey)
                             """;

            var participantId = Guid.NewGuid();
            var command = new CommandDefinition(
                commandText,
                new
                {
                    Id = participantId,
                    participant.RoomId,
                    participant.Name,
                    participant.ParticipantKey
                },
                cancellationToken: cancellationToken);
            try
            {
                await connection.ExecuteAsync(command);
            }
            catch (DbException ex)
            {
                _logger.LogError(ex, "Error occurred while creating new participant in room: {roomId}", participant.RoomId);
                throw;
            }

            return participantId;
        }

        public async Task DeleteParticipantAsync(Guid participantId, CancellationToken cancellationToken)
        {
            await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

            string commandText = """
                                 DELETE FROM Participants
                                     WHERE Id = @Id;
                             """;

            var command = new CommandDefinition(commandText, new { Id = participantId }, cancellationToken: cancellationToken);

            try
            {
                await connection.ExecuteAsync(command);
            }
            catch (DbException ex)
            {
                _logger.LogError(ex, "Error occurred while deleting participant with id: {participantId}", participantId);
                throw;
            }
        }

        public async Task<List<Participant>> GetParticipantsByRoomAsync(Guid roomId, CancellationToken cancellationToken)
        {
            await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

            string commandText = """
                                 WITH Shared AS (
                                 SELECT ParticipantId,
                                        SUM(Share) AS TotalShare
                                 FROM ExpenseEntries
                                 GROUP BY ParticipantId
                                 ), Paid AS (
                                     SELECT PayerId,
                                            SUM(Amount) AS TotalPaid
                                     FROM Expenses
                                     WHERE IsReverted = FALSE
                                     GROUP BY PayerId
                                 )
                                 SELECT p.Id,
                                        p.RoomId,
                                        p.Name,
                                        p.ParticipantKey,
                                        (COALESCE(Paid.TotalPaid, 0) - COALESCE(Shared.TotalShare, 0)) AS Balance
                                 FROM Participants AS p
                                          LEFT JOIN Shared ON Shared.ParticipantId = p.Id
                                          LEFT JOIN Paid ON Paid.PayerId = p.Id
                                 WHERE p.RoomId = @RoomId
                                 GROUP BY p.Id
                                 ORDER BY p.Id;
                             """;

            var command = new CommandDefinition(commandText, new { RoomId = roomId }, cancellationToken: cancellationToken);

            try
            {
                return [.. await connection.QueryAsync<Participant>(command)];
            }
            catch (DbException ex)
            {
                _logger.LogError(ex, "Error occurred while getting participants in room: {roomId}", roomId);
                throw;
            }
        }

        public async Task UpdateParticipantNameAsync(Guid participantId, string newName, CancellationToken cancellationToken)
        {
            await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

            string commandText = """
                                     UPDATE Participants
                                        SET Name = @Name
                                        WHERE Id = @Id;
                                 """;

            var command = new CommandDefinition(commandText, new { Name = newName, Id = participantId },
                cancellationToken: cancellationToken);

            try
            {
                await connection.ExecuteAsync(command);
            }
            catch (DbException ex)
            {
                _logger.LogError(ex, "Error occurred while updating name to participant: {participantId}", participantId);
                throw;
            }
        }
    }
}