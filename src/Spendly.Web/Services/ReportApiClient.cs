using System.Net.Http.Headers;
using System.Net.Http.Json;
using Spendly.Web.Contracts.Reports;

namespace Spendly.Web.Services
{
    /// <summary>
    /// Cliente HTTP que consume el endpoint de reportes financieros de la API.
    /// Todos los métodos usan el rango de fechas (dateFrom/dateTo) como parámetros principales.
    /// </summary>
    public class ReportApiClient
    {
        private readonly HttpClient             _http;
        private readonly IHttpContextAccessor   _httpContextAccessor;
        private readonly ILogger<ReportApiClient> _logger;

        public ReportApiClient(
            HttpClient             http,
            IHttpContextAccessor   httpContextAccessor,
            ILogger<ReportApiClient> logger)
        {
            _http                = http;
            _httpContextAccessor = httpContextAccessor;
            _logger              = logger;
        }

        private void SetAuthHeader()
        {
            var token = Helpers.TokenHelper.GetToken(_httpContextAccessor.HttpContext);
            _http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
                ? null
                : new AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Obtiene el reporte financiero para el rango de fechas indicado.
        /// </summary>
        /// <param name="dateFrom">Inicio del período (yyyy-MM-dd).</param>
        /// <param name="dateTo">Fin del período (yyyy-MM-dd).</param>
        /// <param name="periodLabel">Etiqueta del preset seleccionado (ej. "Últimos 90 días").</param>
        public async Task<FinancialReportDto?> GetReportAsync(
            DateTime dateFrom,
            DateTime dateTo,
            string   periodLabel = "")
        {
            SetAuthHeader();

            try
            {
                var from  = dateFrom.ToString("yyyy-MM-dd");
                var to    = dateTo.ToString("yyyy-MM-dd");
                var label = Uri.EscapeDataString(periodLabel);
                var query = $"api/reports?dateFrom={from}&dateTo={to}&periodLabel={label}";

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
        /// Obtiene las transacciones individuales de una categoría en un rango de fechas.
        /// Usado para el modal de drill-down.
        /// </summary>
        public async Task<List<CategoryTransactionDto>> GetCategoryTransactionsAsync(
            DateTime dateFrom,
            DateTime dateTo,
            string   category)
        {
            SetAuthHeader();

            try
            {
                var from    = dateFrom.ToString("yyyy-MM-dd");
                var to      = dateTo.ToString("yyyy-MM-dd");
                var encoded = Uri.EscapeDataString(category);
                var query   = $"api/reports/category-transactions?dateFrom={from}&dateTo={to}&category={encoded}";

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
