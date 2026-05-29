using Microsoft.AspNetCore.Mvc;
using Spendly.Web.Services;

namespace Spendly.Web.Controllers
{
    public class NotificationsController : Controller
    {
        private readonly NotificationApiClient _api;

        public NotificationsController(NotificationApiClient api) => _api = api;

        /// <summary>
        /// GET /Notifications — Full paginated history page
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> Index(int page = 1, string? filter = null)
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var notifications = await _api.GetAllAsync(page, 20);

            // Client-side filter applied to the page result
            if (!string.IsNullOrEmpty(filter) && filter == "unread")
                notifications = notifications.Where(n => !n.IsRead).ToList();

            ViewBag.CurrentPage = page;
            ViewBag.ActiveFilter = filter ?? "all";
            return View(notifications);
        }

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

        /// <summary>
        /// POST /Notifications/Delete/{id}
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
                return Json(new { success = false, message = "Unauthorized" });

            await _api.DeleteAsync(id);
            return Json(new { success = true });
        }

        /// <summary>
        /// POST /Notifications/DeleteAll
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> DeleteAll()
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
                return Json(new { success = false, message = "Unauthorized" });

            await _api.DeleteAllAsync();
            return Json(new { success = true });
        }
    }
}
