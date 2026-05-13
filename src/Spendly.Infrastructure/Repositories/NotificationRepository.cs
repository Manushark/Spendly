using Microsoft.EntityFrameworkCore;
using Spendly.Application.Interfaces;
using Spendly.Domain.Entities;
using Spendly.Domain.Enums;
using Spendly.Infrastructure.Persistence;

namespace Spendly.Infrastructure.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly SpendlyDbContext _context;

        public NotificationRepository(SpendlyDbContext context) => _context = context;

        public async Task AddAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetAllByUserAsync(int userId, int page, int pageSize)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<List<Notification>> GetUnreadByUserAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .ToListAsync();
        }

        public async Task<int> CountUnreadAsync(int userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task MarkAsReadAsync(int userId, int id)
        {
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == userId);

            if (notification != null)
            {
                notification.MarkAsRead();
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarkAllAsReadAsync(int userId)
        {
            var unread = await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync();

            foreach (var n in unread)
                n.MarkAsRead();

            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Checks whether a notification of the given type already exists for this
        /// budget in the specified month. Uses explicit DateTimeKind.Utc to ensure
        /// the comparison against CreatedAt (stored as UTC) is consistent.
        /// </summary>
        public async Task<bool> ExistsForBudgetThisMonthAsync(
            int userId, int budgetId, NotificationType type, int year, int month)
        {
            var monthStart = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var monthEnd   = monthStart.AddMonths(1);

            return await _context.Notifications
                .AnyAsync(n => n.UserId == userId
                    && n.RelatedEntityId == budgetId
                    && n.Type == type
                    && n.CreatedAt >= monthStart
                    && n.CreatedAt < monthEnd);
        }
    }
}
