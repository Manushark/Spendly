using System.Net.Http.Json;
using System.Text.Json;
using Spendly.Web.Contracts.Dashboard;

namespace Spendly.Web.Services
{
    public class DashboardApiClient
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public DashboardApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<DashboardStatsDto?> GetStatsAsync()
        {
            try
            {
                // El AuthHeaderHandler automáticamente agrega el token
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
