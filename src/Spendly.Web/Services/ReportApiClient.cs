using System.Net.Http.Headers;
using System.Net.Http.Json;
using Spendly.Web.Contracts.Reports;

namespace Spendly.Web.Services
{
    /// <summary>
    /// Cliente HTTP que consume el endpoint de reportes financieros de la API.
    /// </summary>
    public class ReportApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReportApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
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

        /// <summary>
        /// Obtiene el reporte financiero del mes y año indicados.
        /// Si no se especifican, se usa el mes actual.
        /// </summary>
        public async Task<FinancialReportDto?> GetReportAsync(int? year = null, int? month = null)
        {
            SetAuthHeader();

            try
            {
                var query = (year.HasValue && month.HasValue)
                    ? $"api/reports?year={year}&month={month}"
                    : "api/reports";

                var response = await _http.GetAsync(query);

                if (!response.IsSuccessStatusCode)
                    return null;

                return await response.Content.ReadFromJsonAsync<FinancialReportDto>();
            }
            catch
            {
                return null;
            }
        }
    }
}
