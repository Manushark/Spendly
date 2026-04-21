using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Spendly.Api.Extensions;
using Spendly.Application.DTOs.Import;
using Spendly.Application.UseCases.Import;

namespace Spendly.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ImportController : ControllerBase
    {
        private readonly ImportCsvUseCase _importUseCase;

        public ImportController(ImportCsvUseCase importUseCase) => _importUseCase = importUseCase;

        /// <summary>
        /// POST /api/import/preview — Upload CSV and preview parsed data
        /// </summary>
        [HttpPost("preview")]
        public async Task<IActionResult> Preview(IFormFile file, [FromQuery] string currency = "USD")
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file uploaded." });

            if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { error = "Only CSV files are supported." });

            if (file.Length > 5 * 1024 * 1024) // 5MB limit
                return BadRequest(new { error = "File size exceeds 5MB limit." });

            using var reader = new StreamReader(file.OpenReadStream());
            var content = await reader.ReadToEndAsync();

            var preview = _importUseCase.Preview(content, currency);
            return Ok(preview);
        }

        /// <summary>
        /// POST /api/import/confirm — Import validated rows
        /// </summary>
        [HttpPost("confirm")]
        public async Task<IActionResult> Confirm([FromBody] List<CsvExpenseRow> rows)
        {
            var userId = User.GetUserId();
            var result = await _importUseCase.ImportAsync(userId, rows);
            return Ok(result);
        }
    }
}
