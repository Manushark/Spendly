using System.Net.Http.Json;
using System.Text.Json;
using Spendly.Web.Contracts.Budgets;

namespace Spendly.Web.Services
{
    public class BudgetApiClient
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public BudgetApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<BudgetSummaryDto?> GetSummaryAsync(int year, int month)
        {
            // El AuthHeaderHandler automáticamente agrega el token
            var response = await _http.GetAsync($"api/budgets/summary?year={year}&month={month}");

            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<BudgetSummaryDto>(_jsonOptions);
        }

        public async Task<BudgetDto?> GetByIdAsync(int id)
        {
            var response = await _http.GetAsync($"api/budgets/{id}");

            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<BudgetDto>(_jsonOptions);
        }

        public async Task<(bool Ok, string? Error)> CreateAsync(CreateBudgetDto dto)
        {
            var response = await _http.PostAsJsonAsync("api/budgets", dto);

            if (response.IsSuccessStatusCode) return (true, null);

            try
            {
                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(_jsonOptions);
                return (false, error?.Error ?? "Could not create budget");
            }
            catch
            {
                return (false, "Could not create budget");
            }
        }

        public async Task<(bool Ok, string? Error)> UpdateAsync(int id, UpdateBudgetDto dto)
        {
            var response = await _http.PutAsJsonAsync($"api/budgets/{id}", dto);

            if (response.IsSuccessStatusCode) return (true, null);

            try
            {
                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>(_jsonOptions);
                return (false, error?.Error ?? "Could not update budget");
            }
            catch
            {
                return (false, "Could not update budget");
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var response = await _http.DeleteAsync($"api/budgets/{id}");
            return response.IsSuccessStatusCode;
        }
    }

}
