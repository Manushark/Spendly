using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spendly.Api.Extensions;
using Spendly.Application.DTOs.Expense;
using Spendly.Application.UseCase.CreateExpense;
using Spendly.Application.UseCase.DeleteExpense;
using Spendly.Application.UseCase.GetExpenseById;
using Spendly.Application.UseCase.ListExpenses;
using Spendly.Application.UseCases.Expenses;
using Spendly.Application.UseCases.Exports;

namespace Spendly.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ExpensesController : ControllerBase
    {
        private readonly CreateExpenseUseCase _createExpenseUseCase;
        private readonly ListExpensesUseCase _listExpensesUseCase;
        private readonly GetExpenseByIdUseCase _getExpenseByIdUseCase;
        private readonly DeleteExpenseUseCase _deleteExpenseUseCase;
        private readonly UpdateExpenseUseCase _updateExpenseUseCase;
        private readonly ExportExpensesCsvUseCase _exportCsv;
        private readonly ExportMonthlyReportUseCase _exportReport;

        public ExpensesController(
            CreateExpenseUseCase createExpenseUseCase,
            ListExpensesUseCase listExpensesUseCase,
            GetExpenseByIdUseCase getExpenseByIdUseCase,
            DeleteExpenseUseCase deleteExpenseUseCase,
            UpdateExpenseUseCase updateExpenseUseCase,
            ExportExpensesCsvUseCase exportCsv,
            ExportMonthlyReportUseCase exportReport)
        {
            _createExpenseUseCase = createExpenseUseCase;
            _listExpensesUseCase = listExpensesUseCase;
            _getExpenseByIdUseCase = getExpenseByIdUseCase;
            _deleteExpenseUseCase = deleteExpenseUseCase;
            _updateExpenseUseCase = updateExpenseUseCase;
            _exportCsv = exportCsv;
            _exportReport = exportReport;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateExpenseDto dto)
        {
            var userId = User.GetUserId();
            var expenseId = await _createExpenseUseCase.ExecuteAsync(userId, dto);
            return CreatedAtAction(nameof(GetById), new { id = expenseId }, new { id = expenseId, message = "Expense created successfully" });
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? category,
            [FromQuery] string? search,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] decimal? minAmount,
            [FromQuery] decimal? maxAmount,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page <= 0 || pageSize <= 0)
                return BadRequest("Page and pageSize must be greater than zero.");

            if (pageSize > 100)
                return BadRequest("pageSize cannot exceed 100.");

            var userId = User.GetUserId();
            var result = await _listExpensesUseCase.ExecuteAsync(userId, category, search, dateFrom, dateTo, minAmount, maxAmount, page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var userId = User.GetUserId();
            var result = await _getExpenseByIdUseCase.ExecuteAsync(userId, id);

            if (result == null) return NotFound();

            return Ok(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.GetUserId();
            var deleted = await _deleteExpenseUseCase.ExecuteAsync(userId, id);

            if (!deleted) return NotFound();

            return NoContent();
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateExpenseDto dto)
        {
            var userId = User.GetUserId();
            await _updateExpenseUseCase.ExecuteAsync(userId, id, dto);
            return NoContent();
        }

        /// <summary>
        /// GET /api/expenses/export/csv
        /// </summary>
        [HttpGet("export/csv")]
        public async Task<IActionResult> ExportCsv(
            [FromQuery] string? category = null,
            [FromQuery] DateTime? dateFrom = null,
            [FromQuery] DateTime? dateTo = null)
        {
            var userId = User.GetUserId();
            var csvBytes = await _exportCsv.ExecuteAsync(userId, category, dateFrom, dateTo);
            return File(csvBytes, "text/csv", $"spendly-expenses-{DateTime.UtcNow:yyyyMMdd}.csv");
        }

        /// <summary>
        /// GET /api/expenses/export/report?month=4&year=2026
        /// </summary>
        [HttpGet("export/report")]
        public async Task<IActionResult> ExportReport(
            [FromQuery] int? month = null,
            [FromQuery] int? year = null)
        {
            var userId = User.GetUserId();
            var now = DateTime.UtcNow;
            var reportHtml = await _exportReport.ExecuteAsync(userId, year ?? now.Year, month ?? now.Month);
            return Content(reportHtml, "text/html");
        }
    }
}
