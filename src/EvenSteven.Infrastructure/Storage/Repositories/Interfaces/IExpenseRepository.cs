using EvenSteven.Shared.Models;

namespace EvenSteven.Infrastructure.Storage.Repositories.Interfaces
{
    public interface IExpenseRepository
    {
        bool AddExspenseAsync(Event _event);
        Event GetExspenseByRoomAsync(Guid roomId);
        bool RevertExpenseAsync(Guid eventId);
    }
}
