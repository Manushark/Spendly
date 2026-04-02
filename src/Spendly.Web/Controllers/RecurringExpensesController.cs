using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Spendly.Web.Contracts.RecurringExpenses;
using Spendly.Web.Services;

namespace Spendly.Web.Controllers
{
    public class RecurringExpensesController : Controller
    {
        private readonly RecurringExpenseApiClient _api;

        public RecurringExpensesController(RecurringExpenseApiClient api)
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

        // GET /RecurringExpenses
        public async Task<IActionResult> Index()
        {
            var summary = await _api.GetAllAsync();

            if (summary == null)
            {
                TempData["Error"] = "Could not load recurring expenses.";
                summary = new RecurringExpenseSummaryDto();
            }

            return View(summary);
        }

        // GET /RecurringExpenses/Create
        public IActionResult Create()
        {
            var model = new CreateRecurringExpenseDto
            {
                StartDate = DateTime.Today,
                Frequency = 3  // Monthly by default
            };

            return View(model);
        }

        // POST /RecurringExpenses/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRecurringExpenseDto dto)
        {
            if (!ModelState.IsValid)
            {
                return View(dto);
            }

            var (ok, error) = await _api.CreateAsync(dto);

            if (!ok)
            {
                TempData["Error"] = error ?? "Could not create recurring expense.";
                return View(dto);
            }

            TempData["Success"] = $"Recurring expense '{dto.Description}' created successfully!";
            return RedirectToAction(nameof(Index));
        }

        // GET /RecurringExpenses/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var recurring = await _api.GetByIdAsync(id);

            if (recurring == null)
            {
                TempData["Error"] = "Recurring expense not found.";
                return RedirectToAction(nameof(Index));
            }

            // Convertir string a int (Daily=1, Weekly=2, Monthly=3, Yearly=4)
            var frequencyInt = recurring.Frequency switch
            {
                "Daily" => 1,
                "Weekly" => 2,
                "Monthly" => 3,
                "Yearly" => 4,
                _ => 3  // Default Monthly
            };

            var model = new UpdateRecurringExpenseDto
            {
                Description = recurring.Description,
                Amount = recurring.Amount,
                Category = recurring.Category,
                Frequency = frequencyInt,
                StartDate = recurring.StartDate,
                EndDate = recurring.EndDate
            };

            ViewBag.RecurringId = id;
            ViewBag.IsActive = recurring.IsActive;
            return View(model);
        }

        // POST /RecurringExpenses/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, UpdateRecurringExpenseDto dto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.RecurringId = id;
                return View(dto);
            }

            var (ok, error) = await _api.UpdateAsync(id, dto);

            if (!ok)
            {
                TempData["Error"] = error ?? "Could not update recurring expense.";
                ViewBag.RecurringId = id;
                return View(dto);
            }

            TempData["Success"] = $"Recurring expense updated successfully!";
            return RedirectToAction(nameof(Index));
        }

        // POST /RecurringExpenses/Toggle/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Toggle(int id, bool activate)
        {
            var success = await _api.ToggleAsync(id, activate);

            if (!success)
            {
                TempData["Error"] = "Could not toggle recurring expense.";
            }
            else
            {
                var action = activate ? "activated" : "paused";
                TempData["Success"] = $"Recurring expense {action} successfully.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST /RecurringExpenses/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _api.DeleteAsync(id);

            if (!deleted)
            {
                TempData["Error"] = "Could not delete recurring expense.";
            }
            else
            {
                TempData["Success"] = "Recurring expense deleted successfully.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}