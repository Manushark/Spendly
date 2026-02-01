using Microsoft.AspNetCore.Mvc;
using Spendly.Contracts.Expenses;
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

        public async Task<IActionResult> Index()
        {
            var expenses = await _api.GetAllAsync();
            return View(expenses);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ExpenseDto dto)
        {
            await _api.CreateAsync(dto);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            await _api.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
