namespace EvenSteven.Shared.Models
{
    public record Room(
        Guid Id,
        string Title,
        string InviteCode,
        Guid EditKey,
        string PasswordHash,
        long Version,
        DateTime CreatedAt,
        DateTime EditedAt
    );
}
