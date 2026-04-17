using Microsoft.AspNetCore.Mvc;
using Spendly.Web.Services;

namespace Spendly.Web.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly NotificationApiClient _api;

        public NotificationsController(NotificationApiClient api) => _api = api;

        /// <summary>
        /// GET /Notifications/UnreadCount — AJAX endpoint for the bell badge
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> UnreadCount()
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
                return Json(new { count = 0 });

            var count = await _api.GetUnreadCountAsync();
            return Json(new { count });
        }

        /// <summary>
        /// GET /Notifications/Recent — AJAX endpoint for the dropdown
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Recent()
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
                return Json(new List<NotificationDto>());

            var notifications = await _api.GetAllAsync(1, 10);
            return Json(notifications);
        }

        /// <summary>
        /// POST /Notifications/MarkAsRead/{id}
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _api.MarkAsReadAsync(id);
            return Json(new { success = true });
        }

        /// <summary>
        /// POST /Notifications/MarkAllAsRead
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            await _api.MarkAllAsReadAsync();
            return Json(new { success = true });
        }
    }
}
