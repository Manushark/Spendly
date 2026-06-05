using Microsoft.AspNetCore.Mvc;
using Spendly.Application.Interfaces;
using Spendly.Web.Contracts.Insights;
using Spendly.Web.Services;

namespace Spendly.Web.Controllers
{
    public class InsightsController : Controller
    {
        private readonly InsightsApiClient _api;
        private readonly IDateTimeProvider _dateTime;

        public InsightsController(InsightsApiClient api, IDateTimeProvider dateTime)
        {
            _api = api;
            _dateTime = dateTime;
        }

        public async Task<IActionResult> Index(int? month = null, int? year = null)
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var userTimeZone = HttpContext.Session.GetString("userTimeZone");
            var data = await _api.GetMonthlyInsightsAsync(month, year, userTimeZone);
            var now = _dateTime.Now(userTimeZone);
            data ??= new MonthlyInsightsDto
            {
                Year = year ?? now.Year,
                Month = month ?? now.Month,
                MonthName = new DateTime(year ?? now.Year, month ?? now.Month, 1).ToString("MMMM yyyy")
            };

            return View(data);
        }
    }
}
