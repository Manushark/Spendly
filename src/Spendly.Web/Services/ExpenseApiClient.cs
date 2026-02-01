using System.Net.Http.Json;
using Spendly.Web.Contracts.Expenses;

namespace Spendly.Web.Services
{
    public class ExpenseApiClient
    {
        private readonly HttpClient _http;

        public ExpenseApiClient(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<ExpenseDto>> GetAllAsync()
        {
            return await _http.GetFromJsonAsync<List<ExpenseDto>>("api/expenses")
                   ?? new List<ExpenseDto>();
        }

        public async Task CreateAsync(ExpenseDto dto)
        {
            await _http.PostAsJsonAsync("api/expenses", dto);
        }

        public async Task UpdateAsync(int id, ExpenseDto dto)
        {
            await _http.PutAsJsonAsync($"api/expenses/{id}", dto);
        }

        public async Task DeleteAsync(int id)
        {
            await _http.DeleteAsync($"api/expenses/{id}");
        }
    }
}
