using Spendly.Application.DTOs.Notification;
using Spendly.Application.Interfaces;

namespace Spendly.Application.UseCases.Notifications
{
    public class GetNotificationsUseCase
    {
        private readonly INotificationRepository _repo;

        public GetNotificationsUseCase(INotificationRepository repo) => _repo = repo;

        public async Task<List<NotificationResponseDto>> ExecuteAsync(int userId, int page = 1, int pageSize = 20)
        {
            var notifications = await _repo.GetAllByUserAsync(userId, page, pageSize);
            return notifications.Select(n => new NotificationResponseDto
            {
                Id = n.Id,
                Message = n.Message,
                Type = n.Type.ToString(),
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                RelatedEntityId = n.RelatedEntityId
            }).ToList();
        }
    }

    public class MarkNotificationReadUseCase
    {
        private readonly INotificationRepository _repo;

        public MarkNotificationReadUseCase(INotificationRepository repo) => _repo = repo;

        public async Task ExecuteAsync(int userId, int id)
        {
            await _repo.MarkAsReadAsync(userId, id);
        }
    }

    public class MarkAllNotificationsReadUseCase
    {
        private readonly INotificationRepository _repo;

        public MarkAllNotificationsReadUseCase(INotificationRepository repo) => _repo = repo;

        public async Task ExecuteAsync(int userId)
        {
            await _repo.MarkAllAsReadAsync(userId);
        }
    }

    public class GetUnreadCountUseCase
    {
        private readonly INotificationRepository _repo;

        public GetUnreadCountUseCase(INotificationRepository repo) => _repo = repo;

        public async Task<int> ExecuteAsync(int userId)
        {
            return await _repo.CountUnreadAsync(userId);
        }
    }
}
