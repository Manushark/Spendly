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
        private readonly IExpenseRepository        _expenseRepo;
        private readonly IUserRepository            _userRepo;
        private readonly IDateTimeProvider          _dateTime;

        public ReportsController(GetFinancialReportUseCase getReport, IExpenseRepository expenseRepo, IUserRepository userRepo, IDateTimeProvider dateTime)
        {
            _getReport   = getReport;
            _expenseRepo = expenseRepo;
            _userRepo    = userRepo;
            _dateTime    = dateTime;
        }

        /// <summary>
        /// Genera el reporte financiero para el rango de fechas indicado.
        /// Acepta dateFrom/dateTo (ISO 8601) o year/month como fallback para retrocompatibilidad.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetReport(
            [FromQuery] string? dateFrom,
            [FromQuery] string? dateTo,
            [FromQuery] string? periodLabel,
            // Legacy fallback
            [FromQuery] int?    year,
            [FromQuery] int?    month)
        {
            var userId = User.GetUserId();
            var user = await _userRepo.GetByIdAsync(userId);
            var (start, end, label) = ResolveDateRange(dateFrom, dateTo, periodLabel, year, month, _dateTime, user?.TimeZone);

            if (start > end)
                return BadRequest("dateFrom must be before dateTo.");

            if ((end - start).Days > 730)
                return BadRequest("El rango no puede superar 2 años.");

            var report = await _getReport.ExecuteAsync(User.GetUserId(), start, end, label, user?.TimeZone);
            return Ok(report);
        }

        /// <summary>
        /// Devuelve las transacciones individuales de una categoría en un rango de fechas.
        /// Usado para el drill-down interactivo.
        /// </summary>
        [HttpGet("category-transactions")]
        public async Task<IActionResult> GetCategoryTransactions(
            [FromQuery] string? dateFrom,
            [FromQuery] string? dateTo,
            [FromQuery] string? category,
            // Legacy fallback
            [FromQuery] int?    year,
            [FromQuery] int?    month)
        {
            if (string.IsNullOrWhiteSpace(category))
                return BadRequest("La categoría es requerida.");

            var (start, end, _) = ResolveDateRange(dateFrom, dateTo, null, year, month, _dateTime, null);

            var expenses = await _expenseRepo.GetByDateRangeAsync(User.GetUserId(), start, end);

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

        // ── Helpers ───────────────────────────────────────────────────────────

        private static (DateTime start, DateTime end, string label) ResolveDateRange(
            string? dateFrom, string? dateTo, string? periodLabel,
            int? year, int? month,
            IDateTimeProvider dateTime, string? userTimeZone)
        {
            var now = dateTime.Now(userTimeZone);

            // Try dateFrom / dateTo first
            if (!string.IsNullOrWhiteSpace(dateFrom) && !string.IsNullOrWhiteSpace(dateTo)
                && DateTime.TryParse(dateFrom, out var parsedFrom)
                && DateTime.TryParse(dateTo,   out var parsedTo))
            {
                return (parsedFrom.Date, parsedTo.Date, periodLabel ?? "");
            }

            // Legacy: year + month
            var targetYear  = year  ?? now.Year;
            var targetMonth = month ?? now.Month;
            targetMonth = Math.Clamp(targetMonth, 1, 12);

            var start = new DateTime(targetYear, targetMonth, 1);
            var end   = start.AddMonths(1).AddDays(-1);
            return (start, end, periodLabel ?? "");
        }
    }
}
