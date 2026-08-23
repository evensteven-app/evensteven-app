using EvenSteven.Shared.Models;

namespace EvenSteven.Infrastructure.Storage.Repositories.Interfaces
{
    public interface IRoomRepository
    {
        Guid CreateRoomAsync(Room room);
        Room GetRoomByIdAsync(Guid id);
    }
}
