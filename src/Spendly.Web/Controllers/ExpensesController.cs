using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Spendly.Web.Contracts.Expenses;
using Spendly.Web.Services;

namespace Spendly.Web.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly ExpenseApiClient _api;

        public ExpensesController(ExpenseApiClient api)
        {
            _api = api;
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

        public async Task<IActionResult> Index(
            [FromQuery] string? category = null,
            [FromQuery] int page = 1)
        {
            var result = await _api.GetAllAsync(category, page, pageSize: 10);

            // Si la sesión expiró en el servidor, redirigir al login
            if (result.TotalCount == 0 && HttpContext.Session.GetString("token") != null)
            {
                // Podría ser token expirado; se muestra vacío sin crash
            }

            ViewBag.Category = category;
            ViewBag.CurrentPage = page;
            return View(result);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var expense = await _api.GetByIdAsync(id);
            if (expense is null)
            {
                TempData["Error"] = "Expense not found or you don't have access to it.";
                return RedirectToAction(nameof(Index));
            }
            return View(expense);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExpenseDto dto)
        {
            if (!ModelState.IsValid)
            {
                var result = await _api.GetAllAsync();
                return View("Index", result);
            }

            var (success, error) = await _api.CreateAsync(dto);
            if (!success)
            {
                TempData["Error"] = error ?? "Failed to create expense.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExpenseDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            var (success, error) = await _api.UpdateAsync(id, dto);
            if (!success)
            {
                TempData["Error"] = error ?? "Failed to update expense.";
                return View(dto);
            }

            TempData["Success"] = "Expense updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _api.DeleteAsync(id);
            if (!success)
                TempData["Error"] = "Could not delete expense.";

            return RedirectToAction(nameof(Index));
        }
    }
}