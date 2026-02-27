using Microsoft.AspNetCore.Mvc;
using Spendly.Web.Services;

namespace Spendly.Web.Controllers
{
    public class DashboardController : Controller
    {
        private readonly DashboardApiClient _api;

        public DashboardController(DashboardApiClient api)
        {
            _api = api;
        }

        public async Task<IActionResult> Index()
        {
            // Verificar token
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var stats = await _api.GetStatsAsync();

            if (stats == null)
            {
                TempData["Error"] = "Could not load dashboard data.";
                return RedirectToAction("Index", "Expenses");
            }

            return View(stats);
        }
    }
}
