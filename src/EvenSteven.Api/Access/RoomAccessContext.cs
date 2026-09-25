using EvenSteven.Shared.Filter;

namespace EvenSteven.Api.Access
{
    public class RoomAccessContext : ICurrentRoomAccess
    {
        private List<RoomGrant> _grants = [];

        public IReadOnlyList<RoomGrant> Grants => _grants.AsReadOnly();
        public RoomRole? RoomRole { get; private set; }
        public Guid? ParticipantId { get; private set; }

        public void SetGrants(IReadOnlyList<RoomGrant> grants)
        {
            _grants = [.. grants];

            RoomRole = grants.FirstOrDefault()?.RoomRole;

            ParticipantId =
                grants.FirstOrDefault(g => g.RoomRole == EvenSteven.Shared.Filter.RoomRole.Participant)?.ParticipantId;
        }

        public void Require(Guid roomId)
        {
            if (_grants.All(g => g.RoomId != roomId))
            {
                throw new ForbiddenException($"Access to room {roomId} is denied.");
            }
        }

        public bool CanEdit(Guid roomId)
        {
            return _grants.Any(g => g.RoomId == roomId && g.RoomRole == EvenSteven.Shared.Filter.RoomRole.Admin);
        }
    }
}
