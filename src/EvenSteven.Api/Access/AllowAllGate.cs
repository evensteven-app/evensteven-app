using EvenSteven.Shared.Filter;

namespace EvenSteven.Api.Access
{
    public class AllowAllGate : IRoomAccessGate
    {
        public IReadOnlyList<RoomGrant> Apply(IReadOnlyList<RoomGrant> grants)
        {
            return grants;
        }
    }
}
