namespace EvenSteven.Shared.Models
{
    public record Expense(
        Guid Id,
        Guid RoomId,
        Guid PayerId,
        long Amount,
        string? Note,
        bool IsReverted,
        Guid? RevertedBy,
        DateTime? RevertedAt,
        DateTime CreatedAt
    );
}
