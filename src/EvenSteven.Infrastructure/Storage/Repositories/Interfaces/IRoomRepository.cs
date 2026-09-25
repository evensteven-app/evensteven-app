using EvenSteven.Shared.Models;

namespace EvenSteven.Infrastructure.Storage.Repositories.Interfaces
{
    public interface IRoomRepository
    {
        Task<Guid> CreateRoomAsync(Room room, CancellationToken cancellationToken);
        Task<Room?> GetRoomByIdAsync(Guid id, CancellationToken cancellationToken);
        Task<Room?> GetRoomByEditKeyAsync(Guid roomId, Guid editKey, CancellationToken cancellationToken);
    }
}
