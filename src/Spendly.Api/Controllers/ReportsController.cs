using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spendly.Api.Extensions;
using Spendly.Application.Interfaces;
using Spendly.Application.UseCase.Reports;

namespace Spendly.Api.Controllers
{
    /// <summary>
    /// Expone los endpoints del sistema de reportes financieros avanzados.
    /// Todos los endpoints requieren autenticación JWT.
    /// </summary>
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ReportsController : ControllerBase
    {
        private readonly GetFinancialReportUseCase _getReport;
        private readonly IExpenseRepository _expenseRepo;

        public ReportsController(GetFinancialReportUseCase getReport, IExpenseRepository expenseRepo)
        {
            _getReport   = getReport;
            _expenseRepo = expenseRepo;
        }

        /// <summary>
        /// Genera el reporte financiero mensual del usuario autenticado.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetReport([FromQuery] int? year, [FromQuery] int? month)
        {
            var now         = DateTime.UtcNow;
            var targetYear  = year  ?? now.Year;
            var targetMonth = month ?? now.Month;

            if (targetMonth < 1 || targetMonth > 12)
                return BadRequest("El mes debe estar entre 1 y 12.");

            if (targetYear < 2000 || targetYear > 2100)
                return BadRequest("El año debe estar entre 2000 y 2100.");

            var report = await _getReport.ExecuteAsync(User.GetUserId(), targetYear, targetMonth);
            return Ok(report);
        }

        /// <summary>
        /// Devuelve las transacciones individuales de una categoría específica en un mes dado.
        /// Usado para el drill-down interactivo de la tabla de categorías.
        /// </summary>
        [HttpGet("category-transactions")]
        public async Task<IActionResult> GetCategoryTransactions(
            [FromQuery] int? year,
            [FromQuery] int? month,
            [FromQuery] string? category)
        {
            if (string.IsNullOrWhiteSpace(category))
                return BadRequest("La categoría es requerida.");

            var now         = DateTime.UtcNow;
            var targetYear  = year  ?? now.Year;
            var targetMonth = month ?? now.Month;

            if (targetMonth < 1 || targetMonth > 12)
                return BadRequest("El mes debe estar entre 1 y 12.");

            var periodStart = new DateTime(targetYear, targetMonth, 1);
            var periodEnd   = periodStart.AddMonths(1).AddDays(-1);

            var expenses = await _expenseRepo.GetByDateRangeAsync(User.GetUserId(), periodStart, periodEnd);

            var transactions = expenses
                .Where(e => e.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(e => e.Date)
                .Select(e => new
                {
                    id          = e.Id,
                    description = e.Description,
                    amount      = e.Amount.Value,
                    currency    = e.Amount.Currency,
                    date        = e.Date.ToString("yyyy-MM-dd"),
                    category    = e.Category
                })
                .ToList();

            return Ok(transactions);
        }
    }
}
