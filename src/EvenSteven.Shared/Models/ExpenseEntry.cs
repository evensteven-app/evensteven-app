namespace EvenSteven.Shared.Models
{
    public record ExpenseEntry(
        Guid Id,
        Guid ExpenseId,
        Guid ParticipantId,
        long Share
    );
}
