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

            string command = """
                INSERT INTO Rooms (Id, Title, EditKey, PasswordHash, CreatedAt)
                    VALUES (@Id, @Title, @EditKey, @PasswordHash, @CreatedAt);
                """;

            var roomId = Guid.NewGuid();

            try
            {
                await connection.ExecuteAsync(command, new
                {
                    Id = roomId,
                    room.Title,
                    room.EditKey,
                    room.PasswordHash,
                    CreatedAt = DateTime.UtcNow
                });
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

            string command = """
                SELECT Id, Title, EditKey, PasswordHash, Version, CreatedAt 
                    FROM Rooms
                    WHERE Id = @id;
                """;

            try
            {
                return await connection.QueryFirstOrDefaultAsync<Room>(command, new { id });
            }
            catch (DbException ex)
            {
                logger.LogError(exception: ex, message: "Error occured while getting room with id: {id}", id);
                throw;
            }
        }
    }
}
