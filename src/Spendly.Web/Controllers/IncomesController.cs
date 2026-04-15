using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Spendly.Web.Contracts.Incomes;
using Spendly.Web.Services;

namespace Spendly.Web.Controllers
{
    public class IncomesController : Controller
    {
        private readonly IncomeApiClient _api;

        public IncomesController(IncomeApiClient api) => _api = api;

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

        public async Task<IActionResult> Index([FromQuery] int page = 1)
        {
            var result = await _api.GetAllAsync(page, pageSize: 10);
            ViewBag.CurrentPage = page;
            return View(result);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var income = await _api.GetByIdAsync(id);
            if (income is null)
            {
                TempData["Error"] = "Income not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(income);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(IncomeDto dto)
        {
            var (success, error) = await _api.CreateAsync(dto);
            if (!success)
                TempData["Error"] = error ?? "Failed to create income.";
            else
                TempData["Success"] = "Income added successfully!";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, IncomeDto dto)
        {
            var (success, error) = await _api.UpdateAsync(id, dto);
            if (!success)
            {
                TempData["Error"] = error ?? "Failed to update income.";
                return View(dto);
            }

            TempData["Success"] = "Income updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _api.DeleteAsync(id);
            if (!success)
                TempData["Error"] = "Could not delete income.";

            return RedirectToAction(nameof(Index));
        }
    }
}
