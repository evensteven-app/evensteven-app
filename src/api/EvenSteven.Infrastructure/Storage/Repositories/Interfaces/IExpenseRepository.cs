using EvenSteven.Shared.Models;

namespace EvenSteven.Infrastructure.Storage.Repositories.Interfaces
{
    public interface IExpenseRepository
    {
        Task<Expense> GetExpenseByIdAsync(Guid expenseId);
        Task<List<Expense>> GetAllExpensesByRoomIdAsync(Guid roomId);
        Task<Expense> CreateExpenseAsync(Guid roomId, Guid payerId, long amount, string? note = null);
        Task RevertExpenseByIdAsync(Guid expenseId, Guid revertedBy);
        Task CreateExpenseEntriesAsync(Guid expenseId, Dictionary<Guid, long> shares);
        Task<ExpenseEntry> GetExpenseEntryByIdAsync(Guid expenseEntryId);
        Task<List<ExpenseEntry>> GetAllExpenseEntriesByExpenseIdAsync(Guid expenseId);
        Task<List<ExpenseEntry>> GetAllExpenseEntriesByParticipantIdAsync(Guid participantId);
    }
}
