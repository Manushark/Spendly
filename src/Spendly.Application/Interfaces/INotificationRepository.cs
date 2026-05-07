using Spendly.Domain.Entities;
using Spendly.Domain.Enums;

namespace Spendly.Application.Interfaces
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);
        Task<List<Notification>> GetAllByUserAsync(int userId, int page, int pageSize);
        Task<List<Notification>> GetUnreadByUserAsync(int userId);
        Task<int> CountUnreadAsync(int userId);
        Task MarkAsReadAsync(int userId, int id);
        Task MarkAllAsReadAsync(int userId);
        Task<bool> ExistsForBudgetThisMonthAsync(int userId, int budgetId, NotificationType type, int year, int month);
    }
}
