namespace EvenSteven.Shared.Models
{
    public record Room(
        Guid Id,
        string Title,
        Guid EditKey,
        string InviteCode,
        string PasswordHash,
        long Version,
        DateTime CreatedAt
    );
}
