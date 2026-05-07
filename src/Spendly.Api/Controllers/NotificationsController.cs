using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Spendly.Api.Extensions;
using Spendly.Api.Security;
using Spendly.Application.UseCases.Notifications;

namespace Spendly.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly GetNotificationsUseCase _getAll;
        private readonly MarkNotificationReadUseCase _markRead;
        private readonly MarkAllNotificationsReadUseCase _markAllRead;
        private readonly GetUnreadCountUseCase _getUnreadCount;

        public NotificationsController(
            GetNotificationsUseCase getAll,
            MarkNotificationReadUseCase markRead,
            MarkAllNotificationsReadUseCase markAllRead,
            GetUnreadCountUseCase getUnreadCount)
        {
            _getAll = getAll;
            _markRead = markRead;
            _markAllRead = markAllRead;
            _getUnreadCount = getUnreadCount;
        }

        /// <summary>
        /// GET /api/notifications
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var notifications = await _getAll.ExecuteAsync(User.GetUserId(), page, pageSize);
            return Ok(notifications);
        }

        /// <summary>
        /// GET /api/notifications/unread-count
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var count = await _getUnreadCount.ExecuteAsync(User.GetUserId());
            return Ok(new { count });
        }

        /// <summary>
        /// PUT /api/notifications/{id}/read
        /// </summary>
        [EnableRateLimiting(RateLimitPolicies.WriteOperations)]
        [HttpPut("{id:int}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _markRead.ExecuteAsync(User.GetUserId(), id);
            return Ok(new { message = "Notification marked as read" });
        }

        /// <summary>
        /// PUT /api/notifications/read-all
        /// </summary>
        [EnableRateLimiting(RateLimitPolicies.WriteOperations)]
        [HttpPut("read-all")]
        public async Task<IActionResult> MarkAllAsRead()
        {
            await _markAllRead.ExecuteAsync(User.GetUserId());
            return Ok(new { message = "All notifications marked as read" });
        }
    }
}
