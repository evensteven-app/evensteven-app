using EvenSteven.Shared.Models;

namespace EvenSteven.Infrastructure.Storage.Repositories.Interfaces
{
    public interface IExpenseRepository
    {
        Task AddExpenseAsync(Expense expense, List<Participant> participants, CancellationToken cancellationToken);
        Task<List<Expense>> GetExspensesByRoomAsync(Guid roomId, CancellationToken cancellationToken);
        Task RevertExpenseAsync(Guid expenseId, CancellationToken cancellationToken);
    }
}
