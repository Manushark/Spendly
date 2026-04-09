using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spendly.Api.Extensions;
using Spendly.Application.UseCase.Dashboard;

namespace Spendly.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly GetDashboardStatsUseCase _getDashboardStats;

        public DashboardController(GetDashboardStatsUseCase getDashboardStats)
        {
            _getDashboardStats = getDashboardStats;
        }

        /// <summary>
        /// Devuelve todas las métricas del dashboard para el usuario autenticado.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetStats()
        {
            var stats = await _getDashboardStats.ExecuteAsync(User.GetUserId());
            return Ok(stats);
        }
    }
}
