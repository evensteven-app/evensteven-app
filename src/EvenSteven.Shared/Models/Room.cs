namespace EvenSteven.Shared.Models
{
    public record Room(
        Guid Id,
        Guid EditKey,
        string PasswordHash,
        int Version,
        DateTime CreatedAt
    );
}
