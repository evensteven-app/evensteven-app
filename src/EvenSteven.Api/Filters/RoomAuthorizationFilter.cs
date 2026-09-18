using EvenSteven.Shared.Filter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;

namespace EvenSteven.Api.Filters
{
    public class RoomAuthorizationFilter(
            IEnumerable<IRoomAccessProvider> providers,
            IRoomAccessGate gate,
            ICurrentRoomAccess access
        ) : IAsyncAuthorizationFilter
    {
        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            RoomAccessRequest roomAccessRequest =
                CreateRoomAccessRequest(context.HttpContext.Request, context.HttpContext.Connection);

            if (roomAccessRequest.RoomId == Guid.Empty ||
                    (roomAccessRequest.EditKey == null && roomAccessRequest.ParticipantKey == null && roomAccessRequest.Password == null))
            {
                context.Result = new StatusCodeResult(403);
                return;
            }

            // TODO: Проверка на Brute-Force

            IReadOnlyList<RoomGrant> roomGrants = [];
            foreach (IRoomAccessProvider provider in providers)
            {
                roomGrants = await provider.ResolveAsync(roomAccessRequest, context.HttpContext.RequestAborted);
                if (roomGrants.Count > 0) break;
            }

            if (roomGrants.Count == 0)
            {
                context.Result = new StatusCodeResult(403);
                return;
            }

            roomGrants = gate.Apply(roomGrants);
            if (roomGrants.Count == 0)
            {
                context.Result = new StatusCodeResult(403);
                return;
            }

            // TODO: Сбросить счетчик Brute-Force

            access.SetGrants(roomGrants);
        }

        private static RoomAccessRequest CreateRoomAccessRequest(HttpRequest request, ConnectionInfo connection)
        {
            request.Headers.TryGetValue("X-Edit-Key", out StringValues editKeyValues);
            request.Headers.TryGetValue("X-Participant-Key", out StringValues participantKeyValues);
            request.Headers.TryGetValue("X-Room-Password", out StringValues passwordValues);
            request.RouteValues.TryGetValue("roomId", out object? strRoomId);
            Guid.TryParse(strRoomId?.ToString(), out Guid roomId);

            return new RoomAccessRequest(
                editKeyValues.ToString(),
                participantKeyValues.ToString(),
                passwordValues.ToString(),
                roomId,
                connection.RemoteIpAddress?.ToString()
            );
        }
    }
}
