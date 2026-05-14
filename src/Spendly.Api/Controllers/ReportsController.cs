using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spendly.Api.Extensions;
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

        public ReportsController(GetFinancialReportUseCase getReport)
        {
            _getReport = getReport;
        }

        /// <summary>
        /// Genera el reporte financiero mensual del usuario autenticado.
        /// Si no se especifican year/month, se usa el mes actual como período por defecto.
        /// </summary>
        /// <param name="year">Año del reporte (ej: 2026). Opcional.</param>
        /// <param name="month">Mes del reporte (1–12). Opcional.</param>
        [HttpGet]
        public async Task<IActionResult> GetReport([FromQuery] int? year, [FromQuery] int? month)
        {
            var now      = DateTime.UtcNow;
            var targetYear  = year  ?? now.Year;
            var targetMonth = month ?? now.Month;

            // Validar rango de parámetros
            if (targetMonth < 1 || targetMonth > 12)
                return BadRequest("El mes debe estar entre 1 y 12.");

            if (targetYear < 2000 || targetYear > 2100)
                return BadRequest("El año debe estar entre 2000 y 2100.");

            var report = await _getReport.ExecuteAsync(User.GetUserId(), targetYear, targetMonth);
            return Ok(report);
        }
    }
}
