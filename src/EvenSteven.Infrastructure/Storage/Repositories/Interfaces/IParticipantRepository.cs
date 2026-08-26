using EvenSteven.Shared.Models;

namespace EvenSteven.Infrastructure.Storage.Repositories.Interfaces
{
    public interface IParticipantRepository
    {
        Task<Guid> AddParticipantAsync(Participant participant);
        Task<List<Participant>> GetParticipantsByRoomAsync(Guid roomId);
        Task<bool> DeleteParticipantAsync(Guid participantId);
        Task<bool> UpdateParticipantNameAsync(Guid participantId, string newName);
    }
}
