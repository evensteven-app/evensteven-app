namespace EvenSteven.Shared.Models
{
    public record Participant(
        Guid Id,
        Guid RoomId,
        string Name,
        string ParticipantKey,
        long Balance
    );
}
