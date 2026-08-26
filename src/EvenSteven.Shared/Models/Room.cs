namespace EvenSteven.Shared.Models
{
    public record Room(
        Guid Id,
        string Title,
        Guid EditKey,
        string PasswordHash,
        long Version,
        DateTime CreatedAt
    );
}
