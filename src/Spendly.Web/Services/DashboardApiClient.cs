using System.Net.Http.Headers;
using System.Net.Http.Json;
using Spendly.Web.Contracts.Dashboard;

namespace Spendly.Web.Services
{
    public class DashboardApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DashboardApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
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

        public async Task<DashboardStatsDto?> GetStatsAsync()
        {
            SetAuthHeader();

            try
            {
                var response = await _http.GetAsync("api/dashboard");

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<DashboardStatsDto>();
            }
            catch
            {
                return null;
            }
        }
    }
}
