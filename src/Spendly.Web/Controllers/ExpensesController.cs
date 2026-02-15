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

        public async Task<IActionResult> Index()
        {
            var expenses = await _api.GetAllAsync();
            return View(expenses);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var expense = await _api.GetByIdAsync(id);
            if (expense is null)
            {
                TempData["Error"] = "Expense not found.";
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
                var expenses = await _api.GetAllAsync();
                return View("Index", expenses);
            }

            await _api.CreateAsync(dto);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ExpenseDto dto)
        {
            if (!ModelState.IsValid)
                return View(dto);

            await _api.UpdateAsync(id, dto);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            await _api.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
