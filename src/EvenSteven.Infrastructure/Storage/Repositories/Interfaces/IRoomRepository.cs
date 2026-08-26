using EvenSteven.Shared.Models;

namespace EvenSteven.Infrastructure.Storage.Repositories.Interfaces
{
    public interface IRoomRepository
    {
        Task<Guid> CreateRoomAsync(Room room, CancellationToken cancellationToken);
        Task<Room?> GetRoomByIdAsync(Guid id, CancellationToken cancellationToken);
    }
}
