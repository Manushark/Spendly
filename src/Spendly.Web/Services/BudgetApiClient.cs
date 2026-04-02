using System.Net.Http.Headers;
using System.Net.Http.Json;
using Spendly.Web.Contracts.Budgets;

namespace Spendly.Web.Services
{
    public class BudgetApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public BudgetApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
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

        public async Task<BudgetSummaryDto?> GetSummaryAsync(int year, int month)
        {
            SetAuthHeader();
            var response = await _http.GetAsync($"api/budgets/summary?year={year}&month={month}");

            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<BudgetSummaryDto>();
        }

        public async Task<BudgetDto?> GetByIdAsync(int id)
        {
            SetAuthHeader();
            var response = await _http.GetAsync($"api/budgets/{id}");

            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<BudgetDto>();
        }

        public async Task<(bool Ok, string? Error)> CreateAsync(CreateBudgetDto dto)
        {
            SetAuthHeader();
            var response = await _http.PostAsJsonAsync("api/budgets", dto);

            if (response.IsSuccessStatusCode) return (true, null);

            try
            {
                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return (false, error?.Error ?? "Could not create budget");
            }
            catch
            {
                return (false, "Could not create budget");
            }
        }

        public async Task<(bool Ok, string? Error)> UpdateAsync(int id, UpdateBudgetDto dto)
        {
            SetAuthHeader();
            var response = await _http.PutAsJsonAsync($"api/budgets/{id}", dto);

            if (response.IsSuccessStatusCode) return (true, null);

            try
            {
                var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
                return (false, error?.Error ?? "Could not update budget");
            }
            catch
            {
                return (false, "Could not update budget");
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            SetAuthHeader();
            var response = await _http.DeleteAsync($"api/budgets/{id}");
            return response.IsSuccessStatusCode;
        }
    }

}