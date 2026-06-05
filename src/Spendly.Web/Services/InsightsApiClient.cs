using System.Net.Http.Headers;
using System.Net.Http.Json;
using Spendly.Web.Contracts.Insights;

namespace Spendly.Web.Services
{
    public class InsightsApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public InsightsApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
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

        public async Task<MonthlyInsightsDto?> GetMonthlyInsightsAsync(int? month = null, int? year = null, string? userTimeZone = null)
        {
            SetAuthHeader();
            try
            {
                // If month/year not specified, compute using user's timezone client-side
                // The API also resolves timezone but we pass explicit values to avoid double-resolution
                var now = string.IsNullOrEmpty(userTimeZone)
                    ? DateTime.UtcNow
                    : TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                        TimeZoneInfo.TryFindSystemTimeZoneById(userTimeZone, out var tz) ? tz : TimeZoneInfo.Utc);
                var m = month ?? now.Month;
                var y = year ?? now.Year;
                var response = await _http.GetAsync($"api/insights?month={m}&year={y}");
                if (!response.IsSuccessStatusCode) return null;
                return await response.Content.ReadFromJsonAsync<MonthlyInsightsDto>();
            }
            catch { return null; }
        }
    }
}
