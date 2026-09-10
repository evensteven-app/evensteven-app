using EvenSteven.Shared.Models;

namespace EvenSteven.Infrastructure.Storage.Repositories.Interfaces
{
    public interface IParticipantRepository
    {
        Task<Guid> AddParticipantAsync(Participant participant, CancellationToken cancellationToken);
        Task<List<Participant>> GetParticipantsByRoomAsync(Guid roomId, CancellationToken cancellationToken);
        Task DeleteParticipantAsync(Guid participantId, CancellationToken cancellationToken);
        Task UpdateParticipantNameAsync(Guid participantId, string newName, CancellationToken cancellationToken);
    }
}
