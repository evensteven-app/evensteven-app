namespace EvenSteven.Shared.Models
{
    public record Expense(
        Guid Id,
        Guid RoomId,
        long Amount,
        string Note,
        bool IsReverted,
        DateTime? RevertedAt,
        DateTime CreatedAt
    );
}
