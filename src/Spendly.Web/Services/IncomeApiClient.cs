using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Spendly.Web.Contracts.Incomes;

namespace Spendly.Web.Services
{
    public class IncomeApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public IncomeApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
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

        public async Task<PagedIncomeResult> GetAllAsync(int page = 1, int pageSize = 10)
        {
            SetAuthHeader();
            var response = await _http.GetAsync($"api/incomes?page={page}&pageSize={pageSize}");

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return PagedIncomeResult.Empty();

            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<PagedIncomeResult>()
                   ?? PagedIncomeResult.Empty();
        }

        public async Task<IncomeDto?> GetByIdAsync(int id)
        {
            SetAuthHeader();
            var response = await _http.GetAsync($"api/incomes/{id}");
            if (!response.IsSuccessStatusCode) return null;
            return await response.Content.ReadFromJsonAsync<IncomeDto>();
        }

        public async Task<(bool Success, string? Error)> CreateAsync(IncomeDto dto)
        {
            SetAuthHeader();
            var response = await _http.PostAsJsonAsync("api/incomes", dto);
            if (!response.IsSuccessStatusCode)
            {
                var error = await TryReadError(response);
                return (false, error);
            }
            return (true, null);
        }

        public async Task<(bool Success, string? Error)> UpdateAsync(int id, IncomeDto dto)
        {
            SetAuthHeader();
            var response = await _http.PutAsJsonAsync($"api/incomes/{id}", dto);
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
            var response = await _http.DeleteAsync($"api/incomes/{id}");
            return response.IsSuccessStatusCode;
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
}
