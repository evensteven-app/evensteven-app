namespace EvenSteven.Shared.Filter
{
    public interface IRoomAccessGate
    {
        public IReadOnlyList<RoomGrant> Apply(IReadOnlyList<RoomGrant> grants);
    }
}
