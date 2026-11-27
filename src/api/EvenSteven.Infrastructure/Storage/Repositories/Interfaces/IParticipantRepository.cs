using EvenSteven.Shared.Models;

namespace EvenSteven.Infrastructure.Storage.Repositories.Interfaces
{
    public interface IParticipantRepository
    {
        Task<Participant> GetParticipantByIdAsync(Guid participantId);
        Task<List<Participant>> GetParticipantsByRoomIdAsync(Guid roomId);
        Task<Participant> CreateParticipantAsync(Guid roomId, string name);
        Task UpdateParticipantNameAsync(Guid participantId, string newName);
        Task DeleteParticipantByIdAsync(Guid participantId);
    }
}
