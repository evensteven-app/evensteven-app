namespace EvenSteven.Shared.Filter
{
    public interface IRoomAccessProvider
    {
        public uint Priority { get; }
        public Task<IReadOnlyList<RoomGrant>> ResolveAsync(RoomAccessRequest request, CancellationToken cancellationToken);
    }
}
