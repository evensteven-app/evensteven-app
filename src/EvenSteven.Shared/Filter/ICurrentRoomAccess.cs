using System;
using System.Collections.Generic;
using System.Text;

namespace EvenSteven.Shared.Filter
{
    public interface ICurrentRoomAccess
    {
        public IReadOnlyList<RoomGrant> Grants { get; }
        public RoomRole RoomRole { get; }
        public Guid? ParticipantId { get; }

        public void SetGrants(IReadOnlyList<RoomGrant> grants);
        public void Require(Guid roomId);
        public bool CanEdit(Guid roomId);
    }
}
