using Microsoft.AspNetCore.Mvc;

namespace Spendly.Web.Controllers
{
    public class InsightsController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            // Insights feature is coming soon - just show placeholder
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            return View();
        }
    }
}