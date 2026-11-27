namespace EvenSteven.Shared.Models
{
    public class Participant(
        Guid Id,
        Guid RoomId,
        string Name,
        string ParticipantKey,
        bool IsDeleted,
        DateTime CreatedAt,
        long Balance
    );
}
