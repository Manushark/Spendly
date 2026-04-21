using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Spendly.Web.Services
{
    public class ImportApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ImportApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        private void SetAuthHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("token");
            _http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
                ? null
                : new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<CsvImportPreviewDto?> PreviewAsync(Stream fileStream, string fileName, string currency = "USD")
        {
            SetAuthHeader();
            using var content = new MultipartFormDataContent();
            var streamContent = new StreamContent(fileStream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/csv");
            content.Add(streamContent, "file", fileName);

            var response = await _http.PostAsync($"api/import/preview?currency={currency}", content);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<CsvImportPreviewDto>();
        }

        public async Task<CsvImportResultDto?> ConfirmAsync(List<CsvExpenseRow> rows)
        {
            SetAuthHeader();
            var response = await _http.PostAsJsonAsync("api/import/confirm", rows);
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<CsvImportResultDto>();
        }
    }

    public class CsvImportPreviewDto
    {
        public List<CsvExpenseRow> Rows { get; set; } = [];
        public int TotalRows { get; set; }
        public int ValidRows { get; set; }
        public int InvalidRows { get; set; }
        public List<string> Errors { get; set; } = [];
        public List<string> DetectedColumns { get; set; } = [];
    }

    public class CsvExpenseRow
    {
        public int RowNumber { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string Currency { get; set; } = "USD";
        public bool IsValid { get; set; } = true;
        public string? ValidationError { get; set; }
    }

    public class CsvImportResultDto
    {
        public int ImportedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<string> Errors { get; set; } = [];
    }
}
