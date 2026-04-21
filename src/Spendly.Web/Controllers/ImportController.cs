using Microsoft.AspNetCore.Mvc;
using Spendly.Web.Services;

namespace Spendly.Web.Controllers
{
    public class ImportController : Controller
    {
        private readonly ImportApiClient _api;

        public ImportController(ImportApiClient api) => _api = api;

        public IActionResult Index()
        {
            var token = HttpContext.Session.GetString("token");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Preview(IFormFile file, string currency = "USD")
        {
            if (file == null || file.Length == 0)
            {
                TempData["Error"] = "Please select a CSV file.";
                return RedirectToAction(nameof(Index));
            }

            using var stream = file.OpenReadStream();
            var preview = await _api.PreviewAsync(stream, file.FileName, currency);

            if (preview == null)
            {
                TempData["Error"] = "Failed to process CSV file.";
                return RedirectToAction(nameof(Index));
            }

            // Store preview in session as JSON
            HttpContext.Session.SetString("importPreview", System.Text.Json.JsonSerializer.Serialize(preview));

            return View("Preview", preview);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm()
        {
            var previewJson = HttpContext.Session.GetString("importPreview");
            if (string.IsNullOrEmpty(previewJson))
            {
                TempData["Error"] = "Import session expired. Please upload again.";
                return RedirectToAction(nameof(Index));
            }

            var preview = System.Text.Json.JsonSerializer.Deserialize<CsvImportPreviewDto>(previewJson,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (preview == null || !preview.Rows.Any())
            {
                TempData["Error"] = "No data to import.";
                return RedirectToAction(nameof(Index));
            }

            var validRows = preview.Rows.Where(r => r.IsValid).ToList();
            var result = await _api.ConfirmAsync(validRows);

            if (result == null)
            {
                TempData["Error"] = "Import failed.";
                return RedirectToAction(nameof(Index));
            }

            HttpContext.Session.Remove("importPreview");
            TempData["Success"] = $"Successfully imported {result.ImportedCount} expenses. {result.SkippedCount} skipped.";
            return RedirectToAction(nameof(Index));
        }
    }
}
