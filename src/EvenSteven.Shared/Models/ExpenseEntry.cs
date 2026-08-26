namespace EvenSteven.Shared.Models
{
    public record ExpenseEntry(
        Guid Id,
        Guid EventId,
        Guid ParticipantId,
        long Share
    );
}
