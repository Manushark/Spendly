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
            var token = _httpContextAccessor.HttpContext?.Session.GetString("token");
            _http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(token)
                ? null
                : new AuthenticationHeaderValue("Bearer", token);
        }

        private bool IsTokenExpiredOrUnauthorized(HttpResponseMessage response)
            => response.StatusCode == HttpStatusCode.Unauthorized;

        public async Task<PagedExpenseResult> GetAllAsync(
            string? category = null,
            int page = 1,
            int pageSize = 10)
        {
            SetAuthHeader();

            var url = $"api/expenses?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(category))
                url += $"&category={Uri.EscapeDataString(category)}";

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