using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Spendly.Web.Services;

namespace Spendly.Web.Controllers
{
    public class SettingsController : Controller
    {
        private readonly UserApiClient _userApi;
        private readonly CategoryApiClient _categoryApi;

        public SettingsController(UserApiClient userApi, CategoryApiClient categoryApi)
        {
            _userApi = userApi;
            _categoryApi = categoryApi;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
            {
                context.Result = RedirectToAction("Login", "Auth");
                return;
            }
            base.OnActionExecuting(context);
        }

        public async Task<IActionResult> Index()
        {
            var profile = await _userApi.GetProfileAsync();
            var categories = await _categoryApi.GetAllAsync();

            ViewBag.Profile = profile;
            ViewBag.Categories = categories;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProfile(UpdateProfileRequest request)
        {
            var (success, error) = await _userApi.UpdateProfileAsync(request);
            if (!success)
            {
                TempData["Error"] = error ?? "Failed to update profile.";
            }
            else
            {
                TempData["Success"] = "Profile updated successfully.";
                // Update session name if changed
                if (!string.IsNullOrEmpty(request.FullName))
                    HttpContext.Session.SetString("userName", request.FullName);
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordRequest request)
        {
            if (request.NewPassword != request.ConfirmNewPassword)
            {
                TempData["Error"] = "New passwords do not match.";
                return RedirectToAction(nameof(Index));
            }

            var (success, error) = await _userApi.ChangePasswordAsync(request);
            if (!success)
            {
                TempData["Error"] = error ?? "Failed to change password.";
            }
            else
            {
                TempData["Success"] = "Password changed successfully.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CreateCategoryRequest request)
        {
            var (success, error) = await _categoryApi.CreateAsync(request);
            if (!success)
                TempData["Error"] = error ?? "Failed to create category.";
            else
                TempData["Success"] = "Category created successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryRequest request)
        {
            var (success, error) = await _categoryApi.UpdateAsync(id, request);
            if (!success)
                TempData["Error"] = error ?? "Failed to update category.";
            else
                TempData["Success"] = "Category updated successfully.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var success = await _categoryApi.DeleteAsync(id);
            if (!success)
                TempData["Error"] = "Failed to delete category.";
            else
                TempData["Success"] = "Category deleted.";

            return RedirectToAction(nameof(Index));
        }
    }
}
