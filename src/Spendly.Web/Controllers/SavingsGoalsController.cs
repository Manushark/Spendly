using Microsoft.AspNetCore.Mvc;
using Spendly.Web.Contracts.SavingsGoals;
using Spendly.Web.Services;

namespace Spendly.Web.Controllers
{
    public class SavingsGoalsController : Controller
    {
        private readonly SavingsGoalApiClient _api;

        public SavingsGoalsController(SavingsGoalApiClient api) => _api = api;

        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var goals = await _api.GetAllAsync();
            return View(goals);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var goal = await _api.GetByIdAsync(id);
            if (goal == null)
            {
                TempData["Error"] = "Goal not found.";
                return RedirectToAction(nameof(Index));
            }
            return View(goal);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSavingsGoalRequest request)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Index));

            var success = await _api.CreateAsync(request);
            if (!success)
                TempData["Error"] = "Failed to create savings goal.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(int id, UpdateSavingsGoalRequest request)
        {
            var success = await _api.UpdateAsync(id, request);
            if (!success)
                TempData["Error"] = "Failed to update savings goal.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFunds(int id, decimal amount)
        {
            var success = await _api.AddFundsAsync(id, amount);
            if (!success)
                TempData["Error"] = "Failed to add funds.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _api.DeleteAsync(id);
            if (!success)
                TempData["Error"] = "Could not delete savings goal.";

            return RedirectToAction(nameof(Index));
        }
    }
}
