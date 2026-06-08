using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Spendly.Web.Contracts.Expenses;

namespace Spendly.Web.Services
{
    public class ExpenseApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ExpenseApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
        }

        private void SetAuthHeader()
        {
            var token = Helpers.TokenHelper.GetToken(_httpContextAccessor.HttpContext);
            _http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
                ? null
                : new AuthenticationHeaderValue("Bearer", token);
        }

        private bool IsTokenExpiredOrUnauthorized(HttpResponseMessage response)
            => response.StatusCode == HttpStatusCode.Unauthorized;

        public async Task<PagedExpenseResult> GetAllAsync(
            string? category = null,
            string? search = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            decimal? minAmount = null,
            decimal? maxAmount = null,
            List<int>? tagIds = null,
            int page = 1,
            int pageSize = 10)
        {
            SetAuthHeader();

            var url = $"api/expenses?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(category))
                url += $"&category={Uri.EscapeDataString(category)}";
            if (!string.IsNullOrWhiteSpace(search))
                url += $"&search={Uri.EscapeDataString(search)}";
            if (dateFrom.HasValue)
                url += $"&dateFrom={dateFrom.Value:yyyy-MM-dd}";
            if (dateTo.HasValue)
                url += $"&dateTo={dateTo.Value:yyyy-MM-dd}";
            if (minAmount.HasValue)
                url += $"&minAmount={minAmount.Value}";
            if (maxAmount.HasValue)
                url += $"&maxAmount={maxAmount.Value}";
            if (tagIds != null && tagIds.Any())
            {
                foreach (var tagId in tagIds)
                {
                    url += $"&tagIds={tagId}";
                }
            }

            var response = await _http.GetAsync(url);

            if (IsTokenExpiredOrUnauthorized(response))
                return PagedExpenseResult.Empty();

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<PagedExpenseResult>()
                   ?? PagedExpenseResult.Empty();
        }

        public async Task<ExpenseDto?> GetByIdAsync(int id)
        {
            SetAuthHeader();
            var response = await _http.GetAsync($"api/expenses/{id}");

            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<ExpenseDto>();
        }

        public async Task<(bool Success, string? Error)> CreateAsync(ExpenseDto dto)
        {
            SetAuthHeader();
            var response = await _http.PostAsJsonAsync("api/expenses", dto);

            if (!response.IsSuccessStatusCode)
            {
                var error = await TryReadError(response);
                return (false, error);
            }

            return (true, null);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, ExpenseDto dto)
        {
            SetAuthHeader();
            var response = await _http.PutAsJsonAsync($"api/expenses/{id}", dto);

            if (!response.IsSuccessStatusCode)
            {
                var error = await TryReadError(response);
                return (false, error);
            }

            return (true, null);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            SetAuthHeader();
            var response = await _http.DeleteAsync($"api/expenses/{id}");
            return response.IsSuccessStatusCode;
        }

        public async Task<byte[]?> ExportCsvAsync(string? category = null, DateTime? dateFrom = null, DateTime? dateTo = null)
        {
            SetAuthHeader();
            var url = "api/expenses/export/csv?";
            if (!string.IsNullOrWhiteSpace(category))
                url += $"category={Uri.EscapeDataString(category)}&";
            if (dateFrom.HasValue)
                url += $"dateFrom={dateFrom.Value:yyyy-MM-dd}&";
            if (dateTo.HasValue)
                url += $"dateTo={dateTo.Value:yyyy-MM-dd}&";

            var response = await _http.GetAsync(url.TrimEnd('&', '?'));
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadAsByteArrayAsync();
        }

        public async Task<string?> ExportReportAsync(int? month = null, int? year = null, string? userTimeZone = null)
        {
            SetAuthHeader();
            var now = string.IsNullOrEmpty(userTimeZone)
                ? DateTime.UtcNow
                : TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                    TimeZoneInfo.TryFindSystemTimeZoneById(userTimeZone, out var tz) ? tz : TimeZoneInfo.Utc);
            var m = month ?? now.Month;
            var y = year ?? now.Year;

            var response = await _http.GetAsync($"api/expenses/export/report?month={m}&year={y}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadAsStringAsync();
        }

        private static async Task<string> TryReadError(HttpResponseMessage response)
        {
            try
            {
                var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return body?.Error ?? response.ReasonPhrase ?? "Unknown error";
            }
            catch
            {
                return response.ReasonPhrase ?? "Unknown error";
            }
        }
    }

    public record ApiErrorResponse(int Status, string Error);
}