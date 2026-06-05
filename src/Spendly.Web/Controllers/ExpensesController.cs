using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using Spendly.Web.Contracts.Expenses;
using Spendly.Web.Services;

namespace Spendly.Web.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly ExpenseApiClient _api;
        private readonly CategoryApiClient _categoryApi;
        private readonly TagApiClient _tagApi;

        public ExpensesController(ExpenseApiClient api, CategoryApiClient categoryApi, TagApiClient tagApi)
        {
            _api = api;
            _categoryApi = categoryApi;
            _tagApi = tagApi;
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
            [FromQuery] string? search = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null,
            [FromQuery] decimal? minAmount = null,
            [FromQuery] decimal? maxAmount = null,
            [FromQuery] int page = 1)
        {
            var result = await _api.GetAllAsync(category, search, dateFrom, dateTo, minAmount, maxAmount, page, pageSize: 10);
            var categories = await _categoryApi.GetAllAsync();
            var tags = await _tagApi.GetAllAsync();

            ViewBag.Category = category;
            ViewBag.Search = search;
            ViewBag.DateFrom = dateFrom;
            ViewBag.DateTo = dateTo;
            ViewBag.MinAmount = minAmount;
            ViewBag.MaxAmount = maxAmount;
            ViewBag.CurrentPage = page;
            ViewBag.Categories = categories;
            ViewBag.Tags = tags;

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
            var categories = await _categoryApi.GetAllAsync();
            var tags = await _tagApi.GetAllAsync();

            if (expense.Tags != null)
            {
                expense.TagIds = expense.Tags.Select(t => t.Id).ToList();
            }

            ViewBag.CategoryList = new SelectList(categories, "Name", "Name", expense.Category);
            ViewBag.Tags = tags;
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
            {
                var categories = await _categoryApi.GetAllAsync();
                var tags = await _tagApi.GetAllAsync();
                ViewBag.CategoryList = new SelectList(categories, "Name", "Name", dto.Category);
                ViewBag.Tags = tags;
                return View(dto);
            }

            var (success, error) = await _api.UpdateAsync(id, dto);
            if (!success)
            {
                TempData["Error"] = error ?? "Failed to update expense.";
                var categories = await _categoryApi.GetAllAsync();
                var tags = await _tagApi.GetAllAsync();
                ViewBag.CategoryList = new SelectList(categories, "Name", "Name", dto.Category);
                ViewBag.Tags = tags;
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

        public async Task<IActionResult> ExportCsv(
            [FromQuery] string? category = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null)
        {
            var csvBytes = await _api.ExportCsvAsync(category, dateFrom, dateTo);
            if (csvBytes == null)
            {
                TempData["Error"] = "Failed to export data.";
                return RedirectToAction(nameof(Index));
            }
            return File(csvBytes, "text/csv", $"spendly-expenses-{DateTime.Today:yyyyMMdd}.csv");
        }

        public async Task<IActionResult> ExportReport([FromQuery] int? month = null, [FromQuery] int? year = null)
        {
            var html = await _api.ExportReportAsync(month, year);
            if (html == null)
            {
                TempData["Error"] = "Failed to generate report.";
                return RedirectToAction(nameof(Index));
            }
            return Content(html, "text/html");
        }
    }
}
