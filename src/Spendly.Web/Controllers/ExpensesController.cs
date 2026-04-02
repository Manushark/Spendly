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

        // Redirigir al login si no hay token
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

        // GET /Expenses
        public async Task<IActionResult> Index(string? category, int page = 1)
        {
            var result = await _api.GetAllAsync(category, page, 10);

            ViewBag.Category = category;
            ViewBag.CurrentPage = page;

            return View(result);
        }

        // POST /Expenses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ExpenseDto dto)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Invalid expense data.";
                return RedirectToAction(nameof(Index));
            }

            var (success, error) = await _api.CreateAsync(dto);

            if (!success)
            {
                TempData["Error"] = error ?? "Could not create expense.";
            }
            else
            {
                TempData["Success"] = "Expense created successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET /Expenses/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var expense = await _api.GetByIdAsync(id);

            if (expense == null)
            {
                TempData["Error"] = "Expense not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(expense);
        }

        // POST /Expenses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExpenseDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var (success, error) = await _api.UpdateAsync(id, dto);

            if (!success)
            {
                TempData["Error"] = error ?? "Could not update expense.";
                return View(dto);
            }

            TempData["Success"] = "Expense updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST /Expenses/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _api.DeleteAsync(id);

            if (!deleted)
            {
                TempData["Error"] = "Could not delete expense.";
            }
            else
            {
                TempData["Success"] = "Expense deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}