namespace EvenSteven.Shared.Models
{
    public record EventEntry(
        Guid Id,
        Guid EventId,
        Guid ParticipantId,
        long Share
    );
}
