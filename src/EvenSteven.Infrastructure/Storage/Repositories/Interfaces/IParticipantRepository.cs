using EvenSteven.Shared.Models;

namespace EvenSteven.Infrastructure.Storage.Repositories.Interfaces
{
    public interface IParticipantRepository
    {
        Guid AddParticipantAsync(Participant participant);
        List<Participant> GetParticipantsByRoomAsync(Guid roomId);
        bool DeleteParticipantAsync(Guid participantId);
        bool UpdateParticipantNameAsync(Guid participantId, string newName);
    }
}
