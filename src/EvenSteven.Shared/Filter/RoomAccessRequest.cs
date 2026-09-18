using System;
using System.Collections.Generic;
using System.Text;

namespace EvenSteven.Shared.Filter
{
    public record RoomAccessRequest(
        string? EditKey,
        string? ParticipantKey,
        string? Password,
        Guid RoomId,
        string? IpAddress
    );
}
