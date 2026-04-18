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
            var token = _httpContextAccessor.HttpContext?.Session.GetString("token");
            _http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
                ? null
                : new AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<MonthlyInsightsDto?> GetMonthlyInsightsAsync(int? month = null, int? year = null)
        {
            SetAuthHeader();
            try
            {
                var now = DateTime.UtcNow;
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
