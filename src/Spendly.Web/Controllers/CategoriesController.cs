using Microsoft.AspNetCore.Mvc;
using Spendly.Web.Services;

namespace Spendly.Web.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly CategoryApiClient _api;

        public CategoriesController(CategoryApiClient api) => _api = api;

        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var categories = await _api.GetAllAsync();
            return View(categories);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCategoryRequest request)
        {
            var (success, error) = await _api.CreateAsync(request);
            if (!success)
                TempData["Error"] = error ?? "Failed to create category.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, UpdateCategoryRequest request)
        {
            var (success, error) = await _api.UpdateAsync(id, request);
            if (!success)
                TempData["Error"] = error ?? "Failed to update category.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var (success, error) = await _api.DeleteAsync(id);
            if (!success)
                TempData["Error"] = error ?? "Cannot delete this category.";

            return RedirectToAction(nameof(Index));
        }
    }
}
