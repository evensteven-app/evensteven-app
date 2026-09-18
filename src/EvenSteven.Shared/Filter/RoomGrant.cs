namespace EvenSteven.Shared.Filter
{
    public record RoomGrant(
        Guid RoomId,
        RoomRole? RoomRole,
        Guid? ParticipantId
    );
}
