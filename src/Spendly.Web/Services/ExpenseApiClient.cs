using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Spendly.Web.Contracts.Expenses;

namespace Spendly.Web.Services
{
    public class ExpenseApiClient
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public ExpenseApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<PagedExpenseResult> GetAllAsync(
            string? category = null,
            int page = 1,
            int pageSize = 10)
        {
            // El AuthHeaderHandler automáticamente agrega el token
            var url = $"api/expenses?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrWhiteSpace(category))
                url += $"&category={Uri.EscapeDataString(category)}";

            var response = await _http.GetAsync(url);

            if (response.StatusCode == HttpStatusCode.Unauthorized)
                return PagedExpenseResult.Empty();

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<PagedExpenseResult>(_jsonOptions)
                   ?? PagedExpenseResult.Empty();
        }

        public async Task<ExpenseDto?> GetByIdAsync(int id)
        {
            var response = await _http.GetAsync($"api/expenses/{id}");

            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<ExpenseDto>(_jsonOptions);
        }

        public async Task<(bool Success, string? Error)> CreateAsync(ExpenseDto dto)
        {
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
            var response = await _http.DeleteAsync($"api/expenses/{id}");
            return response.IsSuccessStatusCode;
        }

        private static async Task<string> TryReadError(HttpResponseMessage response)
        {
            try
            {
                var body = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(_jsonOptions);
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
