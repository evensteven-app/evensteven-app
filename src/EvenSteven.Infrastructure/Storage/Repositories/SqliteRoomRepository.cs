using Dapper;
using EvenSteven.Infrastructure.Storage.ConnectionFactory;
using EvenSteven.Infrastructure.Storage.Repositories.Interfaces;
using EvenSteven.Shared.Models;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace EvenSteven.Infrastructure.Storage.Repositories
{
    internal class SqliteRoomRepository(IDbConnectionFactory connectionFactory, ILogger logger) : IRoomRepository
    {
        public async Task<Guid> CreateRoomAsync(Room room, CancellationToken cancellationToken)
        {
            await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

            string commandText = """
                             INSERT INTO Rooms (Id, Title, EditKey, InviteCode, PasswordHash, CreatedAt)
                                 VALUES (@Id, @Title, @EditKey, @InviteCode, @PasswordHash, @CreatedAt);
                             """;

            var roomId = Guid.NewGuid();

            var command = new CommandDefinition(commandText, new
            {
                Id = roomId,
                room.Title,
                room.EditKey,
                room.InviteCode,
                room.PasswordHash,
                CreatedAt = DateTime.UtcNow
            }, cancellationToken: cancellationToken);

            try
            {
                await connection.ExecuteAsync(command);
            }
            catch (DbException ex)
            {
                logger.LogError(exception: ex, message: "Error occured while creating new room");
                throw;
            }

            logger.LogDebug("Successfully created room with id: {roomId}", roomId);
            return roomId;
        }

        public async Task<Room?> GetRoomByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            await using var connection = await connectionFactory.CreateOpenConnectionAsync(cancellationToken);

            string commandText = """
                             SELECT Id, Title, EditKey, InviteCode, PasswordHash, Version, CreatedAt 
                                 FROM Rooms
                                 WHERE Id = @id;
                             """;

            try
            {
                var command = new CommandDefinition(commandText, new { id }, cancellationToken: cancellationToken);
                return await connection.QueryFirstOrDefaultAsync<Room>(command);
            }
            catch (DbException ex)
            {
                logger.LogError(exception: ex, message: "Error occured while getting room with id: {id}", id);
                throw;
            }
        }
    }
}
