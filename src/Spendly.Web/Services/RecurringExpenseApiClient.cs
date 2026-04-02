using System.Net.Http.Json;
using System.Text.Json;
using Spendly.Web.Contracts.RecurringExpenses;

namespace Spendly.Web.Services
{
    public class RecurringExpenseApiClient
    {
        private readonly HttpClient _http;
        private static readonly JsonSerializerOptions _jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public RecurringExpenseApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<RecurringExpenseSummaryDto?> GetAllAsync()
        {
            try
            {
                // El AuthHeaderHandler automáticamente agrega el token
                var response = await _http.GetAsync("api/recurring-expenses");
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<RecurringExpenseSummaryDto>(_jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<RecurringExpenseDto?> GetByIdAsync(int id)
        {
            try
            {
                var response = await _http.GetAsync($"api/recurring-expenses/{id}");
                if (!response.IsSuccessStatusCode) return null;

                return await response.Content.ReadFromJsonAsync<RecurringExpenseDto>(_jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<(bool Ok, string? Error)> CreateAsync(CreateRecurringExpenseDto dto)
        {
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
