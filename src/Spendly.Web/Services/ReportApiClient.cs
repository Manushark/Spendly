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
        private readonly ILogger<ReportApiClient> _logger;

        public ReportApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor, ILogger<ReportApiClient> logger)
        {
            _http = http;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        private void SetAuthHeader()
        {
            var token = Helpers.TokenHelper.GetToken(_httpContextAccessor.HttpContext);
            _http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
                ? null
                : new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>Obtiene el reporte financiero del mes y año indicados.</summary>
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
                {
                    var body = await response.Content.ReadAsStringAsync();
                    _logger.LogError("[Reports] API returned {Status} for {Url}. Body: {Body}",
                        (int)response.StatusCode, query, body);
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<FinancialReportDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Reports] Exception calling API at {Url}", _http.BaseAddress);
                return null;
            }
        }

        /// <summary>
        /// Obtiene las transacciones individuales de una categoría y mes específicos.
        /// Usado para el modal de drill-down.
        /// </summary>
        public async Task<List<CategoryTransactionDto>> GetCategoryTransactionsAsync(
            int year, int month, string category)
        {
            SetAuthHeader();

            try
            {
                var encodedCategory = Uri.EscapeDataString(category);
                var query = $"api/reports/category-transactions?year={year}&month={month}&category={encodedCategory}";
                var response = await _http.GetAsync(query);

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("[Reports] category-transactions returned {Status}", (int)response.StatusCode);
                    return new();
                }

                return await response.Content.ReadFromJsonAsync<List<CategoryTransactionDto>>() ?? new();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Reports] Exception fetching category transactions");
                return new();
            }
        }
    }
}
