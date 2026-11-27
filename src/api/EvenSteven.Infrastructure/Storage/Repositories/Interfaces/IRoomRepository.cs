using EvenSteven.Shared.Models;

namespace EvenSteven.Infrastructure.Storage.Repositories.Interfaces
{
    public interface IRoomRepository
    {
        Task<Room> GetRoomByIdAsync(Guid roomId);
        Task<Room> GetRoomByInviteCodeAsync(string inviteCode);
        Task<Room> GetRoomByEditKeyAsync(Guid editKey);
        Task<Room> CreateRoomAsync(string title);
        Task UpdateRoomTitleAsync(Guid roomId, string newTitle);
        Task IncreaseRoomVersionAsync(Guid roomId);
    }
}
