using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Spendly.Web.Contracts.Dashboard;

namespace Spendly.Web.Services
{
    public class DashboardApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public DashboardApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
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

        public async Task<DashboardStatsDto?> GetStatsAsync()
        {
            SetAuthHeader();

            try
            {
                var response = await _http.GetAsync("api/dashboard");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<DashboardStatsDto>(_jsonOptions);
            }
            catch
            {
                return null;
            }
        }
    }
}
