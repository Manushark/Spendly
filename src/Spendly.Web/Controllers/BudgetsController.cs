using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Spendly.Web.Contracts.Budgets;
using Spendly.Web.Services;

namespace Spendly.Web.Controllers
{
    public class BudgetsController : Controller
    {
        private readonly BudgetApiClient _api;

        public BudgetsController(BudgetApiClient api)
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

        // GET /Budgets
        public async Task<IActionResult> Index()
        {
            var now = DateTime.Now;
            var summary = await _api.GetSummaryAsync(now.Year, now.Month);

            if (summary == null)
            {
                TempData["Error"] = "Could not load budget data.";
                summary = new BudgetSummaryDto(); // Empty summary
            }

            ViewBag.CurrentYear = now.Year;
            ViewBag.CurrentMonth = now.Month;
            ViewBag.MonthName = now.ToString("MMMM yyyy");

            return View(summary);
        }

        // GET /Budgets/Create
        public IActionResult Create()
        {
            var now = DateTime.Now;

            var model = new CreateBudgetDto
            {
                Year = now.Year,
                Month = now.Month,
                MonthlyLimit = 0
            };

            return View(model);
        }

        // POST /Budgets/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateBudgetDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var (ok, error) = await _api.CreateAsync(dto);

            if (!ok)
            {
                TempData["Error"] = error ?? "Could not create budget.";
                return View(dto);
            }

            TempData["Success"] = $"Budget for '{dto.Category}' created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET /Budgets/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var budget = await _api.GetByIdAsync(id);

            if (budget == null)
            {
                TempData["Error"] = "Budget not found or you don't have permission to edit it.";
                return RedirectToAction(nameof(Index));
            }

            var model = new UpdateBudgetDto
            {
                Category = budget.Category,
                MonthlyLimit = budget.MonthlyLimit,
                Year = budget.Year,
                Month = budget.Month
            };

            ViewBag.BudgetId = id;
            return View(model);
        }

        // POST /Budgets/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateBudgetDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.BudgetId = id;
                return View(dto);
            }

            var (ok, error) = await _api.UpdateAsync(id, dto);

            if (!ok)
            {
                TempData["Error"] = error ?? "Could not update budget.";
                ViewBag.BudgetId = id;
                return View(dto);
            }

            TempData["Success"] = $"Budget for '{dto.Category}' updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST /Budgets/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _api.DeleteAsync(id);

            if (!deleted)
            {
                TempData["Error"] = "Could not delete budget.";
            }
            else
            {
                TempData["Success"] = "Budget deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}