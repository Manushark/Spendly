using System.Net.Http.Headers;
using System.Net.Http.Json;
using Spendly.Web.Contracts.RecurringExpenses;

namespace Spendly.Web.Services
{
    public class RecurringExpenseApiClient
    {
        private readonly HttpClient _http;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RecurringExpenseApiClient(HttpClient http, IHttpContextAccessor httpContextAccessor)
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

        public async Task<RecurringExpenseSummaryDto?> GetAllAsync()
        {
            SetAuthHeader();

            try
            {
                var response = await _http.GetAsync("api/recurring-expenses");
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<RecurringExpenseSummaryDto>();
            }
            catch
            {
                return null;
            }
        }

        public async Task<RecurringExpenseDto?> GetByIdAsync(int id)
        {
            SetAuthHeader();

            try
            {
                var response = await _http.GetAsync($"api/recurring-expenses/{id}");
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<RecurringExpenseDto>();
            }
            catch
            {
                return null;
            }
        }

        public async Task<(bool Ok, string? Error)> CreateAsync(CreateRecurringExpenseDto dto)
        {
            SetAuthHeader();

            try
            {
                var response = await _http.PostAsJsonAsync("api/recurring-expenses", dto);

                if (response.IsSuccessStatusCode)
                    return (true, null);

                var errorContent = await response.Content.ReadAsStringAsync();
                return (false, errorContent);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<(bool Ok, string? Error)> UpdateAsync(int id, UpdateRecurringExpenseDto dto)
        {
            SetAuthHeader();

            try
            {
                var response = await _http.PutAsJsonAsync($"api/recurring-expenses/{id}", dto);

                if (response.IsSuccessStatusCode)
                    return (true, null);

                var errorContent = await response.Content.ReadAsStringAsync();
                return (false, errorContent);
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        }

        public async Task<bool> ToggleAsync(int id, bool activate)
        {
            SetAuthHeader();

            try
            {
                var response = await _http.PostAsJsonAsync(
                    $"api/recurring-expenses/{id}/toggle",
                    new { Activate = activate });

                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(int id)
        {
            SetAuthHeader();

            try
            {
                var response = await _http.DeleteAsync($"api/recurring-expenses/{id}");
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
    }
}