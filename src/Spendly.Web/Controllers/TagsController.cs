using Microsoft.AspNetCore.Mvc;
using Spendly.Web.Services;

namespace Spendly.Web.Controllers
{
    public class TagsController : Controller
    {
        private readonly TagApiClient _api;

        public TagsController(TagApiClient api) => _api = api;

        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var tags = await _api.GetAllAsync();
            return View(tags);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string name, string color)
        {
            var success = await _api.CreateAsync(name, color ?? "#6366f1");
            if (!success)
                TempData["Error"] = "Failed to create tag. It may already exist.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _api.DeleteAsync(id);
            if (!success)
                TempData["Error"] = "Failed to delete tag.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, string name, string color)
        {
            var success = await _api.UpdateAsync(id, name, color ?? "#6366f1");
            if (!success)
                TempData["Error"] = "Failed to update tag. It may already exist.";

            return RedirectToAction(nameof(Index));
        }
    }
}
